using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using SFC.Gate.Configurations;
using SFC.Gate.Models;
using SFC.Gate.ViewModels;
using Xceed.Words.NET;

namespace SFC.Gate.Material.ViewModels
{
    class StudentsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            awooo.Context.Post(d =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }, null);
        }

        private StudentsViewModel()
        {
            Messenger.Default.AddListener<Student>(Messages.ModelDeleted, student =>
                {
                    MainViewModel.ShowMessage($"A student was deleted.", "UNDO", () =>
                    {
                        
                            Log.Add("REVERT", $"{student.Fullname} is undeleted.", "Students", student.Id);
                            student.Undelete();
                        
                    });
                });

    Messenger.Default.AddListener<Student>(Messages.ModelSelected, s =>
            {
                if (s.Level > Departments.College) return;
                var students = Student.Cache.Where(FilterStudents).ToList();
                bool? sel = null;
                foreach (var student in students)
                {
                    if (sel == null)
                    {
                        sel = student.IsSelected;
                    }
                    else
                    {
                        if (sel != student.IsSelected)
                        {
                            sel = null;
                            break;
                        }
                    }
                }
                _SelectionState = sel;
                OnPropertyChanged(nameof(SelectionState));
                OnPropertyChanged(nameof(HasSelected));
            });
            
            Messenger.Default.AddListener<int>(Messages.ScreenChanged, screen =>
            {
                if (screen == MainViewModel.STUDENTS)
                {
                    RfidScanner.ExclusiveCallback = ScanCallback;
                }
                else
                {
                    InvalidList.ForEach(x=>x.Reset());
                    InvalidList.Clear();
                }
            });
            
            Messenger.Default.AddListener<Student>(Messages.CommitError, student =>
            {
                if (MainViewModel.Instance.Screen != MainViewModel.STUDENTS) return;
                
                if (student.Id == 0 &&
                    string.IsNullOrEmpty(student.Firstname) &&
                    string.IsNullOrEmpty(student.Lastname) &&
                    string.IsNullOrEmpty(student.ContactNumber) &&
                    string.IsNullOrEmpty(student.StudentId))
                {
                    Student.Cache.Remove(student);
                }
                else
                {
                    InvalidList.Add(student);
                    MainViewModel.ShowMessage($"INVALID DATA: {student.GetLastError()}", null, null);
                }
                
            });
        }
        
        private List<Student> InvalidList = new List<Student>();

        private Action<string> ScanCallback = null;

        private bool _ShowRfidDialog;

        public bool ShowRfidDialog
        {
            get => _ShowRfidDialog;
            set
            {
                if(value == _ShowRfidDialog)
                    return;
                _ShowRfidDialog = value;
                OnPropertyChanged(nameof(ShowRfidDialog));
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _cancelRfidCommand;

        public ICommand CancelRfidCommand => _cancelRfidCommand ?? (_cancelRfidCommand = new DelegateCommand(d =>
        {
            ScanCallback = null;
            ShowRfidDialog = false;
        }));

        private ICommand _changeRfidCommand;
        private DateTime _lastScan = DateTime.Now;
        private bool _invalidScanShown;
        public ICommand ChangeRfidCommand => _changeRfidCommand ?? (_changeRfidCommand = new DelegateCommand<Student>(
            stud =>
            {
                ScanCallback = s =>
                {
                    if (Student.Cache.Any(x => x.Rfid?.ToLower() == s.ToLower()))
                    {
                        InvalidRfidMessage = "INVALID! CARD IS IN USE";
                        IsNewRfidInvalid = true;
                    }
                    if (Visit.GetByRfid(s).Count > 0)
                    {
                        InvalidRfidMessage = "ALREADY REGISTERED AS VISITOR'S CARD";
                        IsNewRfidInvalid = true;
                    }

                    if (IsNewRfidInvalid)
                    {
                        if (_invalidScanShown) return;
                        _invalidScanShown = true;
                        _lastScan = DateTime.Now;
                        Task.Factory.StartNew(async () =>
                        {
                            while ((DateTime.Now - _lastScan).TotalMilliseconds < 4444)
                                await TaskEx.Delay(100);
                            _invalidScanShown = false;
                            IsNewRfidInvalid = false;
                        });
                    }
                    else
                    {
                        stud.Update(nameof(Student.Rfid),s);
                        ShowRfidDialog = false;
                        ScanCallback = null;
                    }
                };

                RfidScanner.ExclusiveCallback = ScanCallback;

                ShowRfidDialog = true;
            },d=>d?.Id>0 && (MainViewModel.Instance.CurrentUser?.IsAdmin??false)));

        public bool IsDialogOpen => ShowSmsDialog || ShowRfidDialog;
        private bool _IsNewRfidInvalid;

        public bool IsNewRfidInvalid
        {
            get => _IsNewRfidInvalid;
            set
            {
                if(value == _IsNewRfidInvalid)
                    return;
                _IsNewRfidInvalid = value;
                OnPropertyChanged(nameof(IsNewRfidInvalid));
            }
        }

        private string _InvalidRfidMessage;

        public string InvalidRfidMessage
        {
            get => _InvalidRfidMessage;
            set
            {
                if(value == _InvalidRfidMessage)
                    return;
                _InvalidRfidMessage = value;
                OnPropertyChanged(nameof(InvalidRfidMessage));
            }
        }

        private string _ChangeRfidMessage = "PLEASE SCAN CARD";

        public string ChangeRfidMessage
        {
            get => _ChangeRfidMessage;
            set
            {
                if(value == _ChangeRfidMessage)
                    return;
                _ChangeRfidMessage = value;
                OnPropertyChanged(nameof(ChangeRfidMessage));
            }
        }

        

        private static StudentsViewModel _instance;
        public static StudentsViewModel Instance => _instance ?? (_instance = new StudentsViewModel());

        private ListCollectionView _students;

        public ListCollectionView Students
        {
            get
            {
                if (_students != null)
                    return _students;
                _students = new ListCollectionView(Models.Student.Cache);
                _students.Filter = FilterStudents;
               
                Models.Student.Cache.CollectionChanged += (sender, args) =>
                {
                    if(!_students.IsAddingNew)
                        _students.Filter = FilterStudents;
                };
                return _students;
            }
        }

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand =
            new DelegateCommand<Student>(
                stud =>
                {
                    if (!MainViewModel.Instance.CurrentUser?.IsAdmin ?? false) return;
                    
                    var file = Extensions.GetPicture();
                    if (file == null)
                        return;
                    var oldPic = stud.Picture;
                    stud.Update(nameof(Student.Picture),
                        Extensions.ResizeImage(file));
                    Log.Add("REVERT", $"{stud.Fullname}'s picture was changed.",
                        "Students", stud.Id);
                    MainViewModel.ShowMessage("Picture Changed", "UNDO", () =>
                    {
                        stud.Update(nameof(Student.Picture), oldPic);
                        Log.Add("REVERT",
                            $"{stud.Fullname}'s picture changed was undone.",
                            "Students", stud.Id);
                    });
                }, s => s != null && s.Id>0));

        
        private DateTime _lastSearch = DateTime.Now;
        private Task _searchTask;
        
        private string _StudentsKeyword;
        private bool _searchStarted;
        public string StudentsKeyword
        {
            get => _StudentsKeyword;
            set
            {
                if (value == _StudentsKeyword)
                    return;
                _StudentsKeyword = value;
                OnPropertyChanged(nameof(StudentsKeyword));

                _lastSearch = DateTime.Now;

                if (_searchStarted)
                    return;
                _searchStarted = true;

                Task.Factory.StartNew(async () =>
                {
                    while ((DateTime.Now - _lastSearch).TotalMilliseconds < 777)
                        await TaskEx.Delay(10);
                    awooo.Context.Post(d=> Students.Filter = FilterStudents,null);
                    _searchStarted = false;
                });
            }
        }
        
        private string _Title = "ALL STUDENTS";

        public string Title
        {
            get
            {
                if (FilterCollege && FilterElementary && FilterHighSchool) return " STUDENTS [ ALL ]";
                var title = "";
                if (FilterElementary)
                    title += " [ ELEMENTARY";
                if (FilterHighSchool)
                {
                    if (title.Length > 0)
                    {
                        title += " | ";
                    }
                    else
                    {
                        title = " [ ";
                    }
                    title += "HIGH SCHOOL";
                }
                if (FilterCollege)
                {
                    if (title.Length > 0)
                    {
                        title += " | ";
                    }
                    else
                    {
                        title = " [ ";
                    }
                    title += "COLLEGE";
                }
                if (title.Length > 0)
                {
                    title += " ]";
                }
                else
                {
                    title = " [ ALL ]";
                }
                title = " STUDENTS" + title;
                return title;
            }
            set
            {
                if(value == _Title)
                    return;
                _Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        
        private bool FilterStudents(object o)
        {
            if (!(o is Student s))
                return false;

            if (s.Level > Departments.College) return false;
            
            var fe = FilterElementary || (!FilterElementary && !FilterCollege && !FilterHighSchool);
            var fh = FilterHighSchool || (!FilterElementary && !FilterCollege && !FilterHighSchool);
            var fc = FilterCollege || (!FilterElementary && !FilterCollege && !FilterHighSchool);

            if (!fe && s.Level == Departments.Elementary)
                return false;
            if (!fh && (s.Level == Departments.JuniorHigh || s.Level == Departments.SeniorHigh))
                return false;
            if (!fc && s.Level == Departments.College)
                return false;

            if(string.IsNullOrEmpty(StudentsKeyword))
                return true;
            if (s.Firstname?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
            if (s.Lastname?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
            if (s.YearLevel?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
           // if (s.Department?.ToLower().Contains(StudentsKeyword.ToLower()))
                //return true;
            if (s.Rfid?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
            if (s.StudentId?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
            if (s.ContactNumber?.ToLower().Contains(StudentsKeyword.ToLower())??false)
                return true;
            
            s.Select(false);
            return false;
        }

        private bool _FilterElementary;

        public bool FilterElementary
        {
            get => _FilterElementary;
            set
            {
                if(value == _FilterElementary)
                    return;
                _FilterElementary = value;
                OnPropertyChanged(nameof(FilterElementary));
                Students.Filter = FilterStudents;
                OnPropertyChanged(nameof(Title));
            }
        }

        private bool _FilterHighSchool;

        public bool FilterHighSchool
        {
            get => _FilterHighSchool;
            set
            {
                if(value == _FilterHighSchool)
                    return;
                _FilterHighSchool = value;
                OnPropertyChanged(nameof(FilterHighSchool));
                Students.Filter = FilterStudents;
                OnPropertyChanged(nameof(Title));
            }
        }

        private bool _FilterCollege;

        public bool FilterCollege
        {
            get => _FilterCollege;
            set
            {
                if(value == _FilterCollege)
                    return;
                _FilterCollege = value;
                OnPropertyChanged(nameof(FilterCollege));
                Students.Filter = FilterStudents;
                OnPropertyChanged(nameof(Title));
            }
        }
        
        private bool _StudentActivityOpen;

        public bool StudentActivityOpen
        {
            get => _StudentActivityOpen;
            set
            {
                if (value == _StudentActivityOpen)
                    return;
                _StudentActivityOpen = value;
                OnPropertyChanged(nameof(StudentActivityOpen));
            }
        }

        private ICommand _showActivityCommand;

        public ICommand ShowActivityCommand => _showActivityCommand ?? (_showActivityCommand = new DelegateCommand(d =>
        {
            StudentActivityOpen = true;
        }));

        private bool _ShowViolationSelector;

        public bool ShowViolationSelector
        {
            get => _ShowViolationSelector;
            set
            {
                if(value == _ShowViolationSelector)
                    return;
                _ShowViolationSelector = value;
                OnPropertyChanged(nameof(ShowViolationSelector));
            }
        }

        private ListCollectionView _violationsList;
        public ListCollectionView ViolationsList
        {
            get
            {
                if (_violationsList != null) return _violationsList;
                _violationsList = new ListCollectionView(Violation.Cache);
                _violationsList.Filter = Filter;
                Students.CurrentChanged += (sender, args) =>
                {
                    _violationsList.Filter = Filter;
                };
                return _violationsList;
            }
        }

        private bool Filter(object o)
        {
            if (Students.CurrentItem == null) return false;
            if (!(o is Violation v)) return false;
            return v.Level == (Students.CurrentItem as Student)?.Level;
        }

        private ICommand _acceptAddViolationCommand;

        public ICommand AcceptAddViolationCommand =>
            _acceptAddViolationCommand ?? (_acceptAddViolationCommand = new DelegateCommand(
                d =>
                {
                    if (_violationsList.CurrentItem == null)
                        return;
                    if (!(Students.CurrentItem is Student stud)) return;
                    var v = (Violation) _violationsList.CurrentItem;
                    stud.AddViolation(v);
                    ShowViolationSelector = false;
                    var msg = SFC.Gate.Configurations.Sms.Default.ViolationTemplate.Replace("[STUDENT]", stud.Fullname)
                        .Replace("[VIOLATION]", v.Name)
                        .Replace("[TIME]",DateTime.Now.ToString("g"));

                    new SmsNotification()
                    {
                        UserId = MainViewModel.Instance.CurrentUser.Id,
                        StudentId = stud.Id,
                        Message = msg,
                    }.Save();
                    
                    if (Config.Sms.IncludeUsername)
                        msg += $" Sent By: {MainViewModel.Instance.CurrentUser.Username}";
                    SMS.Send(msg,stud.ContactNumber);
                }));

        private ICommand _clearLogCommand;

        public ICommand ClearLogCommand => _clearLogCommand ?? (_clearLogCommand = new DelegateCommand(d =>
        {
            var stud = Students.CurrentItem as Student;
            stud.ClearLog();
        },d=>MainViewModel.Instance.CurrentUser?.IsAdmin??false));

        private ICommand _addViolationCommand;

        public ICommand AddViolationCommand => _addViolationCommand ?? (_addViolationCommand = new DelegateCommand(d =>
        {
            ViolationsList.MoveCurrentTo(null);
            ShowViolationSelector = true;
        }));

        private ICommand _cancelAddViolationCommand;

        public ICommand CancelAddViolationCommand =>
            _cancelAddViolationCommand ?? (_cancelAddViolationCommand = new DelegateCommand(
                d =>
                {
                    ShowViolationSelector = false;
                }));

        private ICommand _clearViolationsCommand;

        public ICommand ClearViolationsCommand =>
            _clearViolationsCommand ?? (_clearViolationsCommand = new DelegateCommand(
                d =>
                {
                    var stud = Students.CurrentItem as Student;
                    stud?.ClearViolations();
                },d=>MainViewModel.Instance.CurrentUser?.IsAdmin??false));

        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            PrintList(Student.Cache.Where(x=>x.Level<Departments.Faculty));
        }, d => Student.Cache.Count > 0));

        private ICommand _printSelectedCommand;

        public ICommand PrintSelectedCommand => _printSelectedCommand ?? (_printSelectedCommand = new DelegateCommand(
                                                    d =>
                                                    {
                                                        PrintList(Student.Cache.Where(x => x.IsSelected && x.Level<Departments.Faculty));
                                                    }, d => Student.Cache.Any(x => x.IsSelected)));

        private static void PrintList(IEnumerable<Student> students)
        {
            if(!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"List of Students [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\Students.docx"))
            {
                var tbl = doc.Tables.First();

                var pt = (1F / 72F);
                foreach(var item in students)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.Rfid);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Fullname);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    r.Cells[2].Paragraphs.First().Append(item.YearLevel).LineSpacingAfter = 0;

                    p = r.Cells[3].Paragraphs.First().Append(item.Level.EnumDescription());
                    p.Alignment = Alignment.left;
                    p.LineSpacingAfter = 0;
                }
                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0, System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Extensions.Print(temp);
        }

        private ICommand _printViolationsCommand;

        public ICommand PrintViolationsCommand => _printViolationsCommand ?? (_printViolationsCommand = new DelegateCommand(d =>
        {
            var stud = Students.CurrentItem as Student;
            PrintList(stud.Violations.Cast<StudentsViolations>());
            Log.Add($"Printed {stud.Fullname}'s violations.", "");
        }, CanPrint));

        private bool CanPrint(object obj)
        {
            var stud = Students.CurrentItem as Student;
            if (stud == null) return false;
            return stud.Violations.Count > 0;
        }

        private static void PrintList(IEnumerable<StudentsViolations> violations)
        {
            if(!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
            var stud = Instance.Students.CurrentItem as Student;
            if (stud == null) return;
            
            var temp = Path.Combine("Temp", $"{stud.Fullname}'s Violations [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\StudentViolations.docx"))
            {
                doc.ReplaceText("[NAME]",stud.Fullname);
                doc.ReplaceText("[DATE]",DateTime.Now.ToShortDateString());
                
                var tbl = doc.Tables.First();// doc.InsertTable(1, 6);
               
                foreach(var item in violations)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.DateCommitted.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;
                    
                    r.Cells[1].Paragraphs.First().Append(item.Violation.Name).LineSpacingAfter = 0;

                }
                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0, System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Extensions.Print(temp);
        }

        private static void PrintLog(Student stud)
        {
            if(!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"{stud.Fullname}'s Activity Log [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\StudentLog.docx"))
            {
                var tbl = doc.Tables.First();// doc.InsertTable(1, 6);
                
                doc.ReplaceText("[NAME]",stud.Fullname);
                
                foreach(Log item in stud.Logs)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.DateTime.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;
                    
                    r.Cells[1].Paragraphs.First().Append(item.Description).LineSpacingAfter = 0;

                }
                var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0, System.Drawing.Color.Black);
                tbl.SetBorder(TableBorderType.Bottom, border);
                tbl.SetBorder(TableBorderType.Left, border);
                tbl.SetBorder(TableBorderType.Right, border);
                tbl.SetBorder(TableBorderType.Top, border);
                tbl.SetBorder(TableBorderType.InsideV, border);
                tbl.SetBorder(TableBorderType.InsideH, border);
                File.Delete(temp);
                doc.SaveAs(temp);
            }
            Extensions.Print(temp);
        }

        private ICommand _printLogCommand;

        public ICommand PrintLogCommand => _printLogCommand ?? (_printLogCommand = new DelegateCommand(d =>
        {
            PrintLog((Student) Students.CurrentItem);
        },d=>(Students.CurrentItem as Student)?.Logs.Count>0));

        private bool _LogTab = true;

        public bool LogTab
        {
            get => _LogTab;
            set
            {
                if(value == _LogTab)
                    return;
                _LogTab = value;
                OnPropertyChanged(nameof(LogTab));
                if (!SmsTab && !ViolationsTab && !LogTab)
                    LogTab = true;
            }
        }

        private bool _ViolationsTab;

        public bool ViolationsTab
        {
            get => _ViolationsTab;
            set
            {
                if(value == _ViolationsTab)
                    return;
                _ViolationsTab = value;
                OnPropertyChanged(nameof(ViolationsTab));
                if (!SmsTab && !ViolationsTab && !LogTab)
                    ViolationsTab = true;
            }
        }

        private bool _SmsTab;

        public bool SmsTab
        {
            get => _SmsTab;
            set
            {
                if(value == _SmsTab)
                    return;
                _SmsTab = value;
                OnPropertyChanged(nameof(SmsTab));
                if (!value && !ViolationsTab && !LogTab)
                    SmsTab = true;
            }
        }

        private ListCollectionView _messages;

        public ListCollectionView SmsMessages
        {
            get
            {
                if (_messages != null) return _messages;
                _messages = new ListCollectionView(SmsNotification.Cache);
                _messages.Filter = FilterMessage;
                Students.CurrentChanged += (sender, args) =>
                {
                    _messages.Filter = FilterMessage;
                    NotificationMessage = "";
                };
                return _messages;
            }
        }

        private bool FilterMessage(object o)
        {
            if (!(o is SmsNotification msg)) return false;
            if (!(Students.CurrentItem is Student s)) return false;
            return s.Id == msg.StudentId;
        }

        private string _NotificationMessage;

        public string NotificationMessage
        {
            get => _NotificationMessage;
            set
            {
                if(value == _NotificationMessage)
                    return;
                _NotificationMessage = value;
                OnPropertyChanged(nameof(NotificationMessage));
            }
        }

        private ICommand _sendNotificationCommand;
        private DateTime _lastSent = DateTime.MinValue;
        
        private bool CanSend()
        {
            if (!(MainViewModel.Instance.CurrentUser?.IsAdmin??false) && !Config.Sms.AllowNonAdmin) return false;
            if ((DateTime.Now - _lastSent).TotalSeconds < 7) return false;
            return Students.CurrentItem != null && !string.IsNullOrWhiteSpace(NotificationMessage);
        }
        
        public ICommand SendNotificationCommand =>
            _sendNotificationCommand ?? (_sendNotificationCommand = new DelegateCommand(
                d =>
                {
                    if (!CanSend()) return;
                    
                    if (!(Students.CurrentItem is Student s)) return;
                    _lastSent = DateTime.Now;
                    var msg = new SmsNotification()
                    {
                        UserId = MainViewModel.Instance.CurrentUser.Id,
                        StudentId =s.Id,
                        Message = NotificationMessage
                    };
                    msg.Save();

                    if (Config.Sms.IncludeUsername)
                        NotificationMessage += $" Sent By: {MainViewModel.Instance.CurrentUser.Username}";
                    
                    SMS.Send(NotificationMessage,s.ContactNumber);
                    
                    NotificationMessage = "";
                },d=>CanSend()));


        private bool? _SelectionState = false;

        public bool? SelectionState
        {
            get => _SelectionState;
            set
            {
                if(value == _SelectionState)
                    return;
                _SelectionState = value;
                OnPropertyChanged(nameof(SelectionState));
                
                var students = Student.Cache.Where(FilterStudents);
                foreach (var student in students)
                {
                    student.Select(_SelectionState??false);
                }
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private bool _ShowSmsDialog;

        public bool ShowSmsDialog
        {
            get => _ShowSmsDialog;
            set
            {
                if(value == _ShowSmsDialog)
                    return;
                _ShowSmsDialog = value;
                OnPropertyChanged(nameof(ShowSmsDialog));
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _sendBulkCommand;

        public ICommand SendBulkCommand => _sendBulkCommand ?? (_sendBulkCommand = new DelegateCommand(d =>
        {
            _aborting = false;
            ShowSmsDialog = true;
            BulkMessage = "";
            if (SelectionState ?? true)
                BulkSendTo = 4;
            else
                BulkSendTo = 0;
        },d=>(MainViewModel.Instance.CurrentUser?.IsAdmin??false) || Config.Sms.AllowNonAdmin));

        private int _BulkSendTo;

        public int BulkSendTo
        {
            get => _BulkSendTo;
            set
            {
                _BulkSendTo = value;
                OnPropertyChanged(nameof(BulkSendTo));
                if (value == -1)
                    BulkSendTo = 0;
            }
        }

        private bool _HasSelected;

        public bool HasSelected
        {
            get
            {
                var students = Student.Cache.Where(FilterStudents);
                return students.Any(x => x.IsSelected);
            }
            set
            {
                _HasSelected = value;
                OnPropertyChanged(nameof(HasSelected));
            }
        }
        
        private string _BulkMessage;

        public string BulkMessage
        {
            get => _BulkMessage;
            set
            {
                if(value == _BulkMessage)
                    return;
                _BulkMessage = value;
                OnPropertyChanged(nameof(BulkMessage));
            }
        }

        private ICommand _cancelSendBulkCommand;

        public ICommand CancelBulkSendCommand =>
            _cancelSendBulkCommand ?? (_cancelSendBulkCommand = new DelegateCommand(
                d =>
                {
                    ShowSmsDialog = false;
                    BulkMessage = null;
                }));

        private ICommand _acceptSendBulkCommand;
        private Task _bulkSenderTask;
        private CancellationTokenSource _bulkToken;
        private Queue<Student> _sendList;
        private int _sentCount = 0;
        public ICommand AcceptBulkSendCommand =>
            _acceptSendBulkCommand ?? (_acceptSendBulkCommand = new DelegateCommand(
                d =>
                {
                    _sentCount = 0;
                    _sentCount = 0;
                    _aborting = false;
                    ShowSmsDialog = false;
                    IsBulkSending = true;
                    SendingProgressIndeterminate = true;
                    SendingProgress = 0;
                    _bulkToken = new CancellationTokenSource();

                    var students = GetSendList();

                    if (students?.Count > 0)
                    {
                        SendingProgressMaximum = students.Count;

                        _sendList = new Queue<Student>(students);

                        SendNext(_sendList.Dequeue());
                    }
                    else
                    {
                        IsBulkSending = false;
                    }
                        
                       
                },d=>!string.IsNullOrWhiteSpace(BulkMessage) && GetSendList()?.Count>0));

        private List<Student> GetSendList()
        {
            List<Student> students = null;
            switch (BulkSendTo)
            {
                case 0:
                    students = Student.Cache.Where(x => x.Level < Departments.Faculty).ToList();
                    break;
                case 1:
                    students = Student.Cache.Where(x => x.Level == Departments.Elementary).ToList();
                    break;
                case 2:
                    students = Student.Cache.Where(x => (x.Level == Departments.JuniorHigh || x.Level == Departments.SeniorHigh)).ToList();
                    break;
                case 3:
                    students = Student.Cache.Where(x => x.Level == Departments.College).ToList();
                    break;
                case 4:
                    students = Student.Cache.Where(x => x.Level < Departments.Faculty && x.IsSelected).ToList();
                    break;
            }
            return students;
        }

        private async void SentCallback(int code)
        {
            if (_aborting) return;
            if (code != 777) return;
            _sentCount++;
            if (_sendList.Count == 0)
            {
                ShowDone();
                return;
            }
            var student = _sendList.Dequeue();
            if (student == null)
            {
                ShowDone();
                return;
            }
            
            if (_bulkToken.IsCancellationRequested)
            {
                SendingProgressText = "Aborted";
                await TaskEx.Delay(1000);
                IsBulkSending = false;
                return;
            }
            
            SendNext(student);
        }

        private async void ShowDone()
        {
            _aborting = true;
            SendingProgressText = "Done";
            await TaskEx.Delay(1000);

            IsBulkSending = false;

            BulkMessage = "";
        }

        private void SendNext(Student student)
        {
            SendingProgressText = $"Sending {_sentCount + 1}/{SendingProgressMaximum} ...";

            new SmsNotification()
            {
                UserId = MainViewModel.Instance.CurrentUser.Id,
                StudentId = student.Id,
                Message = BulkMessage
            }.Save();

            var msg = BulkMessage;
            if (Config.Sms.IncludeUsername)
                msg = $"{BulkMessage}\nSent By: {MainViewModel.Instance.CurrentUser.Username}";

            SMS.Send(msg, student.ContactNumber, SentCallback, 777);
            
        }

        private bool _IsBulkSending;

        public bool IsBulkSending
        {
            get => _IsBulkSending;
            set
            {
                if(value == _IsBulkSending)
                    return;
                _IsBulkSending = value;
                OnPropertyChanged(nameof(IsBulkSending));
            }
        }

        private string _SendingProgressText = "Sending 0/7 ...";

        public string SendingProgressText
        {
            get => _SendingProgressText;
            set
            {
                if(value == _SendingProgressText)
                    return;
                _SendingProgressText = value;
                OnPropertyChanged(nameof(SendingProgressText));
            }
        }

        private bool _SendingProgressIndeterminate = true;

        public bool SendingProgressIndeterminate
        {
            get => _SendingProgressIndeterminate;
            set
            {
                if(value == _SendingProgressIndeterminate)
                    return;
                _SendingProgressIndeterminate = value;
                OnPropertyChanged(nameof(SendingProgressIndeterminate));
            }
        }

        private double _SendingProgressMaximum;

        public double SendingProgressMaximum
        {
            get => _SendingProgressMaximum;
            set
            {
                if(value == _SendingProgressMaximum)
                    return;
                _SendingProgressMaximum = value;
                OnPropertyChanged(nameof(SendingProgressMaximum));
            }
        }

        private double _SendingProgress;

        public double SendingProgress
        {
            get => _SendingProgress;
            set
            {
                if(value == _SendingProgress)
                    return;
                _SendingProgress = value;
                OnPropertyChanged(nameof(SendingProgress));
            }
        }

        private ICommand _abortSendingCommand;
        private bool _aborting = false;
        public ICommand AbortSendingCommand => _abortSendingCommand ?? (_abortSendingCommand = new DelegateCommand(
        async d =>
        {
            _aborting = true;
            _bulkToken?.Cancel();
            SendingProgressText = "Aborting...";
            await TaskEx.Delay(2000);
            IsBulkSending = false;
        },d=>!_aborting));

        public void ShowStudent(Student student)
        {
            if (!MainViewModel.Instance.HasLoggedIn) return;
            
            awooo.Context.Post(d =>
            {
                if(!Students.MoveCurrentTo(student))
                {
                    StudentsKeyword = "";
                    if(!Students.MoveCurrentTo(student))
                    switch(student.Level)
                    {
                        case Departments.Elementary:
                            FilterElementary = true;
                            break;
                        case Departments.JuniorHigh:
                        case Departments.SeniorHigh:
                            FilterHighSchool = true;
                            break;
                        case Departments.College:
                            FilterCollege = true;
                            break;
                    }
                }

                Students.MoveCurrentTo(student);

                MainViewModel.Instance.Screen = MainViewModel.STUDENTS;
            }, null);
        }

        private bool _IsPrintingViolators;

        public bool IsPrintingViolators
        {
            get => _IsPrintingViolators;
            set
            {
                if(value == _IsPrintingViolators)
                    return;
                _IsPrintingViolators = value;
                OnPropertyChanged(nameof(IsPrintingViolators));
            }
        }

        private ICommand _printViolatorsCommand;

        public ICommand PrintViolatorsCommand =>
            _printViolatorsCommand ?? (_printViolatorsCommand = new DelegateCommand(
                d =>
                {
                    IsPrintingViolators = true;
                }));

        private ICommand _cancelPrintingViolatorsCommand;

        public ICommand CancelPrintingViolatorsCommand =>
            _cancelPrintingViolatorsCommand ?? (_cancelPrintingViolatorsCommand = new DelegateCommand(
                d =>
                {
                    IsPrintingViolators = false;
                }));

        private DateTime _PrintViolatorsFrom = DateTime.Now.AddDays(-1);

        public DateTime PrintViolatorsFrom
        {
            get => _PrintViolatorsFrom;
            set
            {
                if(value == _PrintViolatorsFrom)
                    return;
                _PrintViolatorsFrom = value;
                OnPropertyChanged(nameof(PrintViolatorsFrom));
            }
        }

        private DateTime _PrintViolatorsTo = DateTime.Now;

        public DateTime PrintViolatorsTo
        {
            get => _PrintViolatorsTo;
            set
            {
                if(value == _PrintViolatorsTo)
                    return;
                _PrintViolatorsTo = value;
                OnPropertyChanged(nameof(PrintViolatorsTo));
            }
        }

        public DateTime MinDate => Models.StudentsViolations
                                       .Cache
                                       .OrderByDescending(x=>x.DateCommitted).FirstOrDefault()?.DateCommitted
                                   ?? DateTime.Parse("2/14/2018");

        private ICommand _acceptPrintingViolatorsCommand;


        public ICommand AcceptPrintingViolatorsCommand =>
            _acceptPrintingViolatorsCommand ?? (_acceptPrintingViolatorsCommand = new DelegateCommand(
                d =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        IsPrintingViolators = false;
                        var violations = Models.StudentsViolations.Cache
                            .Where(x => x.DateCommitted.Date >= PrintViolatorsFrom &&
                                        x.DateCommitted.Date <= PrintViolatorsTo)
                            .OrderBy(x => x.Violation.Name);

                        if (!Directory.Exists("Temp"))
                            Directory.CreateDirectory("Temp");
                        var groups = violations.Select(x => x.ViolationId).Distinct();

                        foreach (var vid in groups)
                        {
                            var ids = new List<long>();
                            var temp = Path.Combine("Temp",
                                $"Student Violations By Violation [{DateTime.Now.Ticks}].docx");
                            using (var doc = DocX.Load(@"Templates\ViolationsByViolation.docx"))
                            {

                                var date = "";
                                if (PrintViolatorsFrom.Date == PrintViolatorsTo.Date)
                                {
                                    date = PrintViolatorsFrom.Date.ToString("MMMM d, yyyy");
                                }
                                else
                                {
                                    date = $"{PrintViolatorsFrom:MMM d, yyyy} - {PrintViolatorsTo:MMM d, yyyy}";
                                }
                                doc.ReplaceText("[INCLUSIVE_DATE]", date);
                                doc.ReplaceText("[VIOLATION]",
                                    Models.Violation.Cache.FirstOrDefault(x => x.Id == vid)?.Name);
                                var list = doc.AddList();
                                foreach (var v in violations.Where(x => x.ViolationId == vid))
                                {
                                    if (ids.Contains(v.StudentId))
                                        continue;
                                    ids.Add(v.StudentId);

                                    var violateCount = violations.Count(x =>
                                        x.StudentId == v.StudentId &&
                                        x.ViolationId == v.ViolationId);
                                    var sCount = violateCount > 1 ? $"({violateCount} times)" : "";

                                    doc.AddListItem(list, $"{v.Student.Fullname} {sCount}");

                                }
                                doc.InsertList(list);

                                File.Delete(temp);
                                doc.SaveAs(temp);
                            }
                            Extensions.Print(temp);
                        }

                    });

                }));
        
        private ICommand _acceptPrintViolatorsByDepartmentCommand;
       
       
        public ICommand AcceptPrintViolatorsByDepartmentCommand =>
            _acceptPrintViolatorsByDepartmentCommand ?? (_acceptPrintViolatorsByDepartmentCommand = new DelegateCommand(
                d =>
                {
                    Task.Factory.StartNew(() =>
                    {

                    
                    IsPrintingViolators = false;
                    var violations = Models.StudentsViolations.Cache
                        .Where(x => x.DateCommitted.Date >= PrintViolatorsFrom &&
                                    x.DateCommitted.Date <= PrintViolatorsTo)
                        .OrderBy(x=>x.Violation.Name);
                    
                    if (!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");
                    var groups = violations.Select(x=>x.Violation.Level).Distinct();
                    
                    foreach (var department in groups)
                    {
                        var ids = new List<long>();
                        var temp = Path.Combine("Temp", $"Student Violations By Department [{DateTime.Now.Ticks}].docx");
                        using (var doc = DocX.Load(@"Templates\ViolationsByDepartment.docx"))
                        {
                            
                            var date = "";
                            if (PrintViolatorsFrom.Date == PrintViolatorsTo.Date)
                            {
                                date = PrintViolatorsFrom.Date.ToString("MMMM d, yyyy");
                            }
                            else
                            {
                                date = $"{PrintViolatorsFrom:MMM d, yyyy} - {PrintViolatorsTo:MMM d, yyyy}";
                            }
                            doc.ReplaceText("[INCLUSIVE_DATE]", date);
                            doc.ReplaceText("[DEPARTMENT]", department.ToString().ToUpper() + " DEPARTMENT");

                            var tbl = doc.Tables.First();
                                
                            foreach (var v in violations.Where(x => x.Student.Level == department))
                            {
                                if (ids.Contains(v.StudentId))
                                    continue;
                                ids.Add(v.StudentId);
                                
                                var r = tbl.InsertRow();
                                var p = r.Cells[0].Paragraphs.First().Append(v.Student.Fullname);
                                p.Alignment = Alignment.left;

                                var studentViolations = violations.Where(x => x.StudentId == v.StudentId);
                                var sV = "";
                                var violationIds = new List<long>();
                                foreach (var sv in studentViolations)
                                {
                                    if(violationIds.Contains(sv.ViolationId)) continue;
                                    violationIds.Add(sv.ViolationId);
                                    
                                    var violateCount = violations.Count(x =>
                                        x.StudentId == v.StudentId &&
                                        x.ViolationId == sv.ViolationId);
                                    var sCount = violateCount > 1 ? $" ({violateCount})" : "";

                                    sV += sv.Violation.Name + sCount + ", ";
                                }
                                
                                p = r.Cells[1].Paragraphs.First().Append(sV);
                                p.Alignment = Alignment.left;
                            }
                            
                            var border = new Xceed.Words.NET.Border(BorderStyle.Tcbs_single, BorderSize.one, 0,
                                System.Drawing.Color.Black);
                            tbl.SetBorder(TableBorderType.Bottom, border);
                            tbl.SetBorder(TableBorderType.Left, border);
                            tbl.SetBorder(TableBorderType.Right, border);
                            tbl.SetBorder(TableBorderType.Top, border);
                            tbl.SetBorder(TableBorderType.InsideV, border);
                            tbl.SetBorder(TableBorderType.InsideH, border);

                            
                            
                            File.Delete(temp);
                            doc.SaveAs(temp);
                        }
                        Extensions.Print(temp);
                    }


                    });

                }));

        private ICommand _deleteSelectedCommand;

        public ICommand DeleteSelectedCommand =>
            _deleteSelectedCommand ?? (_deleteSelectedCommand = new DelegateCommand(
                d =>
                {
                    var list = Student.Cache.Where(x=>x.IsSelected && x.Level < Departments.Faculty).ToList();
                    foreach (var student in list)
                    {
                        student.Delete(false);
                    }
                    var s = list.Count>1 ? "students were":"student was";
                    MainViewModel.ShowMessage($"{list.Count} {s} deleted","UNDO", () =>
                    {
                        list.ForEach(x=>x.Undelete());
                    });
                },d=>HasSelected));
    }
}
