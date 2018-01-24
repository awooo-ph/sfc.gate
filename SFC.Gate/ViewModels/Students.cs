using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using SFC.Gate.Models;

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
        }));

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
                }));
    }
}
