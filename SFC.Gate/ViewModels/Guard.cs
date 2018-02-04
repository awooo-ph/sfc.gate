using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

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
            Messenger.Default.AddListener<string>(Messages.Scan, ProcessScan);
        }

        public static Action<string> RedirectScan { get; set; }

        private void ProcessScan(string id)
        {
            if (RedirectScan != null)
            {
                RedirectScan(id);
                return;
            }
            //Ignore when adding new visitor or returning visitor card.
            if(MainViewModel.Instance.Screen == MainViewModel.VISITORS &&
                (VisitorsViewModel.Instance.IsAddingVisitor || VisitorsViewModel.Instance.IsReturningCard))
                return;

            //Ignore if not on Guard Mode and GlobalScan is disabled.
            if(!Config.Rfid.GlobalScan && MainViewModel.Instance.Screen != MainViewModel.GUARD_MODE)
                return;

            //Ignore if no user has logged in and is required.
            if(Config.Rfid.RequireUser && !MainViewModel.Instance.HasLoggedIn)
                return;


            Student = Student.Cache.FirstOrDefault(x => x.Rfid.ToUpper() == id);
            if(Student == null)
                ShowInvalid(id);
            else
            {
                //We got a valid scan now check if it is an incoming, outgoing or too soon.   
                var pass = Student.Pass(MainViewModel.Instance.CurrentUser?.Id);

                if(pass == Student.PassReturnValues.Ignored)
                    return;

                Welcome = pass == Student.PassReturnValues.Entry ? "WELCOME" : "GOODBYE";

                Instance.Index = StudentIndex;
                _infoTimer?.Dispose();
                if(Config.Rfid.StudentInfoDelay > 0)
                    _infoTimer = new Timer(HideStudentInfo, null, Config.Rfid.StudentInfoDelay * 1000,
                        int.MaxValue);

                //Show toasts when not in guard mode
                if(MainViewModel.Instance.Screen != MainViewModel.GUARD_MODE)
                {
                    var m = Welcome == "WELCOME" ? "entered" : "left";
                    MainViewModel.ShowMessage($"{Student.Fullname} {Student.Department} - {Student.YearLevel} has {m} the campus.",
                        "VIEW STUDENT", s => StudentsViewModel.Instance.ShowStudent(s), Student);
                }
            }
        }

        private DateTime _lastShownInvalid = DateTime.Now;
        private void ShowInvalid(string id)
        {
            _lastShownInvalid = DateTime.Now;
            var student = Student.GetByRfid(id);
            InvalidTitle = "INVALID CARD";
            InvalidMessage = "The card is not registered in the system. Please ask for assistance.";
            if (student != null)
            {
                InvalidTitle = "CARD HAS EXPIRED";
                InvalidMessage = $"Hello {student.Fullname}! Your card is no longer active. Please ask the guard on duty for assistance.";
                Log.Add("SWIPE",
                    $"Someone attempted to use an expired card. This card was previously owned by {student.Fullname}.");
            }
            else
            {
                var visits = Visit.GetByRfid(id);
                if (visits != null)
                {
                    if (visits.Any(x => !x.HasLeft))
                    {
                        var v = visits.FirstOrDefault(x => !x.HasLeft);
                        InvalidTitle = $"HELLO {v?.Visitor.Name}";
                        InvalidMessage = "Are you leaving already? Please return the card to the guard. Thank you!";
                        Log.Add("SWIPE", $"{v?.Visitor.Name} has swiped the card issued to him/her on {v.TimeIn:g}.");
                    }
                    else
                    {
                        InvalidTitle = "INVALID VISITOR'S CARD";
                        InvalidMessage =
                            "You are using a VISITOR'S CARD which is currently not issued. Please ask the guard for assistance.";
                        Log.Add("SWIPE", $"An unissued card is swiped. Card ID#: {id}");
                    }
                }
                else
                {
                    Log.Add("SWIPE", "Unknown card is swiped.");
                }
            }
            
            IsInvalidShown = true;
            Task.Factory.StartNew(async () =>
            {
                while ((DateTime.Now - _lastShownInvalid).TotalSeconds < Config.Rfid.StudentInfoDelay)
                    await TaskEx.Delay(10);

                 IsInvalidShown = false;
            });
        }

        private string _InvalidTitle;

        public string InvalidTitle
        {
            get => _InvalidTitle;
            set
            {
                if(value == _InvalidTitle)
                    return;
                _InvalidTitle = value;
                OnPropertyChanged(nameof(InvalidTitle));
            }
        }

        

        private string _InvalidMessage;

        public string InvalidMessage
        {
            get => _InvalidMessage;
            set
            {
                if(value == _InvalidMessage)
                    return;
                _InvalidMessage = value;
                OnPropertyChanged(nameof(InvalidMessage));
            }
        }

        private bool _IsInvalidShown;

        public bool IsInvalidShown
        {
            get => _IsInvalidShown;
            set
            {
                if(value == _IsInvalidShown)
                    return;
                _IsInvalidShown = value;
                OnPropertyChanged(nameof(IsInvalidShown));
                if (value)
                {
                    StudentInfoVisibility = Visibility.Collapsed;
                    StandbyVisibility = Visibility.Collapsed;
                }
                else
                {
                    _standbyVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(StandbyVisibility));
                }
            }
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
                    _IsInvalidShown = false;
                    OnPropertyChanged(nameof(IsInvalidShown));
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
            awooo.Context.Post(d => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
        }
    }
}
