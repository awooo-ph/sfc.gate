using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SFC.Gate.Models;

namespace SFC.Gate.ViewModels
{
    class Users:ViewModelBase
    {
        private static Users _instance;
        public static Users Instance => _instance ?? (_instance = new Users());

        private Users()
        {
            Context = SynchronizationContext.Current;
        }

        public ObservableCollection<User> Items
        {
            get
            {
                return User.Cache;
            }
        }

        private User _selectedUser;

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value; 
                OnPropertyChanged(nameof(SelectedUser));
                Username = value?.Username;
                CurrentPassword = "";
                NewPassword2 = "";
                NewPassword = "";
                OnPropertyChanged(nameof(HasPassword));
                OnPropertyChanged(nameof(DeleteCancel));
                IsAdmin = _selectedUser?.IsAdmin??false;
            }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value; 
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;

        public string CurrentPassword
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        private string _newPassword;

        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value; 
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        private string _newPassword2;

        public string NewPassword2
        {
            get { return _newPassword2; }
            set
            {
                _newPassword2 = value; 
                OnPropertyChanged(nameof(NewPassword2));
            }
        }

        public bool HasPassword => SelectedUser?.Id > 0 && !string.IsNullOrEmpty(SelectedUser?.Password);

        public string DeleteCancel => SelectedUser?.Id > 0 ? "Delete" : "Cancel";
        
        private ICommand _addUserCommand;

        public ICommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(d =>
        {
            var usr = new User();
            Items.Add(usr);
            SelectedUser = usr;
        }, CanAddUser));

        private bool CanAddUser(object obj)
        {
            return User.Cache.All(x => x?.Id > 0);
        }

        private ICommand _deleteUserCommand;

        public ICommand DeleteCommand => _deleteUserCommand ?? (_deleteUserCommand = new DelegateCommand(d =>
        {
            SelectedUser?.Delete();
        }));

        private bool _isAdmin;

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
                if (!_isAdmin && (SelectedUser?.IsAdmin ?? false) && User.Cache.Count(x => x.IsAdmin) == 1)
                {
                    System.Windows.MessageBox.Show("There must be at least one (1) administrator account.",
                        "Action Failed", MessageBoxButton.OK, MessageBoxImage.Stop);
                    _isAdmin = true;
                }
            }
        }

        private ICommand _saveCommand;

        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(d =>
        {
            var msg = "";
            if (string.IsNullOrEmpty(Username.Trim()))
                msg = "Username is required.";

            if (SelectedUser.Id == 0)
            {
                if (string.IsNullOrEmpty(NewPassword))
                    msg = ("Password cannot be empty.");
                if (NewPassword != NewPassword2)
                    msg = ("Password did not match.");
            }
            else
            {
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    if (CurrentPassword != SelectedUser.Password)
                        msg = "Invalid password.";
                    else if (string.IsNullOrEmpty(NewPassword))
                        msg = "Password is required.";
                    else if (NewPassword != NewPassword2)
                        msg = "Password did not match.";
                }
            }
            
        var usr = User.Cache.FirstOrDefault(x => x.Username == Username);
            if (usr != null && usr.Id != SelectedUser.Id && usr.Id > 0)
            {
                msg = "Please choose a unique username.";
            }
            
            if (msg != "")
            {
                System.Windows.MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            
      
            if (SelectedUser.Id == 0)
            {
                usr = new User();
                usr.Password = NewPassword;
            }
            else
            {
                usr = SelectedUser;
                if (!string.IsNullOrEmpty(NewPassword))
                    usr.Password = NewPassword;
            }
            
            usr.Username = Username;
            usr.IsAdmin = IsAdmin;
            
            SelectedUser.Remove();
            
            usr.Save();
        }));
        
    }
}
