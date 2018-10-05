using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleSamSettings
{
    /// <summary>
    /// Builds on the functionality of SettingsViewModelBase to make undoing settings changes possible, without bothering with disk i/o
    /// </summary>
    public class ResettableSettingsBase : SettingsViewModelBase
    {
        /// <summary>
        /// Set to true so that all changes are stored off to be undoable later
        /// </summary>
        protected virtual bool MakeUndoable { get; set; } = false;

        /// <summary>
        ///     Returns the dictionary states for each property that has been modified after ResettingInitialState
        /// </summary>
        [JsonIgnore] 
        readonly Dictionary<string, object> _originalValues;

        public ResettableSettingsBase()
        {
            _originalValues = new Dictionary<string, object>();
        }

        #region Overrides of SettingsViewModelBase

        protected override bool RaisePropertyChanging<T>(string propertyname, T currentValue, T proposedValue)
        {
            if (MakeUndoable)
            {
                if (!_originalValues.ContainsKey(propertyname))
                    _originalValues.Add(propertyname, currentValue);
            }

            return base.RaisePropertyChanging(propertyname, currentValue, proposedValue);
        }

        /// <summary>
        ///     This is used to set the initial state, which just clears the dictionary.
        /// </summary>
        public void ResetInitialState()
        {
            _originalValues?.Clear();
        }

        /// <summary>
        /// Reverts all pending changes stored in IntialStates
        /// </summary>
        public void OnCancel()
        {
            if (_originalValues == null)
                return;

            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var prop in _originalValues)
            {
                var property = properties.FirstOrDefault(p => p.Name == prop.Key);
                if (property == null) return;
                property.SetValue(this, prop.Value);
            }

            _originalValues.Clear();
        }

        #endregion
    }
}