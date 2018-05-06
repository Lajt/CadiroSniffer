using Loki.Game;
using Loki.Game.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Buddy.Coroutines;
using Loki.Bot;
using Loki.Common;

namespace MirrorQuest.Helpers
{
    public static class Lajt
    {
        public static bool NearMonsters(float distance = 50)
            => LokiPoe.ObjectManager.GetObjectsByType<Monster>().Where(m => !m.IsDead && m.IsHostile && m.Distance < distance).ToList().Any();

        public static async Task<bool> MoveToLocation(Vector2i position, int stopDistance, int timeout, Func<bool> stopCondition)
        {
            var sw = Stopwatch.StartNew();
            var dsw = Stopwatch.StartNew();

            var msg1 = new Message("GetDoAdjustments");
            var res1 = PlayerMover.Instance.Message(msg1);
            var da = msg1.GetOutput<bool?>();

            while (LokiPoe.MyPosition.Distance(position) > stopDistance)
            {
                if (LokiPoe.Me.IsDead)
                {
                    MirrorQuest.Log.ErrorFormat("[MoveToLocation] The player is dead.");
                    var msg = new Message("SetDoAdjustments", null, da);
                    var res = PlayerMover.Instance.Message(msg);
                    return false;
                }

                if (sw.ElapsedMilliseconds > timeout)
                {
                    MirrorQuest.Log.ErrorFormat("[MoveToLocation] Timeout.");
                    var msg = new Message("SetDoAdjustments", null, da);
                    var res = PlayerMover.Instance.Message(msg);
                    return false;
                }

                if (stopCondition())
                    break;

                if (dsw.ElapsedMilliseconds > 100)
                {
                    MirrorQuest.Log.DebugFormat(
                        "[MoveToLocation] Now moving towards {0}. We have been performing this task for {1}.",
                        position,
                        sw.Elapsed);
                    dsw.Restart();
                }

                if (!PlayerMover.MoveTowards(position))
                    MirrorQuest.Log.ErrorFormat("[MoveToLocation] MoveTowards failed for {0}.", position);

                await Coroutine.Yield();
            }

            msg1 = new Message("SetDoAdjustments", null, da);
            res1 = PlayerMover.Instance.Message(msg1);
            await Coroutines.FinishCurrentAction();

            return true;
        }

    }

    public static class Notifications
    {

        /// <summary>
        /// Sends a notification using Pushover service
        /// </summary>
        /// <param name="token">Application Token/Key</param>
        /// <param name="apikey"></param>
        /// <param name="description">Message to send</param>
        /// <param name="ev">Title</param>
        /// <param name="p">Notification Priority</param>
        /// <returns></returns>
        public static Results.NotificationResult Pushover(string token, string apikey, string description, string ev, NotificationPriority p)
        {
            if (!CheckApiToken(token))
                return Results.NotificationResult.TokenError;

            if (!CheckApiToken(apikey))
                return Results.NotificationResult.ApiKeyError;

            string url = "https://api.pushover.net/1/messages.json";
            url += "?token=" + HttpUtility.UrlEncode(token.Trim()) +
                   "&user=" + HttpUtility.UrlEncode(apikey.Trim()) +
                   "&message=" + HttpUtility.UrlEncode(description) +
                   "&title=" + HttpUtility.UrlEncode(ev);

            return Send(url);
        }

        /// <summary>
        /// Sends a notification using Prowl service (iPhone)
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="pluginName">Name of the Plugin</param>
        /// <param name="ev">Event name</param>
        /// <param name="description">Event description</param>
        /// <param name="priority">Notification Priority</param>
        /// <returns>NotificationResult enum entry</returns>
        public static Results.NotificationResult Prowl(string apikey, string pluginName, string ev, string description, NotificationPriority priority)
        {
            if (!CheckApiToken(apikey))
                return Results.NotificationResult.ApiKeyError;

            string url = "https://prowl.weks.net/publicapi/add";
            url += "?apikey=" + HttpUtility.UrlEncode(apikey.Trim()) +
                   "&application=" + HttpUtility.UrlEncode(pluginName) +
                   "&description=" + HttpUtility.UrlEncode(description) +
                   "&event=" + HttpUtility.UrlEncode(ev) +
                   "&priority=" + HttpUtility.UrlEncode(priority.ToString());

            return Send(url);
        }

        /// <summary>
        /// Sends a notification using Pushbullet service
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="description">Body of the notification</param>
        /// <param name="ev">Title of the notification</param>
        /// <returns></returns>
        public static Results.NotificationResult Pushbullet(string apikey, string description, string ev)
        {
            if (!CheckApiToken(apikey))
                return Results.NotificationResult.ApiKeyError;

            const string url = "https://api.pushbullet.com/api/pushes";
            var myCreds = new CredentialCache { { new Uri(url), "Basic", new NetworkCredential(apikey, "") } };
            string postData = "type=note" +
                              "&body=" + HttpUtility.UrlEncode(description) +
                              "&title=" + HttpUtility.UrlEncode(ev);

            return Send(url, postData, myCreds);
        }

        //returns the result to parse it in the main function
        private static Results.NotificationResult Send(string uri, string postData = "", CredentialCache creds = null)
        {
            WebRequest request = WebRequest.Create(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.Method = "POST";

            if (creds != null)
                request.Credentials = creds;

            if (!string.IsNullOrEmpty(postData))
            {
                var sw = new StreamWriter(request.GetRequestStream());
                sw.Write(postData);
                sw.Close();
            }

            var postResponse = default(WebResponse);

            try
            {
                postResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                var test = (HttpWebResponse)ex.Response;
                if (test.StatusCode != HttpStatusCode.OK)
                    return Results.NotificationResult.WebRequestError;
            }
            finally
            {
                postResponse?.Close();
            }

            return Results.NotificationResult.None;
        }

        private static bool CheckApiToken(string shit)
        {
            return !string.IsNullOrEmpty(shit);
        }

        public enum NotificationPriority : sbyte
        {
            VeryLow = -2,
            Moderate = -1,
            Normal = 0,
            High = 1,
            Emergency = 2
        }
    }

    public static class Results
    {
        public enum ClearCursorResults
        {
            None,
            InventoryNotOpened,
            NoSpaceInInventory,
            MaxTriesReached
        }

        public enum NotificationResult
        {
            None,
            ApiKeyError,
            TokenError,
            CredentialsError,
            WebRequestError,
            Bullshit
        }

        public enum FindItemInTabResult
        {
            None,
            GuiNotOpened,
            SwitchToTabFailed,
            GoToFirstTabFailed,
            GoToLastTabFailed,
            ItemFoundInTab,
            ItemNotFoundInTab,
        }

        /// <summary>
        /// Errors for the FastGoToHideOutFunction
        /// </summary>
        public enum FastGoToHideoutResult
        {
            /// <summary>Function ran succesfully. The bot is in hideout.</summary>
            None,
            /// <summary>You can't go to hideout using this function from outside the town</summary>
            NotInTown,
            NotInGame,
            NoHideout,
            TimeOut,
        }

        /// <summary>
        /// Errors for the OpenStash function.
        /// </summary>
        public enum OpenStashError
        {
            /// <summary>None, the stash has been interacted with and the stash panel is opened.</summary>
            None,
            /// <summary>There was an error moving to stash.</summary>
            CouldNotMoveToStash,
            /// <summary>No stash object was detected.</summary>
            NoStash,
            /// <summary>Interaction with the stash failed.</summary>
            InteractFailed,
            /// <summary>The stash panel did not open.</summary>
            StashPanelDidNotOpen,
        }

        /// <summary>
        /// Errors for the TalkToNpc function.
        /// </summary>
        public enum TalkToNpcError
        {
            /// <summary>None, the npc has been interacted with and the npc dialog panel is opened.</summary>
            None,
            /// <summary>There was an error moving to the npc.</summary>
            CouldNotMoveToNpc,
            /// <summary>No waypoint object was detected.</summary>
            NoNpc,
            /// <summary>Interaction with the npc failed.</summary>
            InteractFailed,
            /// <summary>The npc's dialog panel did not open.</summary>
            NpcDialogPanelDidNotOpen,
        }

        /// <summary>
        /// Errors for the OpenWaypoint function.
        /// </summary>
        public enum OpenWaypointError
        {
            /// <summary>None, the waypoint has been interacted with and the world panel is opened.</summary>
            None,
            /// <summary>There was an error moving to the waypoint.</summary>
            CouldNotMoveToWaypoint,
            /// <summary>No waypoint object was detected.</summary>
            NoWaypoint,
            /// <summary>Interaction with the waypoint failed.</summary>
            InteractFailed,
            /// <summary>The world panel did not open.</summary>
            WorldPanelDidNotOpen,
        }

        /// <summary>
        /// Errors for the TakeWaypointTo coroutine.
        /// </summary>
        public enum TakeAreaTransitionError
        {
            /// <summary>None, the area transition has been taken to the desired area.</summary>
            None,

            /// <summary>No area transition object was detected.</summary>
            NoAreaTransitions,

            /// <summary>Interaction with the area transition failed.</summary>
            InteractFailed,

            /// <summary>The instance manager panel did not open.</summary>
            InstanceManagerDidNotOpen,

            /// <summary>The JoinNew function failed with an error.</summary>
            JoinNewFailed,

            /// <summary>An area change did not happen after taking the area transition.</summary>
            WaitForAreaChangeFailed,

            /// <summary>Too many instances are listed based on user configuration.</summary>
            TooManyInstances,
        }

    }
}
