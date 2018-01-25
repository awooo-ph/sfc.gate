using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using SFC.Gate.Configurations;
using SFC.Gate.Models;

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
        }

        protected override void OnExit(ExitEventArgs e)
        {
           // Violation.SaveAll();
            
            base.OnExit(e);
            Config.Save();
            Log.Add("Application Shutdown");
        }
    }
}
