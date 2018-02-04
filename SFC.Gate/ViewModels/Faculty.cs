using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Gate.Material.Views;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Faculty : INotifyPropertyChanged
    {
        private Faculty()
        {
            Messenger.Default.AddListener<int>(Messages.ScreenChanged, screen =>
            {
                if (screen == MainViewModel.FACULTY)
                {
                    RfidScanner.ExclusiveCallback = ScanCallback;
                }
            });
        }

        private static Faculty _instance;
        public static Faculty Instance => _instance ?? (_instance = new Faculty());

        private bool _IsDialogOpen;

        public bool IsDialogOpen
        {
            get => DialogContent!=null && _IsDialogOpen;
            set
            {
                if(value == _IsDialogOpen)
                    return;
                _IsDialogOpen = value;
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private object _DialogContent;

        public object DialogContent
        {
            get => _DialogContent;
            set
            {
                if(value == _DialogContent)
                    return;
                _DialogContent = value;
                OnPropertyChanged(nameof(DialogContent));
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _addCommand;
        private NewFacultyViewModel NewFaculty;
        public ICommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(d =>
        {
            NewFaculty = new NewFacultyViewModel()
            {
                CancelCommand = new DelegateCommand(a =>
                {
                    NewFaculty = null;
                    DialogContent = null;
                    IsDialogOpen = false;
                    ScanCallback = null;
                    RfidScanner.ExclusiveCallback = null;
                }),
                AcceptCommand = new DelegateCommand(a =>
                {
                    NewFaculty.Item.Save();
                    
                    NewFaculty = null;
                    DialogContent = null;
                    IsDialogOpen = false;
                    ScanCallback = null;
                    RfidScanner.ExclusiveCallback = null;
                },a=>NewFaculty.Item.CanSave() && !NewFaculty.HasError)
            };
            DialogContent = NewFaculty;
            IsDialogOpen = true;

            ScanCallback = async id =>
            {
                if (Student.Cache.Any(x => x.Rfid.ToUpper() == id) || Visit.GetByRfid(id).Count > 0)
                {
                    NewFaculty.HasError = true;
                    NewFaculty.Item.Rfid = id;
                    await TaskEx.Delay(777);
                    NewFaculty.HasError = false;
                    NewFaculty.Item.Rfid = "";
                }
                else
                {
                    NewFaculty.Item.Rfid = id;
                    NewFaculty.HasError = false;
                }
            };
            RfidScanner.ExclusiveCallback = ScanCallback;
        }));

        private Action<string> ScanCallback;
        
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
