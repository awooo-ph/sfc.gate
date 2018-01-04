using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastMember;
using Microsoft.Win32;
using SFC.Gate.Configurations;
using SFC.Gate.Data;
using SFC.Gate.Models;
using Xceed.Words.NET;
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

        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            PrintList(Student.Cache);
        },d=>Student.Cache.Count>0));

        private ICommand _printSelectedCommand;

        public ICommand PrintSelectedCommand => _printSelectedCommand ?? (_printSelectedCommand = new DelegateCommand(
                                                    d =>
                                                    {
                                                        PrintList(Student.Cache.Where(x=>x.IsSelected));
                                                    }, d=>Student.Cache.Any(x=>x.IsSelected)));
        
        private static void PrintList(IEnumerable<Student> students)
        {
            if (!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"List of Students [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\Students.docx"))
            {
                var tbl = doc.Tables.First();// doc.InsertTable(1, 6);
                var wholeWidth = doc.PageWidth - doc.MarginLeft - doc.MarginRight;

                var pt = (1F / 72F);
                //var widths = new float[] { 150f, 200f, 400f, 100f, 100f, 100f, 300f };
                //tbl.SetWidths(widths);
                //  tbl.AutoFit = AutoFit.Contents;
                //var r = tbl.Rows[0];
                //r.Cells[0].Paragraphs.First().Append("DATE").Bold().Alignment = Alignment.center;
                //r.Cells[1].Paragraphs.First().Append("STUDENT").Bold().Alignment = Alignment.center;
                //r.Cells[2].Paragraphs.First().Append("DUE").Bold().Alignment = Alignment.center;
                //r.Cells[3].Paragraphs.First().Append("RECEIVED").Bold().Alignment = Alignment.center;
                //  r.Cells[4].Paragraphs.First().Append("BALANCE").Bold().Alignment = Alignment.center;
                //    r.Cells[5].Paragraphs.First().Append("CASHIER").Bold().Alignment = Alignment.center;
                //tbl.AutoFit = AutoFit.Contents;
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
            Print(temp);
        }

        private static void Print(string path)
        {
            var info = new ProcessStartInfo(path);
            //info.Arguments = "\"" + Config.PrinterName + "\"";
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.Verb = "PrintTo";
            Process.Start(info);
        }
    }
}
