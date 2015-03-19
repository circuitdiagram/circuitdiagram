// winAbout.xaml.cs
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

using MahApps.Metro.Controls;
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
    /// Interaction logic for winAbout.xaml
    /// </summary>
    public partial class winAbout : MetroWindow
    {
        public winAbout()
        {
            InitializeComponent();

            lblVersionNumber.Content = AppVersion;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static string AppVersion
        {
            get
            {
                System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();

                string theVersion = string.Empty;
                if (_assemblyInfo != null)
                {
                    Version ver = _assemblyInfo.GetName().Version;
                    if (ver.Revision == 0 && ver.Build == 0)
                        theVersion = String.Format("{0}.{1}", ver.Major, ver.Minor);
                    else
                        theVersion = _assemblyInfo.GetName().Version.ToString();
                }
                BuildChannelAttribute channelAttribute = _assemblyInfo.GetCustomAttributes(typeof(BuildChannelAttribute), false).FirstOrDefault(item => item is BuildChannelAttribute) as BuildChannelAttribute;
                if (channelAttribute != null && channelAttribute.Type == BuildChannelAttribute.ChannelType.Dev && !String.IsNullOrEmpty(channelAttribute.DisplayName))
                    theVersion += " " + channelAttribute.DisplayName;

#if PORTABLE
                theVersion += " Portable";
#endif

                return theVersion;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
