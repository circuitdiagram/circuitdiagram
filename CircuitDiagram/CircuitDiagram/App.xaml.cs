using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static String[] AppArgs;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                AppArgs = e.Args;
            else
                AppArgs = new string[0];
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if PORTABLE
            string errorLogDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#else
            string errorLogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram";
#endif
            if (System.IO.Directory.Exists(errorLogDirectory))
                System.IO.File.WriteAllText(errorLogDirectory + "\\ErrorLog.txt", e.Exception.ToString());
        }
    }
}
