using Loki.Game;
using Loki.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorQuest.Helpers
{
    public static class Lajt
    {
        public static bool NearMonsters(float distance = 50)
            => LokiPoe.ObjectManager.GetObjectsByType<Monster>().Where(m => !m.IsDead && m.IsHostile && m.Distance < distance).ToList().Any();

    }
}
