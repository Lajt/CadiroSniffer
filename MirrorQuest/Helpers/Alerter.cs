using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityLib;
using Loki.Game.Objects;
using MirrorQuest.Classes;
using Exilebuddy;
using Loki.Game;
using System.Media;
using System.IO;
using Loki.Bot;

namespace MirrorQuest.Helpers
{
    public static class Alerter
    {
        public static bool MobileEnabled => 
            MirrorQuestSettings.Instance.PushoverEnabled || 
            MirrorQuestSettings.Instance.PushbulletEnabled || 
            MirrorQuestSettings.Instance.ProwlEnabled;

        public static void SendNotification(string text, string title = "Cadiro call")
        {
            if(MirrorQuestSettings.Instance.PushoverEnabled && 
                !string.IsNullOrWhiteSpace(MirrorQuestSettings.Instance.PushoverApiKey) &&
                !string.IsNullOrWhiteSpace(MirrorQuestSettings.Instance.PushoverUserKey))
            {
                var req = Notifications.Pushover(
                            MirrorQuestSettings.Instance.PushoverApiKey,
                            MirrorQuestSettings.Instance.PushoverUserKey,
                            text,
                            title,
                            Notifications.NotificationPriority.Normal);

                if (req != Results.NotificationResult.None)
                    MirrorQuest.Log.ErrorFormat($"[MirrorQuest] Network Error: {req}");
            }

            if(MirrorQuestSettings.Instance.PushbulletEnabled && 
                !string.IsNullOrWhiteSpace(MirrorQuestSettings.Instance.PushbulletKey))
            {
                var req = Notifications.Pushbullet(
                            MirrorQuestSettings.Instance.PushbulletKey,
                            text,
                            title);

                if (req != Results.NotificationResult.None)
                    MirrorQuest.Log.ErrorFormat($"[MirrorQuest] Network Error: {req}");
            }

            if(MirrorQuestSettings.Instance.ProwlEnabled && 
                !string.IsNullOrWhiteSpace(MirrorQuestSettings.Instance.ProwlKey))
            {
                var req = Notifications.Prowl(MirrorQuestSettings.Instance.ProwlKey,
                            "MirrorQuest",
                            title,
                            text,
                            Notifications.NotificationPriority.Normal);

                if (req != Results.NotificationResult.None)
                    MirrorQuest.Log.ErrorFormat($"[MirrorQuest] Network Error: {req}");
            }
            
        }

        public static void PlaySound(string sound)
        {
            // success + stop + all
            sound = sound + ".wav";
            var path = Path.Combine(ThirdPartyLoader.GetInstance("MirrorQuest").ContentPath, "Sounds");
            path = Path.Combine(path, sound);
            if (File.Exists(path))
            {
                MirrorQuest.Log.DebugFormat($"[MirrorQuest] Trying to play {sound} sound...");
                var sp = new SoundPlayer(path);
                sp.Play();
                return;
            }
            MirrorQuest.Log.ErrorFormat($"[MirrorQuest] {sound} file dosent exist.");
            MirrorQuest.Log.ErrorFormat($"[MirrorQuest] Full path: {path}");
        }

        public static void Notify(Item item, int price, Status status, int stackCount = 1)
        {
            MirrorQuest.Log.DebugFormat("Starting Notify...");

            string temp = $"{stackCount}x {item.FullName} for {price} Perandus coins.";
            string st = status.ToString();
            string offerResult = "Declined";
            bool mobileNotify = false;
            string soundNotify = "";

            if (MirrorQuestSettings.Instance.MobileNotifyAll)
                mobileNotify = true;

            if (MirrorQuestSettings.Instance.SoundNotifyAll)
                soundNotify = "all";

            switch (status)
            {
                case Status.None:
                case Status.Info:
                    break;
                case Status.Success:
                    temp = $"Successfully purchased: {temp}";
                    offerResult = "Accepted";
                    if (MirrorQuestSettings.Instance.MobileNotifySuccess)
                        mobileNotify = true;
                    if (MirrorQuestSettings.Instance.SoundNotifySuccess)
                        soundNotify = "success";
                    break;
                case Status.Failed:
                    temp = $"Purchase failed: {temp}";
                    break;
                case Status.Stop:
                    temp = $"Bot stopped! Offer: {temp}";
                    if (MirrorQuestSettings.Instance.MobileNotifyBotStop)
                        mobileNotify = true;
                    if (MirrorQuestSettings.Instance.SoundNotifyBotStop)
                        soundNotify = "stop";
                    break;

            }

            // add offer info to offer history
            // Thanks Fujiyama for idea how to live update :)
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                MirrorQuestSettings.Instance.OfferCollection.Add(
                    new Offer()
                    {
                        timeOfOffer = DateTime.Now,
                        itemName = item.FullName,
                        price = price,
                        qty = stackCount,
                        status = st,
                        result = offerResult,
                        location = LokiPoe.LocalData.WorldArea.Name
                    });
            });

            if(MobileEnabled && mobileNotify)
                SendNotification(temp, st);
            if (!string.IsNullOrEmpty(soundNotify))
                PlaySound(soundNotify);
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
