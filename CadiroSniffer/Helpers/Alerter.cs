using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityLib;
using Loki.Game.Objects;
using CadiroSniffer.Classes;
using Exilebuddy;

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
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.ApiKey) &&
                !string.IsNullOrWhiteSpace(CadiroSnifferSettings.Instance.UserKey))
            {
                var req = Notifications.Pushover(
                            CadiroSnifferSettings.Instance.ApiKey,
                            CadiroSnifferSettings.Instance.UserKey,
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

        public static void Notify(Item item, int price, Status status, bool godlike = false)
        {
            string temp = $"{item.StackCount}x {item.FullName} for {price} Perandus coins.";
            string st = status.ToString();
            string offerStatus = "Declined";

            if (godlike)
                st = "GODLIKE!";

            switch (status)
            {
                case Status.None:
                case Status.Info:
                    break;
                case Status.Success:
                    temp = $"Successfully purchased: {temp}";
                    offerStatus = "Accepted";
                    break;
                case Status.Failed:
                    temp = $"Purchase failed: {temp}";
                    break;
                case Status.Stop:
                    temp = $"Bot stopped! Offer: {temp}";
                    break;

            }
            Offer offer = new Offer()
            {
                timeOfOffer = DateTime.Now,
                itemName = item.FullName,
                price = price,
                type = st,
                status = offerStatus
            };

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                CadiroSnifferSettings.Instance.OfferCollection.Add(offer);
            });

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
