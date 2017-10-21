using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SFC.Gate.Configuration;
using SFC.Gate.Models;

namespace SFC.Gate.ViewModels
{
    class Login : ViewModelBase
    {
        private static Login _instance;

        public static Login Instance
        {
            get
            {
                if(_instance==null) _instance = new Login();
                return _instance;
            }
        }

        private Login()
        {}

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
            }
        }

        private ICommand _loginCommand;

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand<PasswordBox>(pwd =>
        {
            if (string.IsNullOrEmpty(pwd.Password.Trim()))
            {
                System.Windows.MessageBox.Show("Invalid Password!", "Login", MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
                return;
            }

            User user;
            if (User.Cache.Count == 0)
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
            if (user != null)
            {
                MainViewModel.Instance.CurrentUser = user;
                Log.Add("Login Successful", $"{Username} has logged in.","Users",user.Id);
                Username = "";
                pwd.Password = "";
                return;
            }

            Log.Add("Login Failed", $"Login attempt failed using Username: {Username} and Password: {pwd.Password}.");
            
            System.Windows.MessageBox.Show("Invalid username and password!", "Login", MessageBoxButton.OK,
                MessageBoxImage.Asterisk);

        }, s => !string.IsNullOrEmpty(Username.Trim())));

        private ICommand _exitCommand;

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new DelegateCommand(d =>
        {
            Application.Current.MainWindow.Close();
        }));
    }
}
