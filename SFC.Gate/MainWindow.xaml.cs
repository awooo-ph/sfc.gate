using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using SFC.Gate.Configuration;
using SFC.Gate.Models;
using SFC.Gate.ViewModels;

namespace SFC.Gate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.Add("Application Started");
            ViewModelBase.Context = SynchronizationContext.Current;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (General.ConfirmExit && System.Windows.MessageBox.Show(
                    "Are you sure you want to exit?", "Confirm Exit",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
            base.OnClosing(e);
        }
    }
}
