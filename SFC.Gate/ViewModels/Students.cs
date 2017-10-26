using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FastMember;
using Microsoft.Win32;
using SFC.Gate.Data;
using SFC.Gate.Models;
using MsgBox = System.Windows.MessageBox;

namespace SFC.Gate.ViewModels
{
    class Students:ViewModelBase
    {

        private Students()
        {
            Messenger.Default.AddListener<Student>(Messages.DuplicateRfid, s => NotifyDuplicate(s, "RFID"));
            Messenger.Default.AddListener<Student>(Messages.DuplicateStudentId, s => NotifyDuplicate(s, "ID Number"));
            Messenger.Default.AddListener(Messages.DuplicateName, NotifyDuplicateName);
            Messenger.Default.AddListener<Student>(Messages.NewStudentAdded, NewStudentAdded);
            Messenger.Default.AddListener(Messages.StudentSaved, StudentInfoSaved);
        }
        
        private static void StudentInfoSaved()
        {
            Context.Post(d =>
            {
                MsgBox.Show("Student's info updated!", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }, null); ;
        }

        private Student _selectedStudent;

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != null && _savedValues.ContainsKey(_selectedStudent.Id))
                    _savedValues.Remove(_selectedStudent.Id);
                
                _selectedStudent = value;
                
                if (_selectedStudent != null)
                    SaveValues(_selectedStudent);
                
                OnPropertyChanged(nameof(SelectedStudent));
                OnPropertyChanged(nameof(ResetCommand));
                
            }
        }

        private ICommand _editStudentCommand;

        public ICommand EditStudentCommand => _editStudentCommand ?? (_editStudentCommand = new DelegateCommand(d =>
        {
            Instance.SelectedTab = 1;
            OnPropertyChanged(nameof(ResetCommand));
        }));

        private int _selectedTab;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
                OnPropertyChanged(nameof(ResetCommand));
            }
        }

        private static Students _instance;
        public static Students Instance = _instance ?? (_instance= new Students());
        
        private static void NewStudentAdded(Student student)
        {
            MsgBox.Show($"{student.Fullname} has been successfully added.", "New Student Added", MessageBoxButton.OK,
                MessageBoxImage.Information);
            Instance.NewStudentHolder = null;

        }

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand<Student>(
                                                    student =>
                                                    {
                                                        student.PicturePath = Extensions.GetPicture();
                                                    }));
        
        private static void NotifyDuplicate(Student stud, string property)
        {
            Context.Post(d => {
                var res = MessageBox.Show("Cannot Add New Student", $"{property} is currently in use by {stud.Firstname} {stud.Lastname}.",
                    $"Each student must have a unique {property}.\n" +
                    $"Click CANCEL to cancel adding {Instance.NewStudentHolder.Firstname} {Instance.NewStudentHolder.Lastname}.\n" +
                    $"Click REMOVE to remove {stud.Firstname} {stud.Lastname} from the database.\n" +
                    $"What do you want to do?",
                    $"CANCEL Adding New Student",
                    $"REMOVE Old Student");

                if (res == MessageBox.MessageBoxResults.Button2)
                {
                    stud.Delete();
                    Instance.NewStudentHolder.Save();
                    Instance.NewStudentHolder = null;
                    MsgBox.Show($"{stud.Firstname} {stud.Lastname} has been removed while the new student has been successfully added to the database.",
                        "New Student Added",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Instance.SelectedTab = 1;
                }

            }, null);
        }

        private Dictionary<long, Dictionary<string, object>> _savedValues = new Dictionary<long, Dictionary<string, object>>();
        private void SaveValues(Student student)
        {
            if (student == null) return;
            var obj = TypeAccessor.Create(typeof(Student));
            var model = ObjectAccessor.Create(student);
            if(!_savedValues.ContainsKey(student.Id))
                _savedValues.Add(student.Id, new Dictionary<string, object>());
            var sv = _savedValues[student.Id];

            var props = typeof(Student).GetProperties();
            
            
            foreach (var member in props)
            {
                if (!member.CanWrite || member.Name=="Item" || member.Name=="Id") continue;
                if(member.IsDefined(typeof(IgnoreAttribute), true)) continue;
                
                if(!sv.ContainsKey(member.Name))
                    sv.Add(member.Name, null);
                sv[member.Name] = model[member.Name];
            }
        }

        private bool ValuesChanged(Student student)
        {
            if (student == null) return false;
            var obj = TypeAccessor.Create(typeof(Student));
            var model = ObjectAccessor.Create(student);
            if (!_savedValues.ContainsKey(student.Id))
                return false;
            var sv = _savedValues[student.Id];

            var props = typeof(Student).GetProperties();
            
            foreach (var member in props)
            {
                if(!member.CanWrite || member.Name == "Item" || member.Name == "Id") continue;
                if (member.IsDefined(typeof(IgnoreAttribute), true)) continue;
                if (!sv.ContainsKey(member.Name))
                    sv.Add(member.Name, null);
                var mv = model[member.Name];
                var svv = sv[member.Name];
                if (svv?.ToString() != mv?.ToString())
                    return true;
            }
            return false;
        }
        
        private Student _newStudentHolder;

        public Student NewStudentHolder
        {
            get
            {
                if (_newStudentHolder == null)
                {
                    _newStudentHolder = new Student();
                    if(!_savedValues.ContainsKey(0))
                        SaveValues(_newStudentHolder);
                }
                return _newStudentHolder;
            }
            set
            {
                _newStudentHolder = value;
                OnPropertyChanged(nameof(NewStudentHolder));
            }
        }

        private ICommand _deleteSelectedStudents;

        public ICommand DeleteSelectedStudents =>
            _deleteSelectedStudents ?? (_deleteSelectedStudents = new DelegateCommand(
                d =>
                {
                    if (!Student.Cache.Any(stud => stud.IsSelected)) return;
                    if (MsgBox.Show("Are you sure you want to delete all selected students?", "Confirm Action",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
                    var count = 0;
                    long id = 0;
                    foreach (var student in Student.Cache.Where(stud => stud.IsSelected).ToList())
                    {
                        id = student.Id;
                        student.Delete();
                        count++;
                    }
                    if (count > 1)
                        Log.Add("Students Deleted", "Multiple students has been removed from the database.");
                    else
                        Log.Add("Student Deleted", $"Student removed from database.", "Students", id);

                }, d => Student.Cache.Any(stud => stud.IsSelected)));

        private ICommand _deleteAllStudents;

        public ICommand DeleteAllStudents => _deleteAllStudents ?? (_deleteAllStudents = new DelegateCommand(d =>
        {
            if (MsgBox.Show("Are you sure you want to delete all students?", "Confirm Action",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            Student.DeleteAll();
            Log.Add("Database Cleared", "Students database has been cleared.");
        }));
        
        private static void NotifyDuplicateName()
        {
            Context.Post(d =>
            {
                MsgBox.Show($"{Students.Instance.NewStudentHolder.Fullname} is already in the database.",
                    "Student Already Exists", MessageBoxButton.OK, MessageBoxImage.Hand);
                Instance.SelectedTab = 1;
            }, null);
        }

        private ICommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new DelegateCommand<Student>(stud =>
        {
            if (stud.IsNew)
                NewStudentHolder = new Student();
            else
                SelectedStudent = stud.Reset();
        }, stud =>
        {
            return ValuesChanged(stud);
        }));
        
    }
}
