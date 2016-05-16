using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityLib;
using Loki.Game.Objects;
using CadiroSniffer.Classes;
using Exilebuddy;
using Loki.Game;

namespace CadiroSniffer.Helpers
{
    public static class Alerter
    {
        public static bool MobileEnabled => 
            CadiroSnifferSettings.Instance.PushoverEnabled || 
            CadiroSnifferSettings.Instance.PushbulletEnabled || 
            CadiroSnifferSettings.Instance.ProwlEnabled;

        public static void SendNotification(string text, string title = "Cadiro call")
        {
            if(CadiroSnifferSettings.Instance.PushoverEnabled && 
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.PushoverApiKey) &&
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.PushoverUserKey))
            {
                var req = Notifications.Pushover(
                            CadiroSnifferSettings.Instance.PushoverApiKey,
                            CadiroSnifferSettings.Instance.PushoverUserKey,
                            text,
                            title,
                            Notifications.NotificationPriority.Normal);

                if (req != Results.NotificationResult.None)
                    CadiroSniffer.Log.ErrorFormat($"[CadiroSniffer] Network Error: {req}");
            }

            if(CadiroSnifferSettings.Instance.PushbulletEnabled && 
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.PushbulletKey))
            {
                var req = Notifications.Pushbullet(
                            CadiroSnifferSettings.Instance.PushbulletKey,
                            text,
                            title);

                if (req != Results.NotificationResult.None)
                    CadiroSniffer.Log.ErrorFormat($"[CadiroSniffer] Network Error: {req}");
            }

            if(CadiroSnifferSettings.Instance.ProwlEnabled && 
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.ProwlKey))
            {
                var req = Notifications.Prowl(CadiroSnifferSettings.Instance.ProwlKey,
                            "CadiroSniffer",
                            title,
                            text,
                            Notifications.NotificationPriority.Normal);

                if (req != Results.NotificationResult.None)
                    CadiroSniffer.Log.ErrorFormat($"[CadiroSniffer] Network Error: {req}");
            }
            
        }

        public static void Notify(Item item, int price, Status status)
        {
            string temp = $"{item.StackCount}x {item.FullName} for {price} Perandus coins.";
            string st = status.ToString();
            string offerResult = "Declined";
            bool mobileNotify = false;

            if (CadiroSnifferSettings.Instance.NotifyAll)
                mobileNotify = true;

            switch (status)
            {
                case Status.None:
                case Status.Info:
                    break;
                case Status.Success:
                    temp = $"Successfully purchased: {temp}";
                    offerResult = "Accepted";
                    if (CadiroSnifferSettings.Instance.NotifySuccess)
                        mobileNotify = true;
                    break;
                case Status.Failed:
                    temp = $"Purchase failed: {temp}";
                    break;
                case Status.Stop:
                    temp = $"Bot stopped! Offer: {temp}";
                    if (CadiroSnifferSettings.Instance.NotifyBotStop)
                        mobileNotify = true;
                    break;

            }

            // add offer info to offer history
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                CadiroSnifferSettings.Instance.OfferCollection.Add(
                    new Offer()
                    {
                        timeOfOffer = DateTime.Now,
                        itemName = item.FullName,
                        price = price,
                        qty = item.StackCount,
                        status = st,
                        result = offerResult,
                        location = LokiPoe.LocalData.WorldArea.Name
                    });
            });

            if(MobileEnabled && mobileNotify)
                SendNotification(temp, st);
        }

        public enum Status
        {
            None,
            Success,
            Failed,
            Info,
            Stop
        }
    }
}
