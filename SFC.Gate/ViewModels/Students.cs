using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Gate.Models;
using Xceed.Words.NET;

namespace SFC.Gate.Material.ViewModels
{
    class StudentsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private StudentsViewModel()
        {
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
                _students = (ListCollectionView) CollectionViewSource.GetDefaultView(Models.Student.Cache);
                Models.Student.Cache.CollectionChanged += (sender, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        var items = args.OldItems.Cast<Student>();
                        foreach (var item in items)
                        {
                            Log.Add("REVERT", $"{item.Fullname} is deleted.", "Students", item.Id);
                            item.Delete();
                        }
                        MainViewModel.ShowMessage($"{args.OldItems.Count} items were deleted.", "UNDO", () =>
                        {
                            foreach (var student in items)
                            {
                                Log.Add("REVERT", $"{student.Fullname} is undeleted.", "Students", student.Id);
                                student.Undelete();
                            }
                        });
                    }
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
                }, s => s != null));

        private string _StudentsKeyword;

        public string StudentsKeyword
        {
            get => _StudentsKeyword;
            set
            {
                if (value == _StudentsKeyword)
                    return;
                _StudentsKeyword = value;
                OnPropertyChanged(nameof(StudentsKeyword));
                Students.Filter = FilterStudents;
            }
        }

        private bool FilterStudents(object o)
        {
            if (string.IsNullOrEmpty(StudentsKeyword))
                return true;
            if (!(o is Student s))
                return false;
            if (s.Firstname.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.Lastname.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.YearLevel.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.Department.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.Rfid.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.StudentId.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            if (s.ContactNumber.ToLower().Contains(StudentsKeyword.ToLower()))
                return true;
            return false;
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
                return _violationsList;
            }
        }

        private ICommand _acceptAddViolationCommand;

        public ICommand AcceptAddViolationCommand =>
            _acceptAddViolationCommand ?? (_acceptAddViolationCommand = new DelegateCommand(
                d =>
                {
                    if (_violationsList.CurrentItem == null)
                        return;
                    var stud = Students.CurrentItem as Student;
                    var v = (Violation) _violationsList.CurrentItem;
                    stud?.AddViolation(v);
                    ShowViolationSelector = false;
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
            PrintList(Student.Cache);
        }, d => Student.Cache.Count > 0));

        private ICommand _printSelectedCommand;

        public ICommand PrintSelectedCommand => _printSelectedCommand ?? (_printSelectedCommand = new DelegateCommand(
                                                    d =>
                                                    {
                                                        PrintList(Student.Cache.Where(x => x.IsSelected));
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

                    p = r.Cells[3].Paragraphs.First().Append(item.Department);
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

    }
}
