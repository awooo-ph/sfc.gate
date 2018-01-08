using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SFC.Gate.ViewModels
{
    class MessageBox:ViewModelBase
    {
        public enum MessageBoxResults
        {
            Closed,
            Button1,
            Button2,
            Button3
        }
        
        public string Title { get; set; }
        public string Header { get; set; }
        public string Message { get; set; }
        public string Button1 { get; set; }

        public string Button2
        {
            get;
            set;
        }

        public string Button3 { get; set; }

        public Visibility Button2Visibility =>
            string.IsNullOrEmpty(Button2) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Button3Visibility => string.IsNullOrEmpty(Button3) ? Visibility.Hidden : Visibility.Visible;
        public long Result { get; set; }

        public static MessageBoxResults Show(string title, string header, string message,
             string button1, string button2="", string button3="")
        {
            MainViewModel.Instance.IsDialogOpen = true;
            var vm = new MessageBox()
            {
                Title = title,
                Header = header,
                Message = message,
                Button2 = button2,
                Button1 = button1,
                Button3 = button3
            };
            
                var msg = new Views.MessageBox { DataContext = vm };
            msg.Owner = Application.Current.MainWindow;
                msg.ShowDialog();
                if (!msg.DialogResult ?? false) vm.Result = 0;

            MainViewModel.Instance.IsDialogOpen = false;
            return (MessageBoxResults) vm.Result;
        }
        
    }
}
