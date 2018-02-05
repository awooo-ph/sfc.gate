using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class Users : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Users()
        {
            Messenger.Default.AddListener<int>(Messages.ScreenChanged, s =>
            {
                if (s == MainViewModel.USERS)
                    RfidScanner.ExclusiveCallback = ScanCallback;
            });
        }
        private static Users _instance;
        public static Users Instance => _instance ?? (_instance = new Users());

        private ICommand _addUserCommand;

        public ICommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new DelegateCommand(d =>
        {
            ShowAddItem = true;
            NewItem = new User() {Picture = Extensions.Generate()};
        }));

        private bool _ShowAddITem;

        public bool ShowAddItem
        {
            get => _ShowAddITem;
            set
            {
                if(value == _ShowAddITem)
                    return;
                _ShowAddITem = value;
                OnPropertyChanged(nameof(ShowAddItem));
            }
        }

        private User _NewItem;

        public User NewItem
        {
            get => _NewItem;
            set
            {
                if(value == _NewItem)
                    return;
                _NewItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        private ICommand _cancelAddCommand;

        public ICommand CancelAddCommand => _cancelAddCommand ?? (_cancelAddCommand = new DelegateCommand(d =>
        {
            ShowAddItem = false;
        }));

        private ICommand _acceptAddCommand;

        public ICommand AcceptAddCommand => _acceptAddCommand ?? (_acceptAddCommand = new DelegateCommand(d =>
        {
            NewItem?.Save();
            ShowAddItem = false;
        },d=>NewItem?.CanSave()??false));

        private Action<string> ScanCallback = null;

        private bool _ShowRfidDialog;

        public bool ShowRfidDialog
        {
            get => _ShowRfidDialog;
            set
            {
                if(value == _ShowRfidDialog)
                    return;
                _ShowRfidDialog = value;
                OnPropertyChanged(nameof(ShowRfidDialog));
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _cancelRfidCommand;

        public ICommand CancelRfidCommand => _cancelRfidCommand ?? (_cancelRfidCommand = new DelegateCommand(d =>
        {
            ScanCallback = null;
            ShowRfidDialog = false;
        }));

        private ICommand _changeRfidCommand;
        private DateTime _lastScan = DateTime.Now;
        private bool _invalidScanShown;
        public ICommand ChangeRfidCommand => _changeRfidCommand ?? (_changeRfidCommand = new DelegateCommand<User>(
            user =>
            {
                ChangeRfidMessage = "PLEASE SCAN CARD";
                ScanCallback = async s =>
                {
                    var usr = User.Cache.FirstOrDefault(x => x.Rfid.ToUpper() == s.ToUpper() && x.Id != user.Id);
                    if (usr != null)
                    {
                        InvalidRfidMessage = $"ALREADY USED BY {usr.Username.ToUpper()}";
                        IsNewRfidInvalid = true;
                    }
                    if(Visit.GetByRfid(s).Count > 0)
                    {
                        InvalidRfidMessage = "CANNOT USE VISITOR'S CARD";
                        IsNewRfidInvalid = true;
                    }

                    if(IsNewRfidInvalid)
                    {
                        if(_invalidScanShown)
                            return;
                        _invalidScanShown = true;
                        _lastScan = DateTime.Now;
                        await Task.Factory.StartNew(async () =>
                        {
                            while((DateTime.Now - _lastScan).TotalMilliseconds < 4444)
                                await TaskEx.Delay(100);
                            _invalidScanShown = false;
                            IsNewRfidInvalid = false;
                        });
                    } else
                    {
                        var stud = Student.Cache.FirstOrDefault(x => x.Rfid.ToLower() == s.ToLower());
                        user.Update(nameof(User.Rfid), s);
                        
                        ChangeRfidMessage = stud?.Fullname ?? s;
                        
                        await TaskEx.Delay(1000);
                        
                        ShowRfidDialog = false;
                        ScanCallback = null;
                    }
                };

                RfidScanner.ExclusiveCallback = ScanCallback;

                ShowRfidDialog = true;
            }));

        public bool IsDialogOpen => ShowRfidDialog;
        private bool _IsNewRfidInvalid;

        public bool IsNewRfidInvalid
        {
            get => _IsNewRfidInvalid;
            set
            {
                if(value == _IsNewRfidInvalid)
                    return;
                _IsNewRfidInvalid = value;
                OnPropertyChanged(nameof(IsNewRfidInvalid));
            }
        }

        private string _InvalidRfidMessage;

        public string InvalidRfidMessage
        {
            get => _InvalidRfidMessage;
            set
            {
                if(value == _InvalidRfidMessage)
                    return;
                _InvalidRfidMessage = value;
                OnPropertyChanged(nameof(InvalidRfidMessage));
            }
        }

        private string _ChangeRfidMessage = "PLEASE SCAN CARD";

        public string ChangeRfidMessage
        {
            get => _ChangeRfidMessage;
            set
            {
                if(value == _ChangeRfidMessage)
                    return;
                _ChangeRfidMessage = value;
                OnPropertyChanged(nameof(ChangeRfidMessage));
            }
        }

        
    }
}
