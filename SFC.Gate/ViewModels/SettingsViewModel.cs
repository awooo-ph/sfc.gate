using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    partial class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SettingsViewModel()
        {
            Messenger.Default.AddListener<string>(Messages.Scan, id =>
            {
                ScanTest = id;
            });

            Messenger.Default.AddListener(Messages.ScannerRegistered, () =>
            {
                awooo.Context.Post(d => OnPropertyChanged(""), null);
            });
            
            Messenger.Default.AddListener<User>(Messages.ModelDeleted, 
                user => MainViewModel.ShowMessage("User deleted","UNDO", user.Undelete));
        }

        private static SettingsViewModel _instance;
        public static SettingsViewModel Instance => _instance ?? (_instance = new SettingsViewModel());
        
        private ListCollectionView _users;

        public ListCollectionView Users
        {
            get
            {
                if (_users != null) return _users;
                _users = new ListCollectionView(User.Cache);
                return _users;
            }
        }

        private bool _ShowUserDetails;

        public bool ShowUserDetails
        {
            get => _ShowUserDetails;
            set
            {
                if(value == _ShowUserDetails)
                    return;
                _ShowUserDetails = value;
                OnPropertyChanged(nameof(ShowUserDetails));
            }
        }
        
        private ICommand _editUserCommand;

        public ICommand EditUserCommand => _editUserCommand ?? (_editUserCommand = new DelegateCommand<User>(user =>
        {
            ShowUserDetails = true;
        },u=>u!=null));

        private ICommand _cancelUserCommand;

        public ICommand CancelUserCommand => _cancelUserCommand ?? (_cancelUserCommand = new DelegateCommand(d =>
        {
            var user = Users.CurrentItem as User;
            if (user == null) return;
            user.Reset();
            ShowUserDetails = false;
        }));

        private ICommand _saveUserCommand;

        public ICommand SaveUserCommand => _saveUserCommand ?? (_saveUserCommand = new DelegateCommand<User>(user =>
        {
            user.Save();
            ShowUserDetails = false;
        },u=>u?.CanSave()??false));
    }
}
