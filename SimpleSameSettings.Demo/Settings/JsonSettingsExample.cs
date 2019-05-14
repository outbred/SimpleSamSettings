using System;
using System.Collections.ObjectModel;

namespace SimpleSameSettings.Demo.Settings
{
    /// <summary>
    /// Missing [Serializable] attribute, so it'll be serialized as JSON
    /// </summary>
    internal class JsonSettingsExample : ExampleSettingsBase<JsonSettingsExample>
    {
        public JsonSettingsExample() : base()
        {
            var now = DateTime.Now;
            Year = now.Year + 1;
            Month = now.Month + 1;
            Day = now.Day + 1;
            Version = 2;
            First = "Jane";
            Last = "Doey";
            Address = "124 Shop St";
            City = "Places";
            State = "TS";
            Zip = "12300";
            Aliases = new ObservableCollection<string>() { "Two Alias" };
        }
    }
}