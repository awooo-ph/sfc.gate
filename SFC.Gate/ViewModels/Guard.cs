using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using SFC.Gate.Configurations;
using SFC.Gate.Models;
using Sms = SFC.Gate.Material.Views.Sms;

namespace SFC.Gate.Material.ViewModels
{
    class Guard : INotifyPropertyChanged
    {
        private const long ClockIndex = 0, StudentIndex = 1, InvalidIndex = 2;
        private Timer _infoTimer;

        private bool _Defer;

        public bool IgnoreScans
        {
            get => _Defer;
            set
            {
                if(value == _Defer)
                    return;
                _Defer = value;
                OnPropertyChanged(nameof(IgnoreScans));
            }
        }
        
        private Guard()
        {
            Index = InvalidIndex;
            
          //  Messenger.Default.AddListener(Messages.ConfigChanged,);
            Config.General.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(Background));
            };
            Messenger.Default.AddListener<string>(Messages.Scan, id =>
            {
                if (MainViewModel.Instance.Screen==MainViewModel.VISITORS && 
                    (VisitorsViewModel.Instance.IsAddingVisitor || VisitorsViewModel.Instance.IsReturningCard)) return;
                
                if(!Config.Rfid.GlobalScan && MainViewModel.Instance.Screen!=3) return;
                if (Config.Rfid.RequireUser && !MainViewModel.Instance.HasLoggedIn) return;
                
                Student = Student.Cache.FirstOrDefault(x => x.Rfid.ToUpper() == id);
                if (Student == null)
                    Instance.Index = InvalidIndex;
                else
                {


                    var pass = Student.Pass();
                    var msg = "";
                    switch (pass)
                    {
                        case Student.PassReturnValues.Ignored:
                            Student = null;
                            return;
                        case Student.PassReturnValues.Entry:
                            Welcome = "WELCOME";
                            msg = SFC.Gate.Configurations.Sms.Default.EntryTemplate;
                            msg = msg.Replace("[STUDENT]", Student.Fullname);
                            msg = msg.Replace("[TIME]", DateTime.Now.ToString("g"));
                            break;
                        case Student.PassReturnValues.Exit:
                            Welcome = "GOODBYE";
                            msg = SFC.Gate.Configurations.Sms.Default.ExitTemplate;
                            msg = msg.Replace("[STUDENT]", Student.Fullname);
                            msg = msg.Replace("[TIME]", DateTime.Now.ToString("g"));
                            break;
                    }
                    if (msg != "")
                    {
                        if(Config.Sms.Enabled)
                            SFC.Gate.ViewModels.SMS.Send(msg,Student.ContactNumber);
                    }
                    
                    Instance.Index = StudentIndex;
                    _infoTimer?.Dispose();
                    if (Config.Rfid.StudentInfoDelay > 0)
                        _infoTimer = new Timer(HideStudentInfo, null, Config.Rfid.StudentInfoDelay * 1000,
                            int.MaxValue);
                    
                    if(MainViewModel.Instance.Screen != MainViewModel.GUARD_MODE)
                    {
                        var m = Welcome == "WELCOME" ? "entered" : "left";
                        MainViewModel.ShowMessage($"{Student.Fullname} {Student.Department} - {Student.YearLevel} has {m} the campus.",
                            "VIEW STUDENT", s =>
                            {
                                StudentsViewModel.Instance.ShowStudent(s);
                            },Student);
                    }
                    
                    

                }
            });
        }

        private string _welcome = "WELCOME";

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

        private long _index = StudentIndex;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
