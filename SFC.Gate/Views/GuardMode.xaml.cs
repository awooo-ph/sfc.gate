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
    /// Interaction logic for GuardMode.xaml
    /// </summary>
    public partial class GuardMode : UserControl
    {
        public GuardMode()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            MainViewModel.Instance.ShowSideBar = true;
        }


        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            MainViewModel.Instance.ShowSideBar = false;
        }

        private void UIElement_OnIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MainViewModel.Instance.ShowSideBar = ((Grid) sender).IsMouseOver;
        }
    }
}
