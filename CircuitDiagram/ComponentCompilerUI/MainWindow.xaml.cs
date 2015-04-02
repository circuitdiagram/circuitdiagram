// MainWindow.xaml.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Xml;

namespace ComponentCompiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Input";
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                tbxInputPath.Text = ofd.FileName;
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Icon";
            ofd.Filter = "PNG Files (*.png)|*.png";
            if (ofd.ShowDialog() == true)
            {
                tbxIconPath.Text = ofd.FileName;
            }
        }

        private void btnCompile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "Circuit Diagram Component (*.cdcom)|*.cdcom";
            if (sfd.ShowDialog() == true)
            {
                string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                if (!System.IO.File.Exists(path + "\\cdcompile.exe"))
                {
                    MessageBox.Show(this, "The file cdcompile.exe cannot be found.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!String.IsNullOrEmpty(tbxInputPath.Text))
                {
                    StringBuilder command = new StringBuilder();
                    command.AppendFormat("--input \"{0}\" ", tbxInputPath.Text);
                    command.AppendFormat("--output \"{0}\" ", sfd.FileName);
                    if (!String.IsNullOrEmpty(tbxIconPath.Text))
                        command.AppendFormat("--icon \"{0}\" ", tbxIconPath.Text);
                    if (chbSign.IsChecked == true)
                        command.AppendFormat("--sign");

                    System.Diagnostics.Process.Start(path + "\\cdcompile.exe", command.ToString());
                }
            }
        }
    }
}
