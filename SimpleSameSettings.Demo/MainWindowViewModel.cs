using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SimpleSameSettings.Demo.Settings;
using SimpleSamSettings;

namespace SimpleSameSettings.Demo
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _binarySettings;
        private bool _persistToDisk;
        protected internal IDisposable _token;
        protected internal IDisposable _jsonToken;

        public MainWindowViewModel()
        {
            PersistToDisk = true;
            Binary = BinarySettingsExample.Instance;
            Json = JsonSettingsExample.Instance;
            _token = Binary.Listener.Listen(SettingsEvents.Saved, async () => MessageBox.Show($"Binary saved!"));
            _jsonToken = Json.Listener.Listen(SettingsEvents.Saved, async () => MessageBox.Show($"Json saved!"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BinarySettingsExample Binary { get; }

        public JsonSettingsExample Json { get; }

        public IExampleSettingsBase CurrentSettings => BinarySettings ?? false ? (IExampleSettingsBase)Binary : Json;

        public bool? BinarySettings
        {
            get => _binarySettings;
            set
            {
                if (_binarySettings == value)
                    return;
                _binarySettings = value ?? false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSettings));
            }
        }        
        
        public bool? PersistToDisk
        {
            get => _persistToDisk;
            set
            {
                if (_persistToDisk == value)
                    return;
                _persistToDisk = value ?? false;
                Globals.NoPersistence = !_persistToDisk;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCurrent => new AsyncDelegateCommand(async _ => await CurrentSettings.SaveInstance());

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
