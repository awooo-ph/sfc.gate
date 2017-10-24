using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

namespace SFC.Gate.ViewModels
{
    class Guard : ViewModelBase
    {
        private const long ClockIndex = 0, StudentIndex = 1, InvalidIndex = 2;
        private Timer _infoTimer;
        
        private Guard()
        {
            Messenger.Default.AddListener<string>(Messages.Scan, id =>
            {
                Student = Student.Cache.FirstOrDefault(x => x.Rfid == id);
                if (Student == null)
                    Instance.Index = InvalidIndex;
                else
                {
                    Instance.Index = StudentIndex;
                    _infoTimer?.Dispose();
                    if (Config.Rfid.StudentInfoDelay > 0)
                        _infoTimer = new Timer(HideStudentInfo, null, 0,Config.Rfid.StudentInfoDelay*1000);
                }
            });
        }

        private void HideStudentInfo(object state)
        {
            Index = 0;
            Student = null;
        }


        private static Guard _instance;
        public static Guard Instance => _instance ?? (_instance = new Guard());

        private long _index;

        public long Index
        {
            get => _index;
            private set
            {
                _index = value; 
                OnPropertyChanged(nameof(Index));
            }
        }
        

        private Student _student;

        public Student Student
        {
            get => _student;
            private set
            {
                _student = value; 
                OnPropertyChanged(nameof(Student));
            }
        }

    }
}
