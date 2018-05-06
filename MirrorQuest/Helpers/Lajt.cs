using Loki.Game;
using Loki.Game.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
