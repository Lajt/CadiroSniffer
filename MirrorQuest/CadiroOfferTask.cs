using Loki.Bot;
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
using Default.EXtensions;
using MirrorQuest.Helpers;
using MirrorQuest.Classes;
using Loki.Bot.Pathfinding;
using Loki.Game.GameData;

namespace MirrorQuest
{
    class CadiroOfferTask : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Name => "CadiroOfferTask";
        public string Description => "Task to handle Cadiro offers.";
        public string Author => "Lajt";
        public string Version => "1.0.0";

        private bool _skip = false;
        private Vector2i _cadiroPos = Vector2i.Zero;

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
        
        /// <inheritdoc />
        public MessageResult Message(Message message)
        {
            if (message.Id == "area_changed_event")
            {
                _skip = false;
                _cadiroPos = Vector2i.Zero;

                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }

        /// <inheritdoc />
        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        /// <inheritdoc />
        public async Task<bool> Run()
        {
            
            if (_skip)
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
                    await Navigation.MoveToLocation(cadiro.Position, 25, 10000,
                        () => Lajt.NearMonsters());
                    return true;
                }

                await Coroutines.CloseBlockingWindows();
                var res = await Coroutines.InteractWith(cadiro);
                Log.DebugFormat($"[{Name}] Interaction returned: {res}");
                if (!res)
                    return true;

                // click on cadiro offer in npc dialog
                await Coroutines.LatencyWait(5);
                LokiPoe.InGameState.NpcDialogUi.CadirosOffer();
                await Coroutines.LatencyWait(5);

                if (LokiPoe.InGameState.CadiroOfferUi.IsOpened)
                {
                    // get info about item
                    List<KeyValuePair<string, int>> lista;
                    bool canAfford;
                    var item = LokiPoe.InGameState.CadiroOfferUi.InventoryControl.Inventory.Items.FirstOrDefault();
                    // item.StackCount isnt accesable after purchase so cache that info into var
                    int stackCount = item.StackCount;
                    LokiPoe.InGameState.CadiroOfferUi.GetItemCost(out lista, out canAfford);
                    var price = lista.Where(t => t.Key.Equals("Perandus Coin")).FirstOrDefault().Value;

                    Log.DebugFormat($"[{Name}] Cadiro's offer is ----> {item.StackCount}x {item.FullName} for {price} Perandus Coins.");

                    bool shouldAccept = false;
                    bool shouldStop = false;
                    int maxPrice = 0;

                    // check item list
                    var pos = MirrorQuestSettings.Instance.CommonCollection.Where(
                        i => !string.IsNullOrEmpty(i.Name)
                             && i.Name.Equals(item.FullName)).FirstOrDefault();

                    if (pos != null)
                    {
                        Log.DebugFormat($"[{Name}] Item is on specific list.");
                        shouldAccept = true;
                        shouldStop = pos.StopOnFailed;
                        maxPrice = pos.MaxPrice;
                    }

                    if (item.Rarity == Rarity.Currency && MirrorQuestSettings.Instance.AutoCurrency)
                    {
                        Log.DebugFormat($"[{Name}] Item is currency and autobuy is enabled.");
                        shouldAccept = true;
                    }

                    // no specific items so check specific types
                    if (!shouldAccept)
                    {
                        Log.DebugFormat($"[{Name}] Checking for specific types.");
                        if (item.HasMetadataFlags(MetadataFlags.Amulets) && MirrorQuestSettings.Instance.AmuletBuy)
                        {
                            maxPrice = MirrorQuestSettings.Instance.AmuletPrice;
                            shouldAccept = true;
                        }
                        if (item.HasMetadataFlags(MetadataFlags.Rings) && MirrorQuestSettings.Instance.RingBuy)
                        {
                            maxPrice = MirrorQuestSettings.Instance.RingPrice;
                            shouldAccept = true;
                        }
                        if (item.HasMetadataFlags(MetadataFlags.Jewels) && MirrorQuestSettings.Instance.JewelBuy)
                        {
                            maxPrice = MirrorQuestSettings.Instance.JewelPrice;
                            shouldAccept = true;
                        }
                        if (item.HasMetadataFlags(MetadataFlags.Maps) && MirrorQuestSettings.Instance.MapBuy)
                        {
                            maxPrice = MirrorQuestSettings.Instance.MapPrice;
                            shouldAccept = true;
                        }
                        if (shouldAccept)
                            Log.DebugFormat($"[{Name}] Specific type in trade window.");
                    }

                    if (shouldAccept)
                    {
                        // check if space in inventory
                        var freeSpace = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory.CanFitItem(item);

                        // buy item, maxprice = 0 is unlimited
                        Log.DebugFormat($"[{Name}] Trying to buy: {item.FullName}");
                        if (canAfford && (price <= maxPrice || maxPrice == 0) && freeSpace)
                        {
                            LokiPoe.InGameState.CadiroOfferUi.Accept();
                            await Coroutines.LatencyWait(2);
                            LokiPoe.InGameState.NpcDialogUi.Continue();

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
                    LokiPoe.InGameState.CadiroOfferUi.Decline();

                    // Skip cadiro in this instance because we don't need to interact with him anymore
                    _skip = true;
                    await Coroutines.ReactionWait();

                    while (LokiPoe.InGameState.CadiroOfferUi.IsOpened)
                    {
                        Log.ErrorFormat($"[{Name}] Cadiro window is still open.");
                        LokiPoe.InGameState.CadiroOfferUi.Decline();
                        await Coroutines.LatencyWait(2);
                    }
                    Log.DebugFormat($"[{Name}] Cadiro trade window closed.");
                }
                else
                {
                    Log.ErrorFormat($"[{Name}] Cadiro offer is NOT OPENED :(");
                    return true;
                }
            }
            else if (_cadiroPos != Vector2i.Zero)
            {
                Log.ErrorFormat($"[{Name}] No Cadiro in our sight. Moving to place where we first seen him.");
                await Navigation.MoveToLocation(ExilePather.FastWalkablePositionFor(_cadiroPos), 25, 50000,
                    () => Lajt.NearMonsters());
                return true;
            }

            return false;
        }
    }
}

