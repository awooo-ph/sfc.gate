﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using SFC.Gate.Material.Views;
using SFC.Gate.Models;
using Xceed.Words.NET;

namespace SFC.Gate.Material.ViewModels
{
    class Faculty : INotifyPropertyChanged
    {
        private Faculty()
        {
            Messenger.Default.AddListener<Student>(Messages.ModelSelected, s =>
            {
                if (s.Level != Departments.Faculty) return;
                var students = Student.Cache.Where(Filter).ToList();
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
                if (screen == MainViewModel.FACULTY)
                {
                    RfidScanner.ExclusiveCallback = ScanCallback;
                }
                else
                {
                    IsDialogOpen = false;
                    InvalidList.ForEach(x => x.Reset());
                    InvalidList.Clear();
                }
            });
            
            Messenger.Default.AddListener<Student>(Messages.BeginAddStudent, student =>
            {
                if (MainViewModel.Instance.Screen == MainViewModel.FACULTY)
                {
                    student.Level = Departments.Faculty;
                }
            });

            Messenger.Default.AddListener<Student>(Messages.CommitError, student =>
            {
                if (MainViewModel.Instance.Screen != MainViewModel.FACULTY)
                    return;

                if (student.Id == 0 &&
                    string.IsNullOrEmpty(student.Firstname) &&
                    string.IsNullOrEmpty(student.Lastname) &&
                    string.IsNullOrEmpty(student.ContactNumber))
                {
                    Student.Cache.Remove(student);
                }
                else
                {
                    InvalidList.Add(student);
                    //MessageDialog.Show("INVALID DATA", student.GetLastError(), PackIconKind.Alert, "OKAY");
                    MainViewModel.ShowMessage($"INVALID DATA: {student.GetLastError()}",null,null);
                }

            });
        }
        
        private List<Student> InvalidList = new List<Student>();
        
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

        private ICommand _deleteSelectedCommand;

        public ICommand DeleteSelectedCommand =>
            _deleteSelectedCommand ?? (_deleteSelectedCommand = new DelegateCommand(
                d =>
                {
                    var list = Student.Cache.Where(x=>x.IsSelected && x.Level == Departments.Faculty).ToList();
                    
                    var s = list.Count > 1 ? "items were" : "item was";
                    
                    foreach (var student in list)
                    {
                        student.Delete(false);
                    }
                    
                    MainViewModel.ShowMessage($"{list.Count} {s} deleted", "UNDO", () =>
                    {
                        list.ForEach(x => x.Undelete());
                    });
                }, d => HasSelected));

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
                    if(Student.Cache.Any(x => x.Rfid.ToLower() == s.ToLower()))
                    {
                        InvalidRfidMessage = "INVALID! CARD IS IN USE";
                        IsNewRfidInvalid = true;
                    }
                    if(Visit.GetByRfid(s).Count > 0)
                    {
                        InvalidRfidMessage = "ALREADY REGISTERED AS VISITOR'S CARD";
                        IsNewRfidInvalid = true;
                    }

                    if(IsNewRfidInvalid)
                    {
                        if(_invalidScanShown)
                            return;
                        _invalidScanShown = true;
                        _lastScan = DateTime.Now;
                        Task.Factory.StartNew(async () =>
                        {
                            while((DateTime.Now - _lastScan).TotalMilliseconds < 4444)
                                await TaskEx.Delay(100);
                            _invalidScanShown = false;
                            IsNewRfidInvalid = false;
                        });
                    } else
                    {
                        stud.Update(nameof(Student.Rfid), s);
                        ShowRfidDialog = false;
                        ScanCallback = null;
                    }
                };

                RfidScanner.ExclusiveCallback = ScanCallback;

                ShowRfidDialog = true;
            },d=>d!=null && d.Id>0 && (MainViewModel.Instance.CurrentUser?.IsAdmin??false)));
        
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

        private static Faculty _instance;
        public static Faculty Instance => _instance ?? (_instance = new Faculty());

        private bool _IsDialogOpen;

        public bool IsDialogOpen
        {
            get => (DialogContent!=null && _IsDialogOpen) || ShowRfidDialog;
            set
            {
                if(value == _IsDialogOpen)
                    return;
                _IsDialogOpen = value;
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        public TimeSpan TotalHours
        {
            get
            {
                if(!(Items.CurrentItem is Student s)) return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(
                    DailyTimeRecord.Cache
                        .Where(x=>x.EmployeeId == s.Id)
                        .Sum(x => x.TimeSpan.TotalMilliseconds));
            }
        }

        public TimeSpan ThisMonth
        {
            get
            {
                if (!(Items.CurrentItem is Student s))
                    return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(
                    DailyTimeRecord.Cache
                        .Where(x => x.EmployeeId == s.Id 
                                    && x.TimeIn.Date.Month == DateTime.Now.Month
                                    && x.TimeIn.Date.Year == DateTime.Now.Year)
                        .Sum(x => x.TimeSpan.TotalMilliseconds));
            }
        }

        private ICommand _clearDtrCommand;

        public ICommand ClearDtrCommand => _clearDtrCommand ?? (_clearDtrCommand = new DelegateCommand(async d =>
        {
            if (!(Items.CurrentItem is Student s))
                return;
            
            var dlg = new MessageDialog("DELETE ALL TIME RECORDS?",
                $"You are about to delete all time records of {s.Fullname}. This action can not be undone. Are you sure you want to continue?",
                PackIconKind.Close, "YES, CLEAR RECORDS", true, "NO, CANCEL ACTION");

            await Application.Current.MainWindow.ShowDialog(dlg,
                (sender, args) => { }, (sender, args) =>
                {
                    if(args.Parameter as bool? ?? false)
                        DailyTimeRecord.DeleteRecords(s.Id);
                });
            
        }, d=> Items.CurrentItem!=null && (MainViewModel.Instance.CurrentUser?.IsAdmin??false)));

        private ListCollectionView _timeRecord;

        public ListCollectionView TimeRecord
        {
            get
            {
                if (_timeRecord != null) return _timeRecord;
                _timeRecord = new ListCollectionView(DailyTimeRecord.Cache);
                _timeRecord.GroupDescriptions.Add(new PropertyGroupDescription("Date"));
                _timeRecord.CustomSort = new DtrSorter();
                _timeRecord.Filter = FilterTimeRecord;
                Items.CurrentChanged += (sender, args) =>
                {
                    _timeRecord.Filter = FilterTimeRecord;
                    OnPropertyChanged(nameof(ThisMonth));
                    OnPropertyChanged(nameof(TotalHours));
                };
                return _timeRecord;
            }
        }

        private bool? _SelectionState = false;

        public bool? SelectionState
        {
            get => _SelectionState;
            set
            {
                if (value == _SelectionState)
                    return;
                _SelectionState = value;
                OnPropertyChanged(nameof(SelectionState));

                var students = Student.Cache.Where(Filter);
                foreach (var student in students)
                {
                    student.Select(_SelectionState ?? false);
                }
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private bool _HasSelected;

        public bool HasSelected
        {
            get
            {
                var students = Student.Cache.Where(Filter);
                return students.Any(x => x.IsSelected);
            }
            set
            {
                _HasSelected = value;
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private bool FilterTimeRecord(object o)
        {
            if (!(Items.CurrentItem is Student s)) return false;
            if (!(o is DailyTimeRecord t)) return false;
            return s.Id == t.EmployeeId;
        }

        private object _DialogContent;

        public object DialogContent
        {
            get => _DialogContent;
            set
            {
                if(value == _DialogContent)
                    return;
                _DialogContent = value;
                OnPropertyChanged(nameof(DialogContent));
                OnPropertyChanged(nameof(IsDialogOpen));
            }
        }

        private ICommand _addCommand;
        private NewFacultyViewModel NewFaculty;
        public ICommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(d =>
        {
            NewFaculty = new NewFacultyViewModel()
            {
                CancelCommand = new DelegateCommand(a =>
                {
                    NewFaculty = null;
                    DialogContent = null;
                    IsDialogOpen = false;
                    ScanCallback = null;
                    RfidScanner.ExclusiveCallback = null;
                }),
                AcceptCommand = new DelegateCommand(a =>
                {
                    NewFaculty.Item.Save();
                    
                    NewFaculty = null;
                    DialogContent = null;
                    IsDialogOpen = false;
                    ScanCallback = null;
                    RfidScanner.ExclusiveCallback = null;
                },a=>NewFaculty.Item.CanSave() && !NewFaculty.HasError)
            };
            DialogContent = NewFaculty;
            IsDialogOpen = true;

            ScanCallback = async id =>
            {
                if (Student.Cache.Any(x => x.Rfid.ToUpper() == id.ToUpper()) || Visit.GetByRfid(id).Count > 0)
                {
                    NewFaculty.HasError = true;
                    NewFaculty.Item.Rfid = id;
                    await TaskEx.Delay(777);
                    NewFaculty.HasError = false;
                    NewFaculty.Item.Rfid = "";
                }
                else
                {
                    NewFaculty.Item.Rfid = id;
                    NewFaculty.HasError = false;
                }
            };
            RfidScanner.ExclusiveCallback = ScanCallback;
        }));

        private Action<string> ScanCallback;
        
        private ListCollectionView _items;

        public ListCollectionView Items
        {
            get
            {
                if (_items != null) return _items;
                _items = new ListCollectionView(Student.Cache);
                _items.Filter = Filter;
                    
                Student.Cache.CollectionChanged += (sender, args) =>
                {
                    if(!_items.IsAddingNew)
                        _items.Filter = Filter;
                };
                return _items;
            }
        }

        private string _SearchKeyword;
        private DateTime _lastSearch = DateTime.Now;
        private bool _searchStarted;
        public string SearchKeyword
        {
            get => _SearchKeyword;
            set
            {
                if(value == _SearchKeyword)
                    return;
                _SearchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                _lastSearch = DateTime.Now;

                if (_searchStarted)
                    return;
                _searchStarted = true;

                Task.Factory.StartNew(async () =>
                {
                    while ((DateTime.Now - _lastSearch).TotalMilliseconds < 777)
                        await TaskEx.Delay(10);
                    awooo.Context.Post(d => Items.Filter = Filter, null);
                    _searchStarted = false;
                });
            }
        }

        private ICommand _printLogCOmmand;

        public ICommand PrintLogCommand => _printLogCOmmand ?? (_printLogCOmmand = new DelegateCommand(d =>
        {
            PrintLog((Student) Items.CurrentItem);
        }));

        private void PrintLog(Student stud)
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"{stud.Fullname}'s DTR [{DateTime.Now:d-MMM-yyyy}].docx");
            using (var doc = DocX.Load(@"Templates\DTR.docx"))
            {
                var tbl = doc.Tables.First(); // doc.InsertTable(1, 6);

                doc.ReplaceText("{NAME}", stud.Fullname);
                var items = TimeRecord.Cast<DailyTimeRecord>().ToList();
                var total = TimeSpan.FromMilliseconds(items.Sum(x => x.TimeSpan.TotalMilliseconds));
                
                doc.ReplaceText("{TOTAL_HOURS}",$"{total.Hours}:{total.Minutes:00}");
                
                foreach (var item in items)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.TimeIn.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.TimeOut?.ToString("g")??"");
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    if(item.HasLeft)
                    r.Cells[2].Paragraphs.First().Append($"{item.TimeSpan.Hours}:{item.TimeSpan.Minutes:00}").Alignment=Alignment.center;

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

        private bool Filter(object o)
        {
            if (!(o is Student s)) return false;
            if (s.Level < Departments.Faculty) return false;

            if (string.IsNullOrEmpty(SearchKeyword)) return true;
            if (s.Fullname?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.Rfid?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.YearLevel?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;
            if (s.ContactNumber?.ToLower().Contains(SearchKeyword.ToLower()) ?? false) return true;

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            awooo.Context.Post(d =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            },null);
            
        }
    }
}
