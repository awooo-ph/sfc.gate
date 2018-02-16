using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using SFC.Gate.Configurations;
using SFC.Gate.Models;
using SFC.Gate.ViewModels;

namespace SFC.Gate.Material
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            awooo.IsRunning = true;
            awooo.Context = SynchronizationContext.Current;
            base.OnStartup(e);
            if(Config.Sms.Enabled)
                SMS.Start();
            Config.Sms.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Config.Sms.Enabled))
                {
                    if (Config.Sms.Enabled)
                        SMS.Start();
                    else
                        SMS.Stop();
                }
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Violation.SaveAll();
            Config.Save();
            SMS.Stop();
            Log.Add("Application Shutdown");
            base.OnExit(e);
        }
    }
}
