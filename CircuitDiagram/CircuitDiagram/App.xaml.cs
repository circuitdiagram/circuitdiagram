using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using CircuitDiagram.Dependency;
using CircuitDiagram.Logging;
using CircuitDiagram.Updates;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            LogManager.LoggerFactory.AddNLog();
            var log = LogManager.GetLogger<App>();
            log.LogInformation($"Application starting up: Circuit Diagram {UpdateVersionService.GetAppDisplayVersion()}");
            
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            LogManager.GetLogger<App>().LogInformation("Application shutting down.");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                  typeof(FrameworkElement),
                  new FrameworkPropertyMetadata(
                      XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetLogger<App>().LogError("Unhandled exception.", e.Exception);
        }
    }
}
