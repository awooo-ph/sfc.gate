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
    class Logs : ViewModelBase
    {
        private Logs()
        {
            Context = SynchronizationContext.Current;
        }

        private static Logs _instance;
        public static Logs Instance => _instance ?? (_instance = new Logs());
        
        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            PrintList(Log.Cache);
        }));

        private ICommand _clearCommand;

        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new DelegateCommand(d =>
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to clear the logs?",
                    "Confirm Action", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

                Log.DeleteAll();
                Log.Add("Logs Cleared");
            Messenger.Default.Broadcast(Messages.LogsCleared);
        }));

        private static void PrintList(IEnumerable<Log> logs)
        {
            if(!Directory.Exists("Temp"))
                Directory.CreateDirectory("Temp");
            var temp = Path.Combine("Temp", $"Activity Log [{DateTime.Now:d-MMM-yyyy}].docx");
            using(var doc = DocX.Load(@"Templates\Log.docx"))
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
                foreach(var item in logs)
                {
                    var r = tbl.InsertRow();
                    var p = r.Cells[0].Paragraphs.First().Append(item.DateTime.ToString("g"));
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.center;

                    p = r.Cells[1].Paragraphs.First().Append(item.Event);
                    p.LineSpacingAfter = 0;
                    p.Alignment = Alignment.left;

                    r.Cells[2].Paragraphs.First().Append(item.Description).LineSpacingAfter = 0;

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
