using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using SFC.Gate.Configurations;
using SFC.Gate.Material.ViewModels;
using SFC.Gate.Models;

namespace SFC.Gate.Material
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime _lastActivity = DateTime.Now;
        private double SideBarWidth=0.0;
        public MainWindow()
        {
            InitializeComponent();
            
            WindowState = Config.General.WindowState;//.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
            if(WindowState != WindowState.Maximized)
            {
                Top = Config.General.WindowTop;
                Left = Config.General.WindowLeft;
                Height = Config.General.WindowHeight;
                Width = Config.General.WindowWidth;
            }
            Log.Add("Application Started");
            
            Messenger.Default.AddListener<int>(Messages.ScreenChanged, s =>
            {

                if (s == MainViewModel.GUARD_MODE)
                {
                    Task.Factory.StartNew(HideSideBar);
                }
                else
                {
                    SideBar.Visibility = Visibility.Visible;
                }
            });
            
            
        }
        
        private DateTime _hideCompleted = DateTime.Now;
        private async void HideSideBar()
        {
            _lastActivity = DateTime.Now;
            while ((DateTime.Now - _lastActivity).TotalMilliseconds < 4444)
                await TaskEx.Delay(10);

            if (MainViewModel.Instance.Screen != MainViewModel.GUARD_MODE)
                return;

            awooo.Context.Post(d =>
            {
                if (SideBarWidth == 0 && SideBar.ActualWidth > 0)
                {
                    SideBarWidth = SideBar.ActualWidth;
                  //  SideBar.Width = SideBarWidth;
                }

                //var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100));
                //anim.Completed += (sender, args) =>
                //{
                    _hideCompleted = DateTime.Now;

                SideBar.Visibility = Visibility.Collapsed;
                //};
                //SideBar.BeginAnimation(WidthProperty, anim);
            }, null);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _lastActivity = DateTime.Now;
            if (SideBar.ActualWidth == 0 && SideBarWidth>0)
            {
                if ((DateTime.Now - _hideCompleted).TotalMilliseconds < 777) return;

                SideBar.Visibility = Visibility.Visible;
                //var anim = new DoubleAnimation(SideBarWidth, TimeSpan.FromMilliseconds(100));
                //SideBar.BeginAnimation(WidthProperty, anim);
                
                Task.Factory.StartNew(HideSideBar);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            // Config.General.WindowMaximized = WindowState == WindowState.Maximized;
            if (WindowState != WindowState.Minimized)
                Config.General.WindowState = WindowState;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnClosed(EventArgs e)
        {
            if(WindowState != WindowState.Maximized)
            {
                Config.General.WindowWidth = ActualWidth;
                Config.General.WindowHeight = ActualHeight;
                Config.General.WindowLeft = Left;
                Config.General.WindowTop = Top;
                Config.General.WindowMaximized = WindowState == WindowState.Maximized;
            }

            base.OnClosed(e);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (Config.General.ConfirmExit)
            {
                e.Cancel = true;
                var dlg = new MessageDialog("CONFIRM EXIT",
                    "Are you sure you want to exit?", PackIconKind.ExitToApp, "YES", true, "NO");
                await this.ShowDialog(dlg, (sender, args) => {}, (sender, args) =>
                {
                    if (!(args.Parameter as bool? ?? false)) return;
                        
                    RfidScanner.UnHook();
                    Application.Current.Shutdown(0);
                });
                
            }
            
            RfidScanner.UnHook();
            base.OnClosing(e);
        }
        
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            RfidScanner.Hook(this);
            base.OnSourceInitialized(e);
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeClicked(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void MinimizeClicked(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance.Screen == MainViewModel.GUARD_MODE) return;
            WindowState = WindowState.Minimized;
        }

        private void Menu_OnKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ContextMenu.PlacementTarget = (UIElement) sender;
            ContextMenu.Placement = PlacementMode.Right;
            ContextMenu.IsOpen = true;
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
          
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (MainViewModel.Instance.Screen == MainViewModel.GUARD_MODE) Activate();
        }
    }
}
