using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SuperKeywordGenerator.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        private bool _mnActivatateVisible;
        public bool mnActivatateVisible
        {
            get => _mnActivatateVisible;
            set => SetField(ref _mnActivatateVisible, value);
        }
        private bool _mnGetAllEnabled;
        public bool mnGetAllEnabled
        {
            get => _mnGetAllEnabled;
            set => SetField(ref _mnGetAllEnabled, value);
        }
        private int _TotalKeywords;
        public int TotalKeywords
        {
            get => _TotalKeywords;
            set => SetField(ref _TotalKeywords, value);
        }
        private int _TotalBaseKeywords;
        public int TotalBaseKeywords
        {
            get => _TotalBaseKeywords;
            set => SetField(ref _TotalBaseKeywords, value);
        }
        public ObservableCollection<string> BaseKeywords { get; set; }
        public ObservableCollection<string> GeneratedKeywords { get; set; }
        private string _PlainBaseKeywords;
        public string PlainBaseKeywords
        {
            get => _PlainBaseKeywords;
            set => SetField(ref _PlainBaseKeywords, value);
        }

        public bool _LaunchEnabled;
        public bool LaunchEnabled
        {
            get => _LaunchEnabled;
            set => SetField(ref _LaunchEnabled, value);
        }

        public bool _StopEnabled;
        public bool StopEnabled
        {
            get => _StopEnabled;
            set => SetField(ref _StopEnabled, value);
        }
        public MainVM()
        {
            BaseKeywords = new ObservableCollection<string>();
            GeneratedKeywords = new ObservableCollection<string>();
            PlainBaseKeywords = "";
            mnGetAllEnabled = true;
            _mnActivatateVisible = true;
            StopEnabled = false;
            LaunchEnabled = true;
        }
    }
}
