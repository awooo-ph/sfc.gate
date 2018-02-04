using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

namespace SFC.Gate.Material.ViewModels
{
    class NewFacultyViewModel : INotifyPropertyChanged
    {
        private Student _Item;

        public Student Item
        {
            get
            {
                if (_Item != null) return _Item;
                _Item = new Student()
                {
                    Level = Departments.Faculty
                };
                _Item.PropertyChanged += (sender, args) =>
                {
                    if(args.PropertyName==nameof(Student.Rfid))
                        OnPropertyChanged(nameof(HasRfid));
                };
                return _Item;
            }
        }
        
        public bool HasRfid => !string.IsNullOrEmpty(Item.Rfid);

        private bool _HasError;

        public bool HasError
        {
            get => _HasError;
            set
            {
                if(value == _HasError)
                    return;
                _HasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }
        
        private ICommand _CancelCommand;

        public ICommand CancelCommand
        {
            get => _CancelCommand;
            set
            {
                if(value == _CancelCommand)
                    return;
                _CancelCommand = value;
                OnPropertyChanged(nameof(CancelCommand));
            }
        }

        private ICommand _AcceptCommand;

        public ICommand AcceptCommand
        {
            get => _AcceptCommand;
            set
            {
                if(value == _AcceptCommand)
                    return;
                _AcceptCommand = value;
                OnPropertyChanged(nameof(AcceptCommand));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
    
}
