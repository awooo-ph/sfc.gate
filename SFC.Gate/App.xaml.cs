using System.Windows;
using SFC.Gate.Configurations;
using SFC.Gate.Models;
using SFC.Gate.ViewModels;

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
