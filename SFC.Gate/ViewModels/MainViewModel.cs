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
                
                Messenger.Default.AddListener(Messages.DuplicateName, NotifyDuplicateName);
                
                return _instance;
            }
        }
        
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

        private static void NotifyDuplicateName()
        {
            Context.Post(d =>
            {
                MsgBox.Show($"{Students.Instance.NewStudentHolder.Fullname} is already in the database.",
                    "Student Already Exists", MessageBoxButton.OK, MessageBoxImage.Hand);
                Instance.SelectedTab = 1;
            }, null);
        }
        
    }
}
