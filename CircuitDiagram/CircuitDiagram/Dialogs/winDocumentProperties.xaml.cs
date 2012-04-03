// winDocumentProperties.xaml.cs
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
using System.Windows.Shapes;
using CircuitDiagram.Components;
using System.IO;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winDocument.xaml
    /// </summary>
    public partial class winDocumentProperties : Window
    {
        private CircuitDocument m_document;

        public winDocumentProperties()
        {
            InitializeComponent();
        }

        public void SetDocument(CircuitDocument document)
        {
            m_document = document;

            this.DataContext = document;

            tbxDimensions.Text = String.Format("{0}x{1}", document.Size.Width, document.Size.Height);

            List<ComponentDescription> componentDescriptions = new List<ComponentDescription>();
            foreach (Component component in document.Components)
                if (!componentDescriptions.Contains(component.Description))
                    componentDescriptions.Add(component.Description);
            lbxComponents.ItemsSource = componentDescriptions;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbxComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxComponents.SelectedItem != null && lbxComponents.SelectedItem is ComponentDescription &&
                (lbxComponents.SelectedItem as ComponentDescription).Metadata.Location == ComponentDescriptionMetadata.LocationType.Embedded)
                btnInstallComponent.IsEnabled = true;
            else
                btnInstallComponent.IsEnabled = false;
        }

        private void btnInstallComponent_Click(object sender, RoutedEventArgs e)
        {
            if (lbxComponents.SelectedItem != null && lbxComponents.SelectedItem is ComponentDescription &&
                (lbxComponents.SelectedItem as ComponentDescription).Metadata.Location == ComponentDescriptionMetadata.LocationType.Embedded)
            {
                ComponentDescription selectedDescription = lbxComponents.SelectedItem as ComponentDescription;

                if (selectedDescription.Metadata.GUID != Guid.Empty)
                {
                    foreach (ComponentDescription description in ComponentHelper.ComponentDescriptions)
                    {
                        if (description.Metadata.Location == ComponentDescriptionMetadata.LocationType.Installed && description.Metadata.GUID == selectedDescription.Metadata.GUID)
                        {
                            MessageBox.Show("The component is already installed.", "Could Not Install Component", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                }

#if PORTABLE
                string userComponentsDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\components";
#else
                string userComponentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\components";
#endif

                if (!Directory.Exists(userComponentsDirectory))
                    Directory.CreateDirectory(userComponentsDirectory);

                try
                {
                    using (FileStream outputStream = new FileStream(userComponentsDirectory + "\\" + selectedDescription.ComponentName.ToLowerInvariant() + ".cdcom", FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        CircuitDiagram.IO.BinaryWriter writer = new IO.BinaryWriter(outputStream, new IO.BinaryWriter.BinaryWriterSettings());
                        writer.Descriptions.Add(selectedDescription);
                        writer.Write();
                    }
                    MessageBox.Show("The component \"" + selectedDescription.ComponentName + "\" was installed successfully.", "Install Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("An unknown error occurred.", "Unable to Install Component", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
