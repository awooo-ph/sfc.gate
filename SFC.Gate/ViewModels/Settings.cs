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
                Context.Post(d => OnPropertyChanged(""), null);
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
            OnPropertyChanged("");
        }));

        private ICommand _cancelRegisterCommand;

        public ICommand CancelRegisterCommand => _cancelRegisterCommand ?? (_cancelRegisterCommand = new DelegateCommand(d =>
        {
            RfidScanner.CancelRegistration();
            OnPropertyChanged("");
        }));

        public string ScannerId
        {
            get
            {
                if (RfidScanner.IsWaitingForScanner)
                    return "Waiting for Scanner...";
                if (string.IsNullOrEmpty(Config.Rfid.ScannerId))
                    return "No Scanner Registered";
                return Config.Rfid.ScannerId;
            }
        }
        

        public string ScannerType
        {
            get
            {
                if (RfidScanner.IsWaitingForScanner) return "N/A";
                if (string.IsNullOrEmpty(Config.Rfid.ScannerId)) return "N/A";
                return Config.Rfid.ScannerType;
            }
            private set
            {
                OnPropertyChanged(nameof(ScannerType));
            }
        }

        private string _scannerDescription;

        public string ScannerDescription
        {
            get
            {
                if (RfidScanner.IsWaitingForScanner) return "N/A";
                if (string.IsNullOrEmpty(Config.Rfid.ScannerId)) return "N/A";
                return Config.Rfid.Description;
            }
            private set
            {
                _scannerDescription = value; 
                OnPropertyChanged(nameof(ScannerDescription));
            }
        }

        private string _scannerFullname;

        public string ScannerFullname
        {
            get
            {
                if (RfidScanner.IsWaitingForScanner) return "N/A";
                if (string.IsNullOrEmpty(Config.Rfid.ScannerId)) return "N/A";
                return Config.Rfid.Fullname;

            }
            private set
            {
                _scannerFullname = value; 
                OnPropertyChanged(nameof(ScannerFullname));
            }
            
        }

        private ICommand _testScanCommnad;

        public ICommand TestScanCommand => _testScanCommnad ?? (_testScanCommnad = new DelegateCommand(d =>
        {
            //Todo: Test scanner
        }, d =>
        {
            if (RfidScanner.IsWaitingForScanner) return false;
            if (string.IsNullOrEmpty(Config.Rfid.ScannerId)) return false;
            return true;
        }));

        public bool IsRegistering => RfidScanner.IsWaitingForScanner;

    }
}
