using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace SimpleSamSettings
{
    /// <summary>
    /// Settings base class that helps your settings act like ViewModels (can bind them to a View directly for property editing)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class SettingsViewModelBase : INotifyPropertyChanged, IDisposable, INotifyPropertyChanging
    {
        protected Dictionary<string, object> _values = new Dictionary<string, object>();

        [field: NonSerialized] public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Fired just before a property's new value is committed and PropertyChanged is fired.
        ///     Allows the handler to block the proposed changed.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        ///     Special Get'er that should be used for all properties in a Settings class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual T Get<T>([CallerMemberName] string name = null)
        {
            if (_values.ContainsKey(name))
                return (T) _values[name];

            return default(T);
        }

        /// <summary>
        ///     Stores or updates the value in a dictionary and optionally raises the prop changed event if the value changes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propName"></param>
        /// <param name="i_nonSerialized"></param>
        /// <param name="i_raisePropChanged"></param>
        /// <returns></returns>
        protected virtual bool Set<T>(T value, [CallerMemberName] string name = null, bool raisePropChanged = true)
        {
            var previousValue = default(T);
            // weird unit test check
            var values = _values;

            lock (values)
            {
                if (values.ContainsKey(name))
                {
                    if (values[name] == null && value == null)
                        return false;

                    if (values[name] != null && values[name].Equals(value))
                        return false;

                    previousValue = (T) values[name];

                    if (RaisePropertyChanging(name, previousValue, value))
                        values[name] = value;
                    else
                        return false;
                }
                else
                {
                    if (RaisePropertyChanging(name, previousValue, value))
                        values.Add(name, value);
                    else
                        return false;
                }
            }

            if (raisePropChanged)
                RaisePropertyChanged(name);

            return true;
        }

        /// <summary>
        ///     Should really only be called on composite properties that must recompute based on an input change
        ///     presumes no actual value change...just that a rebinding needs to occur
        /// </summary>
        /// <param name="name"></param>
        public virtual void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        ///     Fired on the same thread as the Set() so that property changes can be blocked
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyname"></param>
        /// <param name="proposedValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        protected virtual bool RaisePropertyChanging<T>(string propertyname, T currentValue, T proposedValue)
        {
            if (PropertyChanging != null)
            {
                var args = new PropertyChangingCancelEventArgs<T>(propertyname, currentValue, proposedValue);
                PropertyChanging(this, args);
                return !args.Cancel;
            }

            return true;
        }

        protected virtual bool SetProperty<TType>(ref TType property, TType value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TType>.Default.Equals(property, value))
                return false;
            RaisePropertyChanging(propertyName, property, value);
            property = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        #region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _values.Clear();
                GC.SuppressFinalize(this);
            }
        }

        ~SettingsViewModelBase()
        {
            Dispose(false);
        }

        #endregion
    }

    public interface ISettingsBase
    {
        bool DiskIsNewer { get; set; }
        bool DiskIsOlder { get; set; }

        /// <summary>
        /// Auto-saves on any property changed event - great if you keep your settings classes small!
        /// </summary>
        bool AutoSave { get; set; }
    }
}