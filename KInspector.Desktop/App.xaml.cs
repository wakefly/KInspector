using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Kentico.KInspector.WebApplication;

namespace Kentico.KInspector.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IDisposable WebApplicationServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            WebApplicationServer = Microsoft.Owin.Hosting.WebApp.Start<Startup>("http://localhost:9000");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            WebApplicationServer?.Dispose();
            base.OnExit(e);
        }
    }
}
