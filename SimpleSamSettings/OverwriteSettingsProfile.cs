using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace SimpleSamSettings
{
    /// <summary>
    ///     A settings profile that overwrites the file on disk with the in-memory instance.
    ///     Other profiles may be created/used like incrementing a file name for each read/write instead of overwriting one every time
    /// </summary>
    /// <typeparam name="TSettingsClass"></typeparam>
    public abstract class OverwriteSettingsProfile<TSettingsClass> : ResettableSettingsBase, ISettingsBase where TSettingsClass : class, ISettingsBase, new()
    {
        private static TSettingsClass _instance;

        private static bool _isNew = true;

        private static readonly object _locker = new object();

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
            _instance = null;
            return Instance != null;
        }

        private static TSettingsClass GetInstance(TSettingsClass instance)
        {
            if (instance == null)
            {
                TSettingsClass temp;
                var type = typeof(TSettingsClass);
                // Add some flexibility so that if settings prefer the JSON format, we can allow
                if (!type.IsSerializable)
                {
                    IsNew = !JsonPersistor.Retrieve(out temp);
                    if (temp != null && type.GetInterface(typeof(INotifyPropertyChanged).FullName) != null)
                    {
                        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                            .Where(p => p.CanRead && p.CanWrite)
                            .Select(p => p.Name).ToList();


                        ((INotifyPropertyChanged) temp).PropertyChanged += (s, e) =>
                        {
                            if (properties.Any(p => p == e.PropertyName))
                                Save();
                        };
                    }
                }
                else
                {
                    IsNew = !BinaryPersistor.Retrieve(out temp);
                    if (temp != null && type.GetInterface(typeof(INotifyPropertyChanged).FullName) != null)
                    {
                        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();


                        // auto-save settings on PropertyChanged events (sweet!)
                        ((INotifyPropertyChanged) temp).PropertyChanged += (s, e) =>
                        {
                            if (properties.Any(p => p.Name == e.PropertyName))
                                Save();
                        };
                    }
                }

                instance = temp;
            }

            return instance;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DiskIsOlder = PersistedVersion < CurrentVersion;
            DiskIsNewer = CurrentVersion < PersistedVersion;
        }

        public static bool Save()
        {
            var settings = _instance;
            if (settings != null)
            {
                bool result;
                lock (_locker)
                {
                    bool exists;
                    if (!typeof(TSettingsClass).IsSerializable)
                        result = JsonPersistor.Save(Instance, out exists);
                    else
                        result = BinaryPersistor.Save(Instance, out exists);

                    Instance.DiskIsOlder = false;
                    Instance.DiskIsNewer = false;
                }

                return result;
            }

            return false;
        }
    }
}