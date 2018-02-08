using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace SFC.Gate.Material.ViewModels
{
    class MessageDialog 
    {
        public MessageDialog(string title, string message, PackIconKind icon, string acceptText) 
            : this(title, message, icon, acceptText, false, null)
        {}

        private static bool _shown;
        public async void Show()
        {
           // if (_shown) return;
            //_shown = true;
            try
            {
                await Application.Current.MainWindow.ShowDialog(this);
            }
            catch (Exception e)
            {
                //
            }
            
            //_shown = false;
        }

        public static void Show(string title, string message, PackIconKind icon, string acceptText)
        {
            Show(title, message, icon, acceptText, false, null);
        }

        public static void Show(string title, string message, PackIconKind icon, string acceptText, bool isCancellable,
            string cancelText)
        {
            new MessageDialog(title,message,icon,acceptText,isCancellable,cancelText).Show();
        }

        public MessageDialog(string title, string message, PackIconKind icon, string acceptText, bool isCancellable, string cancelText)
        {
            Title = title;
            Message = message;
            Icon = icon;
            AcceptText = acceptText;
        
            IsCancellable = isCancellable;
            CancelText = cancelText;
        
        }

        public PackIconKind Icon { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        
        public string CancelText { get; set; }
        public bool IsCancellable { get; set; }
        
        public string AcceptText { get; set; }
        
        
    }
}
