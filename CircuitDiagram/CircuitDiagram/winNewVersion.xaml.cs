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

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winNewVersion.xaml
    /// </summary>
    public partial class winNewVersion : Window
    {
        public winNewVersion(NewVersionWindowType type)
        {
            InitializeComponent();

            if (type == NewVersionWindowType.NewVersionAvailable)
            {
                lblUsingCurentVersion.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (type == NewVersionWindowType.NoNewVersionAvailable)
            {
                stpNewVersionAvailable.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                // Error
                lblUsingCurentVersion.Content = "Unable to check for updates.";
                lblNewVersionAvailable.Content = "You can check for updates manually by visiting:";
            }
        }

        public string Version
        {
            get { return (string)lblNewVersionNumber.Content; }
            set { lblNewVersionNumber.Content = value; }
        }

        public string DownloadLink
        {
            get { return hypDownloadLink.NavigateUri.ToString(); }
            set
            {
                if (value == null)
                    lblDownloadLabel.Visibility = System.Windows.Visibility.Collapsed;
                else
                    lblDownloadLabel.Visibility = System.Windows.Visibility.Visible;
                if (value != null)
                {
                    hypDownloadLink.NavigateUri = new Uri(value);
                    tblDownloadLinkPreview.Text = value;
                }
            }
        }

        public static void Show(Window owner, NewVersionWindowType newVersionType, string version, string downloadLink = null)
        {
            winNewVersion theWindow = new winNewVersion(newVersionType);
            theWindow.Owner = owner;
            theWindow.Version = version;
            theWindow.DownloadLink = downloadLink;
            theWindow.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public enum NewVersionWindowType
    {
        NewVersionAvailable,
        NoNewVersionAvailable,
        Error
    }
}
