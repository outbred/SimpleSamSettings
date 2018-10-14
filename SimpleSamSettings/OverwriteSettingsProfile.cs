using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Broadcaster;
using Newtonsoft.Json;

namespace SimpleSamSettings
{
    /// <summary>
    ///     A settings profile that overwrites the file on disk with the in-memory instance.
    ///     Other profiles may be created/used like incrementing a file name for each read/write instead of overwriting one every time
    /// </summary>
    /// <typeparam name="TSettingsClass"></typeparam>
    [Serializable]
    public abstract class OverwriteSettingsProfile<TSettingsClass> : ResettableSettingsBase, ISettingsBase where TSettingsClass : class, ISettingsBase, new()
    {
        private static TSettingsClass _instance;

        private static bool _isNew = true;

        private static readonly object Locker = new object();

        [NonSerialized] private readonly ulong _currentVersion = 0;

        [NonSerialized] private bool _diskIsNewer;

        [NonSerialized] private bool _diskIsOlder;

        public static TSettingsClass Instance
        {
            get
            {
                _instance = GetInstance(_instance);

                return _instance;
            }
        }

        public static string FilePath => JsonPersistor.BuildFileName<TSettingsClass>();

        /// <summary>
        ///     If this is true, then this is the first time the app has been run
        /// </summary>
        public static bool IsNew
        {
            get
            {
                if (_instance == null)
                {
                    var dummy = Instance;
                }

                return _isNew;
            }
            private set => _isNew = value;
        }

        [JsonProperty] public virtual ulong PersistedVersion { get; protected set; }

        [JsonIgnore] protected virtual ulong CurrentVersion => _currentVersion;

        [JsonIgnore]
        public virtual bool DiskIsOlder
        {
            get => _diskIsOlder;
            set => _diskIsOlder = value;
        }

        public bool AutoSave { get; set; } = true;

        [JsonIgnore]
        public bool DiskIsNewer
        {
            get => _diskIsNewer;
            set => _diskIsNewer = value;
        }

        /// <summary>
        ///     Clears the current Instance in memory so that the next time Instance is accessed, it pulls from disk
        /// </summary>
        /// <returns></returns>
        public static bool Reload()
        {
            if (_instance != null)
                _instance.PropertyChanged -= PropChangedOnPropertyChanged;
            _instance = null;
            return Instance != null;
        }

        private static TSettingsClass GetInstance(TSettingsClass instance)
        {
            if (instance == null)
            {
                TSettingsClass temp;
                var type = typeof(TSettingsClass);
                // If not Serializable, then use JSON
                if (!type.IsSerializable)
                    IsNew = !JsonPersistor.Retrieve(out temp);
                else
                    IsNew = !BinaryPersistor.Retrieve(out temp);

                if (temp is INotifyPropertyChanged propChanged)
                {
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Where(p => p.CanRead && p.CanWrite)
                        .Select(p => p.Name).ToList();


                    // auto-save settings on PropertyChanged events (sweet!)
                    propChanged.PropertyChanged += PropChangedOnPropertyChanged;
                }

                instance = temp;
            }

            return instance;
        }

        private static bool saving = false;
        private static async void PropChangedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var temp = sender as TSettingsClass;
            if (saving)
                return;
            saving = true;
            if (temp.AutoSave)
                await Save();
            saving = false;
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DiskIsOlder = PersistedVersion < CurrentVersion;
            DiskIsNewer = CurrentVersion < PersistedVersion;
        }

        public static async Task<bool> Save()
        {
            return await Instance.SaveInstance();
        }

        [NonSerialized]
        private Listener<SettingsEvents> _listener;
        /// <summary>
        /// To listen for Settings events, subscribe using the property (using Broadcaster pubsub framework)
        /// </summary>
        [JsonIgnore]
        public Listener<SettingsEvents> Listener => _listener ?? (_listener = new Listener<SettingsEvents>(Broadcaster));

        [NonSerialized] private Broadcaster<SettingsEvents> _broadcaster;

        [JsonIgnore]
        private Broadcaster<SettingsEvents> Broadcaster => _broadcaster ?? (_broadcaster = new Broadcaster<SettingsEvents>());

        public async Task<bool> SaveInstance()
        {
            bool result;
            lock (Locker)
            {
                bool exists;
                if (!typeof(TSettingsClass).IsSerializable)
                    result = JsonPersistor.Save(Instance, out exists);
                else
                    result = BinaryPersistor.Save(Instance, out exists);

                Instance.DiskIsOlder = false;
                Instance.DiskIsNewer = false;
            }

            if (result)
                await Broadcaster.Broadcast(SettingsEvents.Saved);

            return result;
        }
    }
}