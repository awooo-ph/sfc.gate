using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Login : INotifyPropertyChanged
    {
        private static Login _instance;

        public static Login Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new Login();
                return _instance;
            }
        }

        private Login()
        { }

        private string _username = "";

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                Picture = User.Cache.FirstOrDefault(x => x.Username.ToLower() == value.ToLower())?.Picture;
            }
        }

        private byte[] _Picture;

        public byte[] Picture
        {
            get => _Picture;
            set
            {
                if(value == _Picture)
                    return;
                _Picture = value;
                OnPropertyChanged(nameof(Picture));
                OnPropertyChanged(nameof(HasPicture));
            }
        }

        public bool HasPicture => Picture?.Length > 0;

        private ICommand _loginCommand;

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand<PasswordBox>(async pwd =>
        {
            if(string.IsNullOrEmpty(pwd.Password.Trim()))
            {
                ShowInvalidLogin();
                return;
            }

            User user;
            if(User.Cache.Count == 0)
            {
                user = new User()
                {
                    Username = Username,
                    Password = pwd.Password,
                    IsAdmin = true
                };
                user.Save();
                MainViewModel.Instance.CurrentUser = user;
                Log.Add("Login Successful", $"{Username} has logged in.", "Users", user.Id);
                return;
            }

            user = User.Cache.FirstOrDefault(x => x.Username.ToLower() == Username.ToLower() && x.Password == pwd.Password);
            if(user != null)
            {
                MainViewModel.Instance.CurrentUser = user;
                MainViewModel.Instance.Screen = MainViewModel.STUDENTS;
                Log.Add("Login Successful", $"{Username} has logged in.", "Users", user.Id);
                Username = "";
                pwd.Password = "";
                return;
            }

            Log.Add("Login Failed", $"Login attempt failed using Username: {Username} and Password: {pwd.Password}.");

            ShowInvalidLogin();

        }, s => !string.IsNullOrEmpty(Username.Trim())));

        private async void ShowInvalidLogin()
        {
            await Application.Current.MainWindow.ShowDialog(
                new MessageDialog("AUTHENTICATION FAILED!",
                    "The username/password you entered is invalid. Please try again.",
                    PackIconKind.Lock, "OKAY"));
        }

        private ICommand _exitCommand;

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new DelegateCommand(d =>
        {
            Application.Current.MainWindow.Close();
        }));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
