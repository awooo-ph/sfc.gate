using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace SFC.Gate.Material.ViewModels
{
    class MessageDialog 
    {
        public MessageDialog(string title, string message, PackIconKind icon, string acceptText) 
            : this(title, message, icon, acceptText, false, null)
        {}

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
