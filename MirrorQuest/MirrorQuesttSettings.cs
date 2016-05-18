using System.Collections.ObjectModel;
using System.ComponentModel;
using Loki;
using Loki.Common;
using Newtonsoft.Json;
using MirrorQuest.Classes;

namespace MirrorQuest
{
    class MirrorQuestSettings : JsonSettings
    {
        private static MirrorQuestSettings _instance;

        public static MirrorQuestSettings Instance => _instance ?? (_instance = new MirrorQuestSettings());

        public MirrorQuestSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "MirrorQuest")))
        {
            if(CommonCollection == null)
            {
                CommonCollection = new ObservableCollection<Common>();
            }
            if(OfferCollection == null)
            {
                OfferCollection = new ObservableCollection<Offer>();
            }
        }
        
        private string _pushoverUserKey;
        private string _pushoverApiKey;
        private string _pushbulletKey;
        private string _prowlKey;

        private bool _pushoverEnabled;
        private bool _pushbulletEnabled;
        private bool _prowlEnabled;

        private bool _mobileNotifySuccess;
        private bool _mobileNotifyBotStop;
        private bool _mobileNotifyAll; // other

        private bool _soundNotifySuccess;
        private bool _soundNotifyBotStop;
        private bool _soundNotifyAll;

        private bool _autoCurrency;

        private bool _amuletBuy;
        private int _amuletPrice;
        private bool _ringBuy;
        private int _ringPrice;
        private bool _jewelBuy;
        private int _jewelPrice;
        private bool _mapBuy;
        private int _mapPrice;

        private ObservableCollection<Offer> _offerCollection;
        private ObservableCollection<Common> _commonCollection;

        [JsonIgnore]
        public ObservableCollection<Offer> OfferCollection
        {
            get { return _offerCollection; }
            set
            {
                _offerCollection = value;
                NotifyPropertyChanged(() => OfferCollection);
            }
        }

        [DefaultValue(null)]
        public ObservableCollection<Common> CommonCollection
        {
            get { return _commonCollection; }
            set
            {
                _commonCollection = value;
                NotifyPropertyChanged(() => CommonCollection);
            }
        }

        [DefaultValue(null)]
        public string PushoverUserKey
        {
            get { return _pushoverUserKey; }
            set
            {
                _pushoverUserKey = value;
                NotifyPropertyChanged(() => PushoverUserKey);
            }
        }

        [DefaultValue(null)]
        public string PushoverApiKey
        {
            get { return _pushoverApiKey; }
            set
            {
                _pushoverApiKey = value;
                NotifyPropertyChanged(() => PushoverApiKey);
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

        [DefaultValue(false)]
        public bool MobileNotifySuccess
        {
            get { return _mobileNotifySuccess; }
            set
            {
                _mobileNotifySuccess = value;
                NotifyPropertyChanged(() => MobileNotifySuccess);
            }
        }

        [DefaultValue(false)]
        public bool MobileNotifyBotStop
        {
            get { return _mobileNotifyBotStop; }
            set
            {
                _mobileNotifyBotStop = value;
                NotifyPropertyChanged(() => MobileNotifyBotStop);
            }
        }

        [DefaultValue(false)]
        public bool MobileNotifyAll
        {
            get { return _mobileNotifyAll; }
            set
            {
                _mobileNotifyAll = value;
                NotifyPropertyChanged(() => MobileNotifyAll);
            }
        }

        [DefaultValue(false)]
        public bool SoundNotifySuccess
        {
            get { return _soundNotifySuccess; }
            set
            {
                _soundNotifySuccess = value;
                NotifyPropertyChanged(() => SoundNotifySuccess);
            }
        }

        [DefaultValue(false)]
        public bool SoundNotifyBotStop
        {
            get { return _soundNotifyBotStop; }
            set
            {
                _soundNotifyBotStop = value;
                NotifyPropertyChanged(() => SoundNotifyBotStop);
            }
        }

        [DefaultValue(false)]
        public bool SoundNotifyAll
        {
            get { return _soundNotifyAll; }
            set
            {
                _soundNotifyAll = value;
                NotifyPropertyChanged(() => SoundNotifyAll);
            }
        }

        [DefaultValue(true)]
        public bool AutoCurrency
        {
            get { return _autoCurrency; }
            set
            {
                _autoCurrency = value;
                NotifyPropertyChanged(() => AutoCurrency);
            }
        }

        [DefaultValue(false)]
        public bool AmuletBuy
        {
            get { return _amuletBuy; }
            set
            {
                _amuletBuy = value;
                NotifyPropertyChanged(() => AmuletBuy);
            }
        }

        [DefaultValue(0)]
        public int AmuletPrice
        {
            get { return _amuletPrice; }
            set
            {
                _amuletPrice = value;
                NotifyPropertyChanged(() => AmuletPrice);
            }
        }

        [DefaultValue(false)]
        public bool RingBuy
        {
            get { return _ringBuy; }
            set
            {
                _ringBuy = value;
                NotifyPropertyChanged(() => RingBuy);
            }
        }

        [DefaultValue(0)]
        public int RingPrice
        {
            get { return _ringPrice; }
            set
            {
                _ringPrice = value;
                NotifyPropertyChanged(() => RingPrice);
            }
        }

        [DefaultValue(false)]
        public bool JewelBuy
        {
            get { return _jewelBuy; }
            set
            {
                _jewelBuy = value;
                NotifyPropertyChanged(() => JewelBuy);
            }
        }

        [DefaultValue(0)]
        public int JewelPrice
        {
            get { return _jewelPrice; }
            set
            {
                _jewelPrice = value;
                NotifyPropertyChanged(() => JewelPrice);
            }
        }

        [DefaultValue(false)]
        public bool MapBuy
        {
            get { return _mapBuy; }
            set
            {
                _mapBuy = value;
                NotifyPropertyChanged(() => MapBuy);
            }
        }

        [DefaultValue(0)]
        public int MapPrice
        {
            get { return _mapPrice; }
            set
            {
                _mapPrice = value;
                NotifyPropertyChanged(() => MapPrice);
            }
        }

        public class StringEntry
        {
            public string Name { get; set; }
        }
    }
}
