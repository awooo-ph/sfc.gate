using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SFC.Gate.Models;
using MsgBox=System.Windows.MessageBox;

namespace SFC.Gate.ViewModels
{
    class Visitors : ViewModelBase
    {
        private Visitors()
        {
            Messenger.Default.AddListener(Messages.VisitorAdded, VisitorAdded);
        }

        private void VisitorAdded()
        {
            MsgBox.Show("New visitor entry successful.", "Action Completed", MessageBoxButton.OK,
                MessageBoxImage.Information);
            NewVisitor = null;
        }

        private static Visitors _instance;
        public static Visitors Instance => _instance ?? (_instance = new Visitors());
        
        private Visitor _newVisitor;

        public Visitor NewVisitor
        {
            get => _newVisitor ?? (_newVisitor = new Visitor());
            private set
            {
                _newVisitor = value; 
                OnPropertyChanged(nameof(NewVisitor));
            }
        }


    }
}
