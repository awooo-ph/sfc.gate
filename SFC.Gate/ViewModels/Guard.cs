using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
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
          //  Messenger.Default.AddListener(Messages.ConfigChanged,);
            Config.General.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(Background));
            };
            Messenger.Default.AddListener<string>(Messages.Scan, id =>
            {
                Student = Student.Cache.FirstOrDefault(x => x.Rfid.ToUpper() == id);
                if (Student == null)
                    Instance.Index = InvalidIndex;
                else
                {


                    var pass = Student.Pass();
                    switch (pass)
                    {
                        case Student.PassReturnValues.Ignored:
                            Student = null;
                            return;
                        case Student.PassReturnValues.Entry:
                            Welcome = "WELCOME";
                            break;
                        case Student.PassReturnValues.Exit:
                            Welcome = "GOODBYE";
                            break;
                    }

                    if (!MainViewModel.Instance.IsGuardMode && Config.General.GuardModeOnScan)
                    {
                        MainViewModel.Instance.IsGuardMode = true;
                    }
                    
                    Instance.Index = StudentIndex;
                    _infoTimer?.Dispose();
                    if (Config.Rfid.StudentInfoDelay > 0)
                        _infoTimer = new Timer(HideStudentInfo, null, Config.Rfid.StudentInfoDelay*1000, int.MaxValue);

                }
            });
        }

        private string _welcome;

        public string Welcome
        {
            get { return _welcome; }
            private set
            {
                _welcome = value; 
                OnPropertyChanged(nameof(Welcome));
            }
        }

        private void HideStudentInfo(object state)
        {
            Index = 0;
            Student = null;
            _infoTimer.Dispose();
        }


        public SolidColorBrush Background => Config.General.GuardModeBackground;

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
                if (_student == null)
                {
                    StandbyVisibility = Visibility.Visible;
                    StudentInfoVisibility = Visibility.Collapsed;
                }
                else
                {
                    StandbyVisibility = Visibility.Collapsed;
                    StudentInfoVisibility = Visibility.Visible;
                }
            }
        }

        private Visibility _standbyVisibility = Visibility.Visible;

        public Visibility StandbyVisibility
        {
            get
            {
                return _standbyVisibility;
            }
            set
            {
                _standbyVisibility = value; 
                OnPropertyChanged(nameof(StandbyVisibility));
            }
        }

        private Visibility _studentInfoVisibility = Visibility.Collapsed;

        public Visibility StudentInfoVisibility
        {
            get
            {
                return _studentInfoVisibility;
            }
            set
            {
                _studentInfoVisibility = value; 
                OnPropertyChanged(nameof(StudentInfoVisibility));
            }
        }
    }
}
