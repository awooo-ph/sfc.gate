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
    class Logs : ViewModelBase
    {
        private Logs()
        {
            Context = SynchronizationContext.Current;
        }

        private static Logs _instance;
        public static Logs Instance => _instance ?? (_instance = new Logs());
        
        private ICommand _printCommand;

        public ICommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(d =>
        {
            //Todo: Print Logs
            
        }));

        private ICommand _clearCommand;

        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new DelegateCommand(d =>
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to clear the logs?",
                    "Confirm Action", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

                Log.DeleteAll();
                Log.Add("Logs Cleared");
            Messenger.Default.Broadcast(Messages.LogsCleared);
        }));
    }
}
