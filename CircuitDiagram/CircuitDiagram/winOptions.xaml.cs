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
    }
}
