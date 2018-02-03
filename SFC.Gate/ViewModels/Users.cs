using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Users : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private Users() { }
        private static Users _instance;
        public static Users Instance => _instance ?? (_instance = new Users());

        private ICommand _addUserCommand;

        public ICommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(d =>
        {
            ShowAddItem = true;
            NewItem = new User() {Picture = Extensions.Generate()};
        }));

        private bool _ShowAddITem;

        public bool ShowAddItem
        {
            get => _ShowAddITem;
            set
            {
                if(value == _ShowAddITem)
                    return;
                _ShowAddITem = value;
                OnPropertyChanged(nameof(ShowAddItem));
            }
        }

        private User _NewItem;

        public User NewItem
        {
            get => _NewItem;
            set
            {
                if(value == _NewItem)
                    return;
                _NewItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        private ICommand _cancelAddCommand;

        public ICommand CancelAddCommand => _cancelAddCommand ?? (_cancelAddCommand = new DelegateCommand(d =>
        {
            ShowAddItem = false;
        }));

        private ICommand _acceptAddCommand;

        public ICommand AcceptAddCommand => _acceptAddCommand ?? (_acceptAddCommand = new DelegateCommand(d =>
        {
            NewItem?.Save();
            ShowAddItem = false;
        },d=>NewItem?.CanSave()??false));
    }
}
