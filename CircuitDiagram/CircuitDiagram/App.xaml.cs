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
using LogManager = log4net.LogManager;

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

            log4net.Config.XmlConfigurator.Configure();
            CircuitDiagram.Logging.LogManager.Initialize(t => new Log4NetLogger(LogManager.GetLogger(t)));

            var log = LogManager.GetLogger(typeof(App));
            log.Info($"Application starting up: Circuit Diagram {UpdateVersionService.GetAppDisplayVersion()}");
            
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            LogManager.GetLogger(typeof(App)).Info("Application shutting down.");
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
            LogManager.GetLogger(typeof(App)).Error("Unhandled exception.", e.Exception);
        }
    }
}
