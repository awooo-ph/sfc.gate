using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using SFC.Gate.Models;

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
            //Todo: Print Violations
            
            Log.Add("Violations Printed", "");
        }, CanPrint));

        private bool CanPrint(object obj)
        {
            return StudentsViolations.Cache.Count > 0;
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
