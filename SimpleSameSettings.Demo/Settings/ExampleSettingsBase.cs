using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SimpleSamSettings;

namespace SimpleSameSettings.Demo.Settings
{
    internal interface IExampleSettingsBase : ISettingsBase
    {
        int Year { get; set; }
        int Month { get; set; }
        int Day { get; set; }
        int Version { get; set; }
        string First { get; set; }
        string Last { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Zip { get; set; }
        ObservableCollection<string> Aliases { get; set; }
    }

    [Serializable]
    internal abstract class ExampleSettingsBase<TInheritor> : OverwriteSettingsProfile<TInheritor>, IExampleSettingsBase where TInheritor : class, IExampleSettingsBase, new()
    {
        protected ExampleSettingsBase()
        {
            var now = DateTime.Now;
            Year = now.Year;
            Month = now.Month;
            Day = now.Day;
            Version = 1;
            First = "John";
            Last = "Doe";
            Address = "123 Chop St";
            City = "Place";
            State = "ST";
            Zip = "00123";
            Aliases = new ObservableCollection<string>() {"One Alias"};
            // example of how to persisted on collectionchanged
            Aliases.CollectionChanged += (sender, args) => SaveInstance();
        }

        public int Year
        {
            get => Get<int>();
            set => Set(value);
        }

        public int Month
        {
            get => Get<int>();
            set => Set(value);
        }

        public int Day
        {
            get => Get<int>();
            set => Set(value);
        }

        public int Version
        {
            get => Get<int>();
            set => Set(value);
        }

        public string First
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Last
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Address
        {
            get => Get<string>();
            set => Set(value);
        }

        public string City
        {
            get => Get<string>();
            set => Set(value);
        }

        public string State
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Zip
        {
            get => Get<string>();
            set => Set(value);
        }

        public ObservableCollection<string> Aliases
        {
            get => Get<ObservableCollection<string>>();
            set => Set(value);
        }
    }
}