using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFC.Gate.Models;

namespace SFC.Gate.ViewModels
{
    class Visitors : ViewModelBase
    {
        private static Visitors _instance;
        public static Visitors Instance => _instance ?? (_instance = new Visitors());
        
        private Visitor _newVisitor;

        public Visitor NewVisitor
        {
            get => _newVisitor ?? (_newVisitor = new Visitor());
            set
            {
                _newVisitor = value; 
                OnPropertyChanged(nameof(NewVisitor));
            }
        }


    }
}
