// winComponents.xaml.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2015  Sam Fisher
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
using System.Windows.Shapes;
using CircuitDiagram.Components;
using System.Collections;
using System.Globalization;
using CircuitDiagram.Components.Description;
using NativeHelpers;
using MahApps.Metro.Controls;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winComponents.xaml
    /// </summary>
    public partial class winComponents : MetroWindow
    {
        public winComponents()
        {
            InitializeComponent();
            lbxComponents.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ComponentName", System.ComponentModel.ListSortDirection.Ascending));

            this.DPIChanged += winComponents_DPIChanged;
        }

        void winComponents_DPIChanged(object sender, EventArgs e)
        {
            var imageConverter = this.Resources["MultiResolutionImageToIMageSourceConverter"] as MultiResolutionImageToImageSourceConverter;
            imageConverter.DPI = CurrentDPI;
        }

        public object Components
        {
            get { return this.DataContext; }
            set { this.DataContext = value; }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void hyperlinkGetMoreComponents_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.circuit-diagram.org/components");
        }

        private void hyperlinkComponentsFolder_Click(object sender, RoutedEventArgs e)
        {
#if !PORTABLE
            string componentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\components";
#endif

#if PORTABLE
            string componentsDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\components";
#endif

            if (!System.IO.Directory.Exists(componentsDirectory))
                System.IO.Directory.CreateDirectory(componentsDirectory);

            System.Diagnostics.Process.Start(componentsDirectory);
        }

        private void lnkViewCertificate_Click(object sender, RoutedEventArgs e)
        {
            ComponentDescription selected = lbxComponents.SelectedItem as ComponentDescription;
            if (selected != null && selected.Metadata.Signature.Certificate != null)
            {
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
                System.Security.Cryptography.X509Certificates.X509Certificate2UI.DisplayCertificate(selected.Metadata.Signature.Certificate, helper.Handle);
            }
        }

        private void hyperlinkComponentDetails_Click(object sender, RoutedEventArgs e)
        {
            winComponentDetails componentDetailsWindow = new winComponentDetails(lbxComponents.SelectedItem as ComponentDescription);
            componentDetailsWindow.Owner = this;
            componentDetailsWindow.ShowDialog();
        }
    }

    class EmbedByDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            ComponentDescription description = value as ComponentDescription;
            if (description != null)
            {
                return ComponentHelper.IsStandardComponent(description) ? "No" : "Yes";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
