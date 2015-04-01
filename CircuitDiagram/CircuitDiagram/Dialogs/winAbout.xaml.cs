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

using CircuitDiagram.DPIWindow;
using System;
using System.Linq;
using System.Windows;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winAbout.xaml
    /// </summary>
    public partial class winAbout : MetroDPIWindow
    {
        public winAbout()
        {
            InitializeComponent();

            lblVersionNumber.Content = UpdateManager.AppDisplayVersion;

            cbxReleaseChannel.ItemsSource = Enum.GetNames(typeof(UpdateChannelType));
            cbxReleaseChannel.SelectedItem = UpdateManager.UpdateChannel.ToString();
            cbxReleaseChannel.SelectionChanged += cbxReleaseChannel_SelectionChanged;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void cbxReleaseChannel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxReleaseChannel.SelectedItem.ToString() != UpdateManager.BuildChannelVersion.UpdateChannel.ToString())
            {
                UpdateManager.UpdateChannel = (UpdateChannelType)Enum.Parse(typeof(UpdateChannelType), cbxReleaseChannel.SelectedItem.ToString());
                Settings.Settings.Write("updateChannel", cbxReleaseChannel.SelectedItem.ToString());
                Settings.Settings.Save();
            }
            else
            {
                UpdateManager.UpdateChannel = UpdateManager.BuildChannelVersion.UpdateChannel;
                Settings.Settings.RemoveSetting("updateChannel");
            }
        }
    }
}
