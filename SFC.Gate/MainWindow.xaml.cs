using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using SFC.Gate.Configurations;
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
            WindowState = Config.General.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
            if (WindowState != WindowState.Maximized)
            {
                Top = Config.General.WindowTop;
                Left = Config.General.WindowLeft;
                Height = Config.General.WindowHeight;
                Width = Config.General.WindowWidth;
            }
            Log.Add("Application Started");
            ViewModelBase.Context = SynchronizationContext.Current;

        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            // Config.General.WindowMaximized = WindowState == WindowState.Maximized;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                Config.General.WindowWidth = ActualWidth;
                Config.General.WindowHeight = ActualHeight;
                Config.General.WindowLeft = Left;
                Config.General.WindowTop = Top;
                Config.General.WindowMaximized = WindowState == WindowState.Maximized;
            }

            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Config.General.ConfirmExit && System.Windows.MessageBox.Show(
                    "Are you sure you want to exit?", "Confirm Exit",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
            RfidScanner.UnHook();
            base.OnClosing(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            MainViewModel.Instance.IsGuardMode = Config.General.GuarModeOnStartup;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            RfidScanner.Hook(this);
            base.OnSourceInitialized(e);
        }
    }
}
