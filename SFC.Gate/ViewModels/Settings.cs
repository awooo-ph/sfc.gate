using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using SFC.Gate.Configurations;
using SFC.Gate.Material.Properties;

namespace SFC.Gate.Material.ViewModels
{
    partial class SettingsViewModel
    {
        
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

        private ICommand _changeBackgroundCommand;

        public ICommand ChangeBackgroundCommand =>
            _changeBackgroundCommand ?? (_changeBackgroundCommand = new DelegateCommand(
                d =>
                {
                    var filename = Extensions.GetPicture();
                    File.Copy("bg.jpg","bgx.jpg",true);
                    File.Copy(filename,"bg.jpg",true);
                    OnPropertyChanged(nameof(BackgroundPath));
                }));
        
        public string BackgroundPath { get; } = Config.General.Background;

        private List<string> _printers;
        public List<string> Printers
        {
            get
            {
                if (_printers != null) return _printers;
                _printers = new List<string>();
                var serv = new System.Printing.LocalPrintServer();
                var pqs = serv.GetPrintQueues();
                foreach (var pq in pqs)
                {
                    _printers.Add(pq.Name);
                }
                return _printers;
            }
        }
    }
}
