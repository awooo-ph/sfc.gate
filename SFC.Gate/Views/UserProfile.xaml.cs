using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SFC.Gate.Material.ViewModels;

namespace SFC.Gate.Material.Views
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : UserControl
    {
        public UserProfile()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPassword.Password != MainViewModel.Instance.CurrentUser.Password)
            {
                MessageBox.Show("Invalid password");
                return;
            }
            if (NewPassword.Password != NewPassword2.Password)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }
            if (string.IsNullOrEmpty(NewPassword.Password)) return;
            var pwd = MainViewModel.Instance.CurrentUser.Password;
            MainViewModel.Instance.CurrentUser.Update("Password",NewPassword.Password);
            Models.Log.Add("CHANGE PASSWORD",
                $"{MainViewModel.Instance.CurrentUser.Username} changed his/her password.");
            MainViewModel.ShowMessage("Password successfully changed.","UNDO",
                ()=>MainViewModel.Instance.CurrentUser.Update("Password",pwd));
        }
    }
}
