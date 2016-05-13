using System.Collections.ObjectModel;
using System.ComponentModel;
using Loki;
using Loki.Common;

namespace CadiroSniffer
{
    class CadiroSnifferSettings : JsonSettings
    {
        private static CadiroSnifferSettings _instance;

        public static CadiroSnifferSettings Instance => _instance ?? (_instance = new CadiroSnifferSettings());

        public CadiroSnifferSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "CadiroSniffer")))
        {
            if(DGItemsCollection == null)
            {
                DGItemsCollection = new ObservableCollection<StringEntry>();
            }
        }

        private ObservableCollection<StringEntry> _DGItemsCollection;
        private string _userKey;
        private string _apiKey;
        private string _pushbulletKey;
        private string _prowlKey;

        private bool _pushoverEnabled;
        private bool _pushbulletEnabled;
        private bool _prowlEnabled;

        private bool _notifyGodlike;
        private bool _notifyCurrency;
        private bool _notifyAll; // other

        [DefaultValue(null)]
        public ObservableCollection<StringEntry> DGItemsCollection
        {
            get { return _DGItemsCollection; }
            set
            {
                _DGItemsCollection = value;
                NotifyPropertyChanged(() => DGItemsCollection);
            }
        }

        [DefaultValue(null)]
        public string UserKey
        {
            get { return _userKey; }
            set
            {
                _userKey = value;
                NotifyPropertyChanged(() => UserKey);
            }
        }

        [DefaultValue(null)]
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                NotifyPropertyChanged(() => ApiKey);
            }
        }

        [DefaultValue(null)]
        public string PushbulletKey
        {
            get { return _pushbulletKey; }
            set
            {
                _pushbulletKey = value;
                NotifyPropertyChanged(() => PushbulletKey);
            }
        }

        [DefaultValue(null)]
        public string ProwlKey
        {
            get { return _prowlKey; }
            set
            {
                _prowlKey = value;
                NotifyPropertyChanged(() => ProwlKey);
            }
        }

        [DefaultValue(false)]
        public bool PushoverEnabled
        {
            get { return _pushoverEnabled; }
            set
            {
                _pushoverEnabled = value;
                NotifyPropertyChanged(() => PushoverEnabled);
            }
        }

        [DefaultValue(false)]
        public bool PushbulletEnabled
        {
            get { return _pushbulletEnabled; }
            set
            {
                _pushbulletEnabled = value;
                NotifyPropertyChanged(() => PushbulletEnabled);
            }
        }

        [DefaultValue(false)]
        public bool ProwlEnabled
        {
            get { return _prowlEnabled; }
            set
            {
                _prowlEnabled = value;
                NotifyPropertyChanged(() => ProwlEnabled);
            }
        }

        [DefaultValue(true)]
        public bool NotifyGodlike
        {
            get { return _notifyGodlike; }
            set
            {
                _notifyGodlike = value;
                NotifyPropertyChanged(() => NotifyGodlike);
            }
        }

        [DefaultValue(true)]
        public bool NotifyCurrency
        {
            get { return _notifyCurrency; }
            set
            {
                _notifyCurrency = value;
                NotifyPropertyChanged(() => NotifyCurrency);
            }
        }

        [DefaultValue(false)]
        public bool NotifyAll
        {
            get { return _notifyAll; }
            set
            {
                _notifyAll = value;
                NotifyPropertyChanged(() => NotifyAll);
            }
        }

        public class StringEntry
        {
            public string Name { get; set; }
        }
    }
}
