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
using SFC.Gate.ViewModels;

namespace SFC.Gate.Views
{
    /// <summary>
    /// Interaction logic for StudentFinder.xaml
    /// </summary>
    public partial class StudentFinder : UserControl
    {
        public StudentFinder()
        {
            InitializeComponent();
        }

        private void StudentList_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if(Students.Instance.SelectedStudent!=null)
            MainViewModel.Instance.SelectedTab = 1;
        }
    }
}
