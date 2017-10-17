using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using SFC.Gate.Models;

namespace SFC.Gate.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public static SynchronizationContext Context { get; set; }
        private MainViewModel() { }
        
        private static MainViewModel _instance;
        public static MainViewModel Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new MainViewModel();
                Messenger.Default.AddListener<Student>(Messages.DuplicateRfid, NotifyDuplicateId);
                return _instance;
            }
        }

        private static async void NotifyDuplicateId(Student stud)
        {
            var res = await MessageBox.Show("Cannot Add New Student", $"{stud.Firstname} {stud.Lastname} is already using RFID Code [{stud.Rfid}].",
                "Each student must have a unique RFID.\n" +
                $"Click CANCEL to cancel adding {Instance.NewStudentHolder.Firstname} {Instance.NewStudentHolder.Lastname}.\n" +
                $"Click REMOVE to remove {stud.Firstname} {stud.Lastname} from the database.\n" +
                $"What do you want to do?",
                $"CANCEL Adding New Student",
                $"REMOVE Old Student");
            if (res == MessageBox.MessageBoxResults.Button2)
            {
                
            }
        }

        private Student _newStudentHolder;
        public Student NewStudentHolder => _newStudentHolder ?? (_newStudentHolder = new Student());
    }
}
