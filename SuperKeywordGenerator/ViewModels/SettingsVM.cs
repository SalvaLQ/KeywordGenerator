using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SuperKeywordGenerator.ViewModels
{
    public class SettingsVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public List<KeywordProviderVM> KeywordProviders { get; set; }

        public int Threads { get; set; }
        public int MaxDelay { get; set; }
        public int MinDelay { get; set; }

        public int MaxKeywords { get; set; }
        public bool ExtendMode { get; set; }

        public SettingsVM()
        {
        }

        public class KeywordProviderVM
        {
            public string Name { get; set; }
            public bool Active { get; set; }
        }

    }
}
