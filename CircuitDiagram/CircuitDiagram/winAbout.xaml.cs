using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Deployment.Application;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winAbout.xaml
    /// </summary>
    public partial class winAbout : Window
    {
        public winAbout()
        {
            InitializeComponent();
            lblVersionNumber.Content = AppVersion;
        }

        public string AppVersion
        {
            get
            {
                System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();

                string theVersion = string.Empty;

                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    theVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() + " beta";
                else
                {
                    if (_assemblyInfo != null)
                        theVersion = _assemblyInfo.GetName().Version.ToString();
                }
                return theVersion + " beta";
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
