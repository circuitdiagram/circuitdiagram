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
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\ErrorLog.txt", e.Exception.ToString());
        }
    }
}
