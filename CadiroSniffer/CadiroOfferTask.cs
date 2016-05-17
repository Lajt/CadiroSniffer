﻿using Loki.Bot;
using Loki.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Windows.Controls;
using Loki.Game;
using CommunityLib;
using Loki.Game.Objects;
using Buddy.Coroutines;
using CadiroSniffer.Helpers;
using CadiroSniffer.Classes;
using Loki.Bot.Pathfinding;

namespace CadiroSniffer
{
    class CadiroOfferTask : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Name => "CadiroOfferTask";
        public string Description => "Sniff that godly offers.";
        public string Author => "Lajt";
        public string Version => "0.0.9.0";

        private bool _skip = false;
        private Vector2i cadiroPos = Vector2i.Zero;

        #region Implementation of IRunnable
        
        public void Start()
        {
            Log.DebugFormat($"[{Name}] Start");
        }

        public void Tick()
        {
        }

        public void Stop()
        {
            Log.DebugFormat($"[{Name}] Stop");
        }

        #endregion

        #region Implementation of ILogic

        public async Task<bool> Logic(string type, params dynamic[] param)
        {
            if (type == "core_area_changed_event")
            {
                _skip = false;
                cadiroPos = Vector2i.Zero;
            }

            if (_skip)
                return false;

            if (type != "task_execute")
                return false;

            if (LokiPoe.Me.IsDead || LokiPoe.Me.IsInTown || LokiPoe.Me.IsInHideout)
                return false;

            //var monsters = LokiPoe.ObjectManager.GetObjectsByType<Monster>().Where(m => !m.IsDead && m.IsHostile && m.Distance < 50).ToList();
            if (Lajt.NearMonsters())
                return false;

            var cadiro = LokiPoe.ObjectManager.Objects.FirstOrDefault(o => o.Name.Equals("Cadiro Perandus"));

            if (cadiro != null)
            {
                if (cadiro.Distance > 25)
                {
                    Log.DebugFormat($"[{Name}] Moving towards Cadiro.");
                    await Navigation.MoveToLocation(cadiro.Position, 25, 15000, 
                        () => Lajt.NearMonsters());
                    return true;
                }

                await Coroutines.CloseBlockingWindows();
                var res = await Coroutines.InteractWith(cadiro);
                Log.DebugFormat($"[{Name}] Interaction returned: {res}");
                if (!res)
                    return true;
                
                // click on cadiro offer in npc dialog
                await Coroutine.Sleep(500);
                LokiPoe.InGameState.NpcDialogUi.CadirosOffer();
                await Coroutine.Sleep(500);

                if (LokiPoe.InGameState.CadiroOfferUi.IsOpened)
                {
                    // get info about item
                    List<KeyValuePair<string, int>> lista;
                    bool canAfford;
                    var item = LokiPoe.InGameState.CadiroOfferUi.InventoryControl.Inventory.Items.FirstOrDefault();
                    // temp fix because item.StackCount isnt accesable after purchase
                    int stackCount = item.StackCount;
                    LokiPoe.InGameState.CadiroOfferUi.GetItemCost(out lista, out canAfford);
                    var price = lista.Where(t => t.Key.Equals("Perandus Coin")).FirstOrDefault().Value;

                    Log.DebugFormat($"[{Name}] Cadiro's offer is ----> {item.StackCount}x {item.FullName} for {price} Perandus Coins.");
                    
                    // we found cadiro in this instance so no reason to talk to him again
                    _skip = true;

                    bool shouldAccept = false;
                    bool shouldStop = false;
                    int maxPrice = 0;
                    
                    // check item list
                    var pos = CadiroSnifferSettings.Instance.CommonCollection.Where(
                            i => !string.IsNullOrEmpty(i.Name)
                        && i.Name.Equals(item.FullName)).FirstOrDefault();

                    if (pos != null)
                    {
                        Log.DebugFormat($"[{Name}] Item is on specific list.");
                        shouldAccept = true;
                        shouldStop = pos.StopOnFailed;
                        maxPrice = pos.MaxPrice;
                    }

                    if(item.IsCurrencyType && CadiroSnifferSettings.Instance.AutoCurrency)
                    {
                        Log.DebugFormat($"[{Name}] Item is currency and autobuy is enabled.");
                        shouldAccept = true;
                    }

                    // no specific items so check specific types
                    if (!shouldAccept)
                    {
                        Log.DebugFormat($"[{Name}] Checking for specific types.");
                        if (item.IsAmuletType && CadiroSnifferSettings.Instance.AmuletBuy)
                        {
                            maxPrice = CadiroSnifferSettings.Instance.AmuletPrice;
                            shouldAccept = true;
                        }
                        if (item.IsRingType && CadiroSnifferSettings.Instance.RingBuy)
                        {
                            maxPrice = CadiroSnifferSettings.Instance.RingPrice;
                            shouldAccept = true;
                        }
                        if (item.IsJewelType && CadiroSnifferSettings.Instance.JewelBuy)
                        {
                            maxPrice = CadiroSnifferSettings.Instance.JewelPrice;
                            shouldAccept = true;
                        }
                        if (item.IsMapType && CadiroSnifferSettings.Instance.MapBuy)
                        {
                            maxPrice = CadiroSnifferSettings.Instance.MapPrice;
                            shouldAccept = true;
                        }
                        if(shouldAccept)
                            Log.DebugFormat($"[{Name}] Specific type in trade window.");
                    }

                    if (shouldAccept)
                    {
                        // check if enough space in inv
                        // buy item, maxprice = 0 is unlimited
                        Log.DebugFormat($"[{Name}] Trying to buy: {item.FullName}");
                        if (canAfford && (price <= maxPrice || maxPrice == 0))
                        {
                            LokiPoe.InGameState.CadiroOfferUi.Accept();
                            await Coroutine.Sleep(100);
                            LokiPoe.InGameState.NpcDialogUi.Continue();
                            // check if success
                            Alerter.Notify(item, price, Alerter.Status.Success, stackCount);
                            Log.DebugFormat($"[{Name}] Item should be purchased at this state.");
                            return false;
                        }
                        else
                        {
                            if (shouldStop)
                            {
                                Alerter.Notify(item, price, Alerter.Status.Stop, stackCount);
                                Log.DebugFormat($"[{Name}] Stopping bot... We found specific item: {item.FullName}");
                                BotManager.Stop();
                                return false;
                            }
                        }
                    }

                    Alerter.Notify(item, price, Alerter.Status.Info, stackCount);

                    Log.DebugFormat($"[{Name}] Trying to decline trade...");
                    //Decline trade
                    LokiPoe.InGameState.CadiroOfferUi.Decline();
                    _skip = true;
                    await Coroutines.ReactionWait();
                    while(LokiPoe.InGameState.CadiroOfferUi.IsOpened)
                    {
                        Log.ErrorFormat($"[{Name}] Cadiro window is still open.");
                        LokiPoe.InGameState.CadiroOfferUi.Decline();
                        await Coroutine.Sleep(200);
                    }
                    Log.DebugFormat($"[{Name}] Cadiro trade window closed.");
                }
                else
                {
                    Log.ErrorFormat($"[{Name}] Cadiro offer is NOT OPENED :(");
                    return true;
                }
            }
            else if(cadiroPos != Vector2i.Zero)
            {
                Log.ErrorFormat($"[{Name}] No Cadiro in our sight. Moving to place where we first seen him.");
                await Navigation.MoveToLocation(ExilePather.FastWalkablePositionFor(cadiroPos),25,50000,
                    () => Lajt.NearMonsters());
                return true;
            }

            return false;
        }

        
        public object Execute(string name, params dynamic[] param)
        {
            return null;
        }

        #endregion

    }
}

