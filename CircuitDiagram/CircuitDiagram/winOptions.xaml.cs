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
    /// Interaction logic for winOptions.xaml
    /// </summary>
    public partial class winOptions : Window
    {
        public winOptions()
        {
            InitializeComponent();

            chbUpdatesStartup.IsChecked = Properties.Settings.Default.CheckForUpdatesOnStartup;
            chbShowToolboxScrollBar.IsChecked = Properties.Settings.Default.IsToolboxScrollBarVisible;

            this.Loaded += new RoutedEventHandler(winOptions_Loaded);
        }

        void winOptions_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Owner is MainWindow)
                btnClearRecentFilesMenu.IsEnabled = !((this.Owner as MainWindow).RecentFiles.Count == 1 && (this.Owner as MainWindow).RecentFiles[0] == "(empty)");
            else
                btnClearRecentFilesMenu.IsEnabled = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckForUpdatesOnStartup = chbUpdatesStartup.IsChecked.Value;
            Properties.Settings.Default.IsToolboxScrollBarVisible = chbShowToolboxScrollBar.IsChecked.Value;

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClearRecentFilesMenu_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RecentFiles = "";
            Properties.Settings.Default.Save();
            if (this.Owner is MainWindow)
            {
                (this.Owner as MainWindow).RecentFiles.Clear();
                (this.Owner as MainWindow).RecentFiles.Add("(empty)");
            }
            btnClearRecentFilesMenu.IsEnabled = false;
        }
    }
}
