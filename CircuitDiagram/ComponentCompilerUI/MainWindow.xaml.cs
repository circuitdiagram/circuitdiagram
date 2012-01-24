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

            chbSign_Checked(this, null);
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

        private void chbSign_Checked(object sender, RoutedEventArgs e)
        {
            if (chbSign.IsChecked == true)
            {
                lblKey.IsEnabled = true;
                tbxKeyPath.IsEnabled = true;
                btnKeyBrowse.IsEnabled = true;
            }
            else
            {
                lblKey.IsEnabled = false;
                tbxKeyPath.IsEnabled = false;
                btnKeyBrowse.IsEnabled = false;
            }
        }

        private void btnCompile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "Circuit Diagram Component (*.cdcom)|*.cdcom";
            if (sfd.ShowDialog() == true)
            {
                if (!String.IsNullOrEmpty(tbxInputPath.Text))
                {
                    string xmlPath = System.IO.Path.GetDirectoryName(sfd.FileName) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sfd.FileName) + ".temp.xml";

                    SaveConfiguration(xmlPath);

                    string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    string one = path + "\\" + String.Format("cdcompile.exe -o \"{0}\" --config \"{1}\"", sfd.FileName, xmlPath);
                    System.Diagnostics.Process.Start(path + "\\cdcompile.exe", String.Format("-o \"{0}\" --config \"{1}\"", sfd.FileName, xmlPath));

                    // Wait for process to complete
                    System.Threading.Thread.Sleep(700);
                    System.IO.File.Delete(xmlPath);
                }
            }
        }

        private void SaveConfiguration(string xmlPath)
        {
            System.IO.FileStream fs = new System.IO.FileStream(xmlPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read);
            XmlTextWriter writer = new XmlTextWriter(fs, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("cdcom");
            writer.WriteAttributeString("markupversion", "1.0");
            writer.WriteAttributeString("x-uiapp", "Component Compiler 1.0 (Windows)");
            if (chbSign.IsChecked == true && !String.IsNullOrEmpty(tbxKeyPath.Text))
                writer.WriteAttributeString("key", tbxKeyPath.Text);

            // Component
            writer.WriteStartElement("component");
            writer.WriteAttributeString("id", "C0");
            writer.WriteAttributeString("path", tbxInputPath.Text);

            // Icon
            if (!String.IsNullOrEmpty(tbxIconPath.Text))
            {
                writer.WriteStartElement("icon");
                writer.WriteValue(tbxIconPath.Text);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.Flush();
            writer.Close();

            fs.Close();
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "XML Files (*.xml)|*.xml";
            if (sfd.ShowDialog() == true)
            {
                SaveConfiguration(sfd.FileName);
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open";
            ofd.Filter = "XML Files (*.xml)|*.xml";
            if (ofd.ShowDialog() == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ofd.FileName);

                if ((doc.SelectSingleNode("/cdcom") as XmlElement).HasAttribute("key"))
                {
                    chbSign.IsChecked = true;
                    tbxKeyPath.Text = (doc.SelectSingleNode("/cdcom") as XmlElement).Attributes["key"].InnerText;
                }

                XmlNode componentNode = doc.SelectSingleNode("/cdcom/component");
                if (componentNode != null)
                {
                    tbxInputPath.Text = componentNode.Attributes["path"].InnerText;

                    foreach (XmlNode childNode in componentNode.ChildNodes)
                    {
                        if (childNode.Name == "icon" && !(childNode as XmlElement).HasAttribute("configuration"))
                        {
                            tbxInputPath.Text = childNode.InnerText;
                        }
                    }
                }
            }
        }

        private void btnKeyBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Key";
            ofd.Filter = "Key Files (*.xml;*.txt)|*.xml;*.txt";
            if (ofd.ShowDialog() == true)
            {
                tbxKeyPath.Text = ofd.FileName;
            }
        }
    }
}
