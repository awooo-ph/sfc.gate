using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SFC.Gate.Models;
using Xceed.Words.NET;

namespace SFC.Gate.ViewModels
{
    class Violations:ViewModelBase
    {
        private Violations()
        {
            Context = SynchronizationContext.Current;
        }
        
        private static Violations _instance;
        public static Violations Instance => _instance ?? (_instance = new Violations());

        private Student _selectedStudent;

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value; 
                OnPropertyChanged(nameof(SelectedStudent));
            }
        }

        private string _violation;

        public string Violation
        {
            get => _violation;
            set
            {
                _violation = value; 
                OnPropertyChanged(nameof(Violation));
            }
        }

        private Violation _selectedViolation;

        public Violation SelectedViolation
        {
            get => _selectedViolation;
            set
            {
                _selectedViolation = value; 
                OnPropertyChanged(nameof(SelectedViolation));
                if (_selectedViolation != null)
                    Description = _selectedViolation.Description;
                else
                    Description = "";
            }
        }
        

        private string _description;

        public string Description
        {
            get => _description;
            set
            {
                _description = value; 
                OnPropertyChanged(nameof(Description));
            }
        }
        
        private ICommand _addViolationCommand;

        public ICommand AddViolationCommand => _addViolationCommand ?? (_addViolationCommand = new DelegateCommand(d =>
        {
            var violation = Models.Violation.Cache.FirstOrDefault(v => v.Name.ToLower() == Violation.ToLower());
            if(violation==null) violation = new Violation()
            {
                Name = Violation, Description = Description
            };
            violation.Save();
            
            SelectedStudent.AddViolation(violation);
            
        }, CanAddViolation));

        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            PrintList(StudentsViolations.Cache);
            Log.Add("Violations Printed", "");
        }, CanPrint));

        private bool CanPrint(object obj)
        {
            return StudentsViolations.Cache.Count > 0;
        }

        private static void PrintList(IEnumerable<StudentsViolations> violations)
        {
            if(!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"List of Violations [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\Violations.docx"))
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
                foreach(var item in violations)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.DateCommitted.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Student.Fullname);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    r.Cells[2].Paragraphs.First().Append(item.Violation.Name).LineSpacingAfter = 0;
                    
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

        private ICommand _clearCommand;

        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new DelegateCommand(d =>
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to delete all violations?", "Confirm Action",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
            
            StudentsViolations.DeleteAll();
            Log.Add("Violations Cleared", "Violations record has been cleared.");
        }, CanPrint));

        private bool CanAddViolation(object obj)
        {
            return SelectedStudent != null && !string.IsNullOrEmpty(Violation);
        }
    }
}
