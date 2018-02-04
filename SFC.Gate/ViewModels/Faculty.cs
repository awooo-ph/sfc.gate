using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Faculty : INotifyPropertyChanged
    {
        private Faculty() { }

        private static Faculty _instance;
        public static Faculty Instance => _instance ?? (_instance = new Faculty());
        
        private ListCollectionView _items;

        public ListCollectionView Items
        {
            get
            {
                if (_items != null) return _items;
                _items = new ListCollectionView(Student.Cache);
                _items.Filter = Filter;
                Student.Cache.CollectionChanged += (sender, args) =>
                {
                    _items.Filter = Filter;
                };
                return _items;
            }
        }

        private string _SearchKeyword;
        private DateTime _lastSearch = DateTime.Now;
        private bool _searchStarted;
        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if(value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                _lastSearch = DateTime.Now;

                if (_searchStarted)
                    return;
                _searchStarted = true;

                Task.Factory.StartNew(async () =>
                {
                    while ((DateTime.Now - _lastSearch).TotalMilliseconds < 777)
                        await TaskEx.Delay(10);
                    awooo.Context.Post(d => Items.Filter = Filter, null);
                    _searchStarted = false;
                });
            }
        }
        
        private bool Filter(object o)
        {
            if (!(o is Student s)) return false;
            if (s.Level < Departments.Faculty) return false;

            if (string.IsNullOrEmpty(SearchKeyword)) return true;
            if (s.Fullname?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.Rfid?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.YearLevel?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.ContactNumber?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            awooo.Context.Post(d =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            },null);
            
        }
    }
}
