using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SFC.Gate.Configurations;
using SFC.Gate.Material.Views;
using SFC.Gate.Models;
using SFC.Gate.ViewModels;

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
            
            Messenger.Default.AddListener<Gate.ViewModels.Sms>(Messages.SmsReceived, sms =>
            {
                if (Config.Sms.ShowReceivedSms)
                {
                    lock (_smsLock)
                        _smsQueue.Enqueue(sms);
                    ShowMessages();
                }
                if (Config.Sms.ForwardReceivedSms && Config.Sms.ForwardSmsTo.IsCellNumber())
                {
                    var stud = Student.GetByNumber(sms.Sender);
                    var sender = stud != null ? $"{stud.Fullname}'s Parent" : $"UNKNOWN [{sms.Sender}]";
                    SMS.Send($"Message from: {sender}\n{sms.Message}",Config.Sms.ForwardSmsTo);
                }
                if (Config.Sms.EnableAutoReply && !string.IsNullOrEmpty(Config.Sms.AutoReply))
                {
                    SMS.Send(Config.Sms.AutoReply,sms.Sender);
                }
            });
        }

        private object _smsLock = new object();
        private Queue<Gate.ViewModels.Sms> _smsQueue = new Queue<Gate.ViewModels.Sms>();
        private bool _messagesRunning;

        private void ShowMessages()
        {
            lock(_smsLock)
                if(_smsQueue.Count==0) return;

            if (_messagesRunning)
                return;
            _messagesRunning = true;
            
            Gate.ViewModels.Sms sms = null;
            lock (_smsLock)
                sms = _smsQueue.Dequeue();
            if (sms == null)
            {
                _messagesRunning = false;
                return;
            }
            
            awooo.Context.Post(d =>
            {
                var stud = Student.GetByNumber(sms.Sender);
                var title = $"UNKNOWN [{sms.Sender}]";
                if (stud != null)
                    title = $"{stud.Fullname}'s Parent";

                var dlg = new MessageDialog(title, sms.Message,
                    PackIconKind.MessageText, "CLOSE");
                dlg.Show(() =>
                {
                    _messagesRunning = false;
                    ShowMessages();
                });
            }, null);
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
            awooo.Context.Post(d =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            },null);
            
        }

        private static User _CurrentUser;
        
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
                RfidScanner.ExclusiveCallback = null;
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
                    if (string.IsNullOrEmpty(filename)) return;
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

        private DateTime _dialogUpdate;
        public async void ShowTimeCard(DailyTimeRecord timeCard)
        {
          
            TimeCard = new FacultyInfoDialog() {DataContext = timeCard};
            
            _dialogUpdate = DateTime.Now;

            if (Instance.IsDialogOpen) return;

            Instance.IsDialogOpen = true;
                var delay = Config.Rfid.StudentInfoDelay * 1000;
                while ((DateTime.Now - _dialogUpdate).TotalMilliseconds < delay)
                   await TaskEx.Delay(100);

                Instance.IsDialogOpen = false;
        }

        public class TimeCardContext
        {
            private DailyTimeRecord _TimeCard;

            public DailyTimeRecord Value { get; set; }

            public TimeCardContext(DailyTimeRecord card)
            {
                Value = card;
            }
        }

        private object _TimeCard;

        public object TimeCard
        {
            get => _TimeCard;
            set
            {
                _TimeCard = value;
                OnPropertyChanged(nameof(TimeCard));
            }
        }
        
        private bool _IsDialogOpen;

        public bool IsDialogOpen
        {
            get => _IsDialogOpen;
            set
            {
                _IsDialogOpen = value;
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }
    }
}
