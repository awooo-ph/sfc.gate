using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private MainViewModel() { }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private ListCollectionView _students;
        public ListCollectionView Students
        {
            get
            {
                if (_students != null) return _students;
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
                        ShowMessage($"{args.OldItems.Count} items were deleted.","UNDO", () =>
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private User _CurrentUser;

        public User CurrentUser
        {
            get => _CurrentUser;
            set
            {
                if(value == _CurrentUser)
                    return;
                _CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
        
        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand<Student>(
            stud =>
            {
                var file = Extensions.GetPicture();
                if (file == null) return;
                var oldPic = stud.Picture;
                stud.Update(nameof(Student.Picture), Extensions.ResizeImage(file));
                Log.Add("REVERT", $"{stud.Fullname}'s picture was changed.", "Students", stud.Id);
                ShowMessage("Picture Changed","UNDO", () =>
                {
                    stud.Update(nameof(Student.Picture),oldPic);
                    Log.Add("REVERT", $"{stud.Fullname}'s picture changed was undone.", "Students", stud.Id);
                });
            }, s=> s!=null));

        private string _StudentsKeyword;

        public string StudentsKeyword
        {
            get => _StudentsKeyword;
            set
            {
                if(value == _StudentsKeyword)
                    return;
                _StudentsKeyword = value;
                OnPropertyChanged(nameof(StudentsKeyword));
                Students.Filter = FilterStudents;
            }
        }

        private SnackbarMessageQueue _messageQueue;
        public SnackbarMessageQueue MessageQueue => _messageQueue ?? (_messageQueue = new SnackbarMessageQueue());

        public static void ShowMessage(string message,string actionContent, Action action, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent, action, promote);
        }

        public static void ShowMessage<T>(string message, string actionContent, Action<T> action, T param, bool promote = false)
        {
            Instance.MessageQueue.Enqueue(message,actionContent,action,param,promote);
        }

        private bool FilterStudents(object o)
        {
            if (string.IsNullOrEmpty(StudentsKeyword)) return true;
            if (!(o is Student s)) return false;
            if (s.Firstname.ToLower().Contains(StudentsKeyword.ToLower())) return true;
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
                if(value == _StudentActivityOpen)
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
    }
}
