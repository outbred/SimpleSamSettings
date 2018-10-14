using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Broadcaster;

namespace SimpleSamSettings
{

    public enum SettingsEvents
    {
        Saved
    }

    public interface ISettingsBase : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    {
        bool DiskIsNewer { get; set; }
        bool DiskIsOlder { get; set; }

        /// <summary>
        /// Auto-saves on any property changed event - great if you keep your settings classes small!
        /// </summary>
        bool AutoSave { get; set; }

        /// <summary>
        /// May call static Save() method or this one - whatever is most convenient
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveInstance();

        /// <summary>
        /// To know when this settings instance is saved, listen using this property
        /// </summary>
        Listener<SettingsEvents> Listener { get; }
    }
}