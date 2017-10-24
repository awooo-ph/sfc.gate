using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SFC.Gate.Models;
using SFC.Gate.Views;
using MsgBox = System.Windows.MessageBox;

namespace SFC.Gate.ViewModels
{
    class MainViewModel : ViewModelBase
    {

        private MainViewModel()
        {
           
        }
        
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new MainViewModel();
                
                return _instance;
            }
        }
        
        private User _currentUser;

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value; 
                OnPropertyChanged(nameof(CurrentUser));
                SelectedTab = _currentUser != null ? 0 : 7;
            }
        }
        
        private ICommand _addViolationCommand;

        public ICommand AddViolationCommand => _addViolationCommand ?? (_addViolationCommand = new DelegateCommand(stud =>
        {
            Violations.Instance.SelectedStudent = Students.Instance.SelectedStudent;
            SelectedTab = 2;
        }));

        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>
        {
            Log.Add("User Logout", $"{CurrentUser.Username} has logged out.", "Users",CurrentUser.Id);
            CurrentUser = null;
        }));

        private ICommand _changePasswordCommand;

        public ICommand ChangePasswordCommand =>
            _changePasswordCommand ?? (_changePasswordCommand = new DelegateCommand(
                d => ChangePassword()));

        private bool _isDialogOpen;

        public bool IsDialogOpen
        {
            get { return _isDialogOpen; }
            set
            {
                _isDialogOpen = value; 
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _enableGuardModeCommand;

        public ICommand EnableGuardModeCommand =>
            _enableGuardModeCommand ?? (_enableGuardModeCommand = new DelegateCommand(
                d =>
                {
                    IsGuardMode = true;
                }));

        private bool _isGuardMode = false;

        public bool IsGuardMode
        {
            get { return _isGuardMode; }
            set
            {
                _isGuardMode = value; 
                OnPropertyChanged(nameof(IsGuardMode));
            }
        }
        

        private void ChangePassword()
        {
            while (true)
            {
                IsDialogOpen = true;
                var cPwd = new ChangePassword();
                cPwd.Owner = Application.Current.MainWindow;
                if (cPwd.ShowDialog() ?? false)
                {
                    var msg = "";
                    if (CurrentUser.Password != cPwd.CurrentPassword.Password)
                        msg = "Invalid password.";
                    else if (cPwd.NewPassword.Password.Length == 0)
                        msg = "Password is required.";
                    else if (cPwd.NewPassword.Password != cPwd.NewPassword2.Password)
                        msg = "Password did not match.";
                    else if (cPwd.NewPassword.Password.Length < 7)
                        msg = "Password must be at least seven (7) characters long.";

                    if (msg != "")
                    {
                        Log.Add("Password Change Failed", msg, "Users", CurrentUser.Id);
                        MsgBox.Show(msg, "Change Password", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        continue;
                    }
                }
                else
                {
                    IsDialogOpen = false;
                    break;
                }
                
                CurrentUser.Update("Password",cPwd.NewPassword.Password);
                Log.Add("Password Changed", $"{CurrentUser.Username} changed his/her password.","Users",CurrentUser.Id);
                MsgBox.Show("Your password is successfully updated!", "Change Password", MessageBoxButton.OK, MessageBoxImage.Information);
                IsDialogOpen = false;
                break;
            }
        }
    }
}
