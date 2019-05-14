using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleSamSettings
{
    /// <summary>
    /// Builds on the functionality of SettingsViewModelBase to make undoing settings changes possible
    ///
    ///
    /// Workflow (can be repeated n times):
    /// ResetInitialState() - clear any and all original values
    /// Bind to View - make a ton of changes
    /// RevertChanges() - returns all settings back to their original values
    /// 
    /// </summary>
    [Serializable]
    public abstract class ResettableSettingsBase : SettingsViewModelBase
    {
        /// <summary>
        /// Set to true so that all changes are stored off to be undoable later
        /// </summary>
        protected virtual bool MakeUndoable { get; set; } = false;

        /// <summary>
        /// Returns the dictionary states for each property that has been modified after ResettingInitialState 
        /// </summary>
        [JsonIgnore] 
        readonly Dictionary<string, object> _originalValues;

        public ResettableSettingsBase()
        {
            _originalValues = new Dictionary<string, object>();
        }

        #region Overrides of SettingsViewModelBase

        protected override bool RaisePropertyChanging<T>(string propertyName, T currentValue, T proposedValue)
        {
            if (MakeUndoable)
            {
                if (!_originalValues.ContainsKey(propertyName))
                    _originalValues.Add(propertyName, currentValue);
            }

            return base.RaisePropertyChanging(propertyName, currentValue, proposedValue);
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
        public void RevertChanges()
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