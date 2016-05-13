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

namespace CadiroSniffer
{
    public class CadiroSniffer : IPlugin
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public string Name => "CadiroSniffer";
        public string Description => "Sniff that godly offers.";
        public string Author => "Lajt";
        public string Version => "0.0.1.0";
        
        #region Implementation of IBase

        /// <summary>Initializes this plugin.</summary>
        public void Initialize()
        {
            Log.DebugFormat($"[{Name}] Initialize");
        }

        /// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
        public void Deinitialize()
        {
            Log.DebugFormat($"[{Name}] Deinitialize");
        }

        #endregion
        
        #region Implementation of IConfigurable

        /// <summary>The settings object. This will be registered in the current configuration.</summary>
        public JsonSettings Settings => CadiroSnifferSettings.Instance;


        /// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
        public UserControl Control => new Gui();

        #endregion

        #region Implementation of IRunnable

        /// <summary> The plugin start callback. Do any initialization here. </summary>
        public void Start()
        {
            Log.DebugFormat($"[{Name}] Start");
            Tasks.AddTask(new CadiroOfferTask(), "TownRunTask", Tasks.AddType.After);
        }

        /// <summary> The plugin tick callback. Do any update logic here. </summary>
        public void Tick()
        {
        }

        /// <summary> The plugin stop callback. Do any pre-dispose cleanup here. </summary>
        public void Stop()
        {
            Log.DebugFormat($"[{Name}] Stop");
        }

        #endregion

        #region Implementation of IEnableable

        /// <summary> The plugin is being enabled.</summary>
        public void Enable()
        {
            Log.DebugFormat($"[{Name}] Enable");
        }

        /// <summary> The plugin is being disabled.</summary>
        public void Disable()
        {
            Log.DebugFormat($"[{Name}] Disable");
        }

        #endregion

        #region Implementation of ILogic

        public async Task<bool> Logic(string type, params dynamic[] param)
        {
            return false;
        }

        public object Execute(string name, params dynamic[] param)
        {
            return null;
        }

        #endregion

    }
}
