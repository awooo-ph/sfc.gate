using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using SFC.Gate.Configurations;
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
                    var msg = Config.Sms.ViolationTemplate
                        .Replace("[STUDENT]", violation.Student.Fullname)
                        .Replace("[VIOLATION]", violation.Violation.Name);
                    SMS.Send(msg, violation.Student.ContactNumber);
                    //violation.IsNotificationSent = true;
                    violation.Update(nameof(violation.IsNotificationSent),true);
                    Log.Add("SMS SENT", $"An SMS notification of {violation.Student.Fullname}'s violation has been sent to his/her parents.");
                }));

        private string _ContactNumber;

        public string ContactNumber
        {
            get => _ContactNumber;
            set
            {
                if(value == _ContactNumber)
                    return;
                _ContactNumber = value;
                OnPropertyChanged(nameof(ContactNumber));
            }
        }

        private string _Message;

        public string Message
        {
            get => _Message;
            set
            {
                if(value == _Message)
                    return;
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private ICommand _sendCommand;

        public ICommand SendCommand => _sendCommand ?? (_sendCommand = new DelegateCommand(d =>
        {
            SMS.Send(Message,ContactNumber);
            Message = "";
        }, d=>!string.IsNullOrEmpty(Message) && !string.IsNullOrEmpty(ContactNumber)));

        private ICommand _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new DelegateCommand(d =>
        {
            ContactNumber = "";
            Message = "";
        }));
    }
}
