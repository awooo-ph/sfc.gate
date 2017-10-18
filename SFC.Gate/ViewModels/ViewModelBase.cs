using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace SFC.Gate.ViewModels
{
    abstract class ViewModelBase:INotifyPropertyChanged
    {
        
        private static SynchronizationContext _context;

        public static SynchronizationContext Context
        {
            get => _context;
            set
            {
                if (_context != null) return;
                _context = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool NotifyChanges = true;
        protected void OnPropertyChanged(string propertyName)
        {
            if (!NotifyChanges) return;
            var pc = PropertyChanged;
            if (pc == null) return;

            VerifyPropertyName(propertyName);

            var e = new PropertyChangedEventArgs(propertyName);
            pc(this, e);
            
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough()]
        public void VerifyPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
                Debug.Fail($"Invalid property name: {propertyName}");
        }
    }
}
