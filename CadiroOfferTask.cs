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
using CadiroSniffer.Helpers;
using CadiroSniffer.Classes;

namespace CadiroSniffer
{
    class CadiroOfferTask : ITask
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Name => "CadiroOfferTask";
        public string Description => "Sniff that godly offers.";
        public string Author => "Lajt";
        public string Version => "0.0.1.0";

        private bool _skip = false;

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
            }

            if (_skip)
                return false;

            if (type != "task_execute")
                return false;

            if (LokiPoe.Me.IsDead || LokiPoe.Me.IsInTown || LokiPoe.Me.IsInHideout)
                return false;

            var monsters = LokiPoe.ObjectManager.GetObjectsByType<Monster>().Where(m => !m.IsDead && m.IsHostile && m.Distance < 50).ToList();
            if (monsters.Any())
                return false;

            var cadiro = LokiPoe.ObjectManager.Objects.FirstOrDefault(o => o.Name.Equals("Cadiro Perandus"));

            if (cadiro != null)
            {
                if (cadiro.Distance > 15)
                {
                    Log.DebugFormat($"[{Name}] Moving towards Cadiro.");
                    await Navigation.MoveToLocation(cadiro.Position, 25, 15000, () => false);
                    return true;
                }

                await Coroutines.CloseBlockingWindows();
                var res = await Coroutines.InteractWith(cadiro);
                Log.ErrorFormat($"[{Name}] Interaction returned: {res}");
                
                await Coroutine.Sleep(500);

                LokiPoe.InGameState.NpcDialogUi.CadirosOffer();

                await Coroutine.Sleep(500);

                if (LokiPoe.InGameState.CadiroOfferUi.IsOpened)
                {
                    List<KeyValuePair<string, int>> lista;
                    bool canAfford;
                    var item = LokiPoe.InGameState.CadiroOfferUi.InventoryControl.Inventory.Items.FirstOrDefault();
                    LokiPoe.InGameState.CadiroOfferUi.GetItemCost(out lista, out canAfford);
                    var price = lista.Where(t => t.Key.Equals("Perandus Coin")).FirstOrDefault().Value;

                    Log.ErrorFormat($"[{Name}] Cadiro's offer is ----> {item.StackCount}x {item.FullName} for {price} Perandus Coins.");
                    
                    
                    _skip = true;

                    if (item.IsCurrencyType && canAfford)
                    {
                        LokiPoe.InGameState.CadiroOfferUi.Accept();
                        //LokiPoe.InGameState.NpcDialogUi.Continue();
                        _skip = true;
                        Log.DebugFormat($"Purchase: > {item.StackCount}x {item.FullName} for {price} Perandus Coins.");
                        if (CadiroSnifferSettings.Instance.NotifyCurrency)
                        {
                            // check if success by looking into inventory
                            Alerter.Notify(item, price, Alerter.Status.Success);
                        }
                        return false;
                    }
                    var godlike = CadiroSnifferSettings.Instance.DGItemsCollection.Where(
                        i => !string.IsNullOrEmpty(i.Name) && i.Name.Equals(item.FullName)).Any();

                    if (godlike)
                    {
                        bool success = false;
                        string status = "Bot stopped! Not enough coins. Come and get it!";
                        if (canAfford)
                        {
                            LokiPoe.InGameState.CadiroOfferUi.Accept();
                            // check if success by looking into inventory
                            success = true;
                        }
                        if (success)
                        {
                            if (CadiroSnifferSettings.Instance.NotifyGodlike)
                            {
                                status = "Successfuly purchased!";
                                Log.DebugFormat($"{item.FullName} {status}");
                                Alerter.Notify(item, price, Alerter.Status.Success, true);
                            }
                            _skip = true;
                            return false;
                        }
                        else
                        {
                            Log.DebugFormat($"{item.FullName} {status}");
                            Alerter.Notify(item, price, Alerter.Status.Stop, true);
                            BotManager.Stop();
                            return false;
                        }

                    }
                    if (CadiroSnifferSettings.Instance.NotifyAll)
                    {
                        Alerter.Notify(item, price, Alerter.Status.Info);
                    }
                    
                    LokiPoe.InGameState.CadiroOfferUi.Decline();

                }
                else
                {
                    Log.ErrorFormat($"[{Name}] Cadiro offer is NOT OPENED :(");
                    return true;
                }
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

