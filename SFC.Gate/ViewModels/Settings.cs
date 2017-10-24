using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SFC.Gate.Configurations;

namespace SFC.Gate.ViewModels
{
    class Settings:ViewModelBase
    {
        private Settings()
        {
            Messenger.Default.AddListener<string>(Messages.Scan, id =>
            {
                ScanTest = id;
            });
            
            Messenger.Default.AddListener(Messages.ScannerRegistered, () =>
            {
                Context.Post(d => OnPropertyChanged(nameof(RegisteredScanner)), null);
            });
        }

        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());
        
        private string _scanTest;

        public string ScanTest
        {
            get { return _scanTest; }
            set
            {
                _scanTest = value; 
                OnPropertyChanged(nameof(ScanTest));
            }
        }

        private ICommand _registerCommand;

        public ICommand RegisterCommand => _registerCommand ?? (_registerCommand = new DelegateCommand(d =>
        {
            RfidScanner.RegisterScanner();
            OnPropertyChanged(nameof(RegisteredScanner));
        }));

        private ICommand _cancelRegisterCommand;

        public ICommand CancelRegisterCommand => _cancelRegisterCommand ?? (_cancelRegisterCommand = new DelegateCommand(d =>
        {
            RfidScanner.CancelRegistration();
            OnPropertyChanged(nameof(RegisteredScanner));
        }));

        public string RegisteredScanner
        {
            get
            {
                if (RfidScanner.IsWaitingForScanner)
                    return "Waiting for Scanner...";
                if (string.IsNullOrEmpty(Config.Rfid.Scanner))
                    return "No Scanner Registered";
                return Config.Rfid.Scanner;
            }
        }

    }
}
