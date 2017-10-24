using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

namespace SFC.Gate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            
            awooo.IsRunning = true;
            Config.Sms.Enabled = true;
            base.OnStartup(e);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            
            Config.Save();
            Log.Add("Application Shutdown");
            base.OnExit(e);
        }
    }
}
