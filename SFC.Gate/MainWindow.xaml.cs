using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
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

            MainViewModel.Context = SynchronizationContext.Current;
        }
        
    }
}
