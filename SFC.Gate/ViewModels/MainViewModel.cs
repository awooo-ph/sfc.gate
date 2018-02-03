using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public const int GUARD_MODE = 4;
        public const int SETTINGS = 5;
        public const int STUDENTS = 0;
        public const int FACULTY = 1;
        public const int VISITORS = 2;
        public const int LOGIN = 6;
        public const int USERS = 3;
        
        private MainViewModel()
        {
            Messenger.Default.AddListener<string>(Messages.SMS, msg =>
            {
                if (string.IsNullOrEmpty(msg)) return;
                MessageQueue.Enqueue($"SMS Notification: {msg}");
            });
        }

        private bool _ShowSideBar;
        
        public bool ShowSideBar
        {
            get
            {
                if (Screen == LOGIN) return false;
                return (_ShowSideBar && HasLoggedIn) || (Screen != GUARD_MODE);
            }
            set
            {
                if(value == _ShowSideBar)
                    return;
                _ShowSideBar = value;
                OnPropertyChanged(nameof(ShowSideBar));
            }
        }

        private ICommand _runExternalCOmmand;

        public ICommand RunExternalCommand => _runExternalCOmmand ?? (_runExternalCOmmand = new DelegateCommand<string>(
        cmd =>
        {
            if (string.IsNullOrWhiteSpace(cmd)) return;
            try
            {
                Process.Start(cmd);
            }
            catch (Exception e)
            {
                //
            }
            
        }));

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private User _CurrentUser;

        public User CurrentUser
        {
            get => _CurrentUser;
            set
            {
                if(value == _CurrentUser)
                    return;
                _CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(HasLoggedIn));
                OnPropertyChanged(nameof(ShowSideBar));
                OnPropertyChanged(nameof(IsContactVisible));
                if (value == null)
                    Screen = LOGIN;
            }
        }

        public bool IsContactVisible => CurrentUser?.IsAdmin ?? false || !Config.General.HideContactNumber;

        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>
        {
            CurrentUser = null;
        }));
        
        private SnackbarMessageQueue _messageQueue;
        public SnackbarMessageQueue MessageQueue => _messageQueue ?? (_messageQueue = new SnackbarMessageQueue(TimeSpan.FromMilliseconds(7777)));

        private int _Screen = LOGIN;
        
        public int Screen
        {
            get => _Screen;
            set
            {
                if (value != LOGIN && !HasLoggedIn)
                    _Screen = LOGIN;
                _Screen = value;
                OnPropertyChanged(nameof(Screen));
                Messenger.Default.Broadcast(Messages.ScreenChanged,value);
            }
        }

        public bool HasLoggedIn => CurrentUser != null;

        public static void ShowMessage(string message,string actionContent, Action action, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent, action, promote);
        }

        public static void ShowMessage<T>(string message, string actionContent, Action<T> action, T param, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent,action,param,promote);
        }

        private ICommand _GeneratePictureCommand;

        public ICommand GeneratePictureCommand =>
            _GeneratePictureCommand ?? (_GeneratePictureCommand = new DelegateCommand(
                d =>
                {
                    if (CurrentUser == null) return;
                    var pic = CurrentUser.Picture;
                    CurrentUser.Update(nameof(User.Picture),Extensions.Generate());
                    ShowMessage("Picture changed","UNDO",()=>CurrentUser.Update("Picture",pic));
                },d=>CurrentUser!=null));

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand(
                d =>
                {
                    if (CurrentUser == null)
                        return;
                    var filename = Extensions.GetPicture();
                    var image = CurrentUser.Picture;
                    CurrentUser.Update(nameof(User.Picture) ,Extensions.ResizeImage(filename));
                    ShowMessage("Picture changed", "UNDO", () => CurrentUser.Update("Picture", image));
                    
                },d=>CurrentUser!=null));

        private bool _ShowUserMenu;

        public bool ShowUserMenu
        {
            get => _ShowUserMenu;
            set
            {
                if(value == _ShowUserMenu)
                    return;
                _ShowUserMenu = value;
                OnPropertyChanged(nameof(ShowUserMenu));
            }
        }

        private int _SettingIndex;

        public int SettingIndex
        {
            get => _SettingIndex;
            set
            {
                _SettingIndex = value;
                OnPropertyChanged(nameof(SettingIndex));
            }
        }

        private ICommand _showUserProfileCommand;

        public ICommand ShowUserProfileCommand =>
            _showUserProfileCommand ?? (_showUserProfileCommand = new DelegateCommand(
                d =>
                {
                    Screen = SETTINGS;
                    SettingIndex = 1;
                    
                }));

        private ICommand _showUserMenuCommand;

        public ICommand ShowUserMenuCommand => _showUserMenuCommand ?? (_showUserMenuCommand = new DelegateCommand(d =>
        {
            ShowUserMenu = !ShowUserMenu;
        }));

        private ICommand _showDevCommand;
       
        public ICommand ShowDevCommand => _showDevCommand ?? (_showDevCommand = new DelegateCommand(d =>
        {
            Screen = SETTINGS;
            SettingIndex = 4;
        }));

    }
}
