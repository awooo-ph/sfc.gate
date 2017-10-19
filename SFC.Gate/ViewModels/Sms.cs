using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using SFC.Gate.Models;
namespace SFC.Gate.ViewModels
{
    class Sms:ViewModelBase
    {
        private static Sms _instance;
        public static Sms Instance => _instance ?? (_instance = new Sms());

        private Sms()
        {
            Context = SynchronizationContext.Current;
        }
        
        private ICommand _sendNotificationCommand;

        public ICommand SendNotificationCommand =>
            _sendNotificationCommand ?? (_sendNotificationCommand = new DelegateCommand<StudentsViolations>(
                violation =>
                {
                    violation.IsNotificationSent = true;
                }));
    }
}
