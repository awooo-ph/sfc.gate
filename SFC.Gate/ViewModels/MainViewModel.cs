using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SFC.Gate.Models;
using MsgBox = System.Windows.MessageBox;

namespace SFC.Gate.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        
        private MainViewModel() { }
        
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new MainViewModel();
                
                return _instance;
            }
        }


        private ICommand _addViolationCommand;

        public ICommand AddViolationCommand => _addViolationCommand ?? (_addViolationCommand = new DelegateCommand(stud =>
        {
            Violations.Instance.SelectedStudent = Students.Instance.SelectedStudent;
            SelectedTab = 2;
        }));

        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }
    }
}
