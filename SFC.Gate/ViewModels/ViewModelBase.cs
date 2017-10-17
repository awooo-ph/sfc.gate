using System.ComponentModel;
using System.Diagnostics;

namespace SFC.Gate.ViewModels
{
    abstract class ViewModelBase:INotifyPropertyChanged
    {
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
