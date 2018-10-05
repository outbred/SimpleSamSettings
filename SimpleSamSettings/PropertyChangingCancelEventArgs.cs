using System.ComponentModel;

namespace SimpleSamSettings
{
    public class PropertyChangingCancelEventArgs<T> : PropertyChangingCancelEventArgs
    {
        public PropertyChangingCancelEventArgs(string propName, T originalValue, T newValue)
            : base(propName)
        {
            OriginalValue = originalValue;
            NewValue = newValue;
        }

        public T OriginalValue { get; }

        public T NewValue { get; }
    }

    /// <summary>
    ///     Set Cancel to true to block a propertychanged event
    /// </summary>
    public class PropertyChangingCancelEventArgs : PropertyChangingEventArgs
    {
        public PropertyChangingCancelEventArgs(string propName)
            : base(propName)
        {
        }

        public bool Cancel { get; set; }
    }
}