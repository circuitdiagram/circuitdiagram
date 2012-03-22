// winNewComponentImplementation.xaml.cs
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

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winNewComponentImplementation.xaml
    /// </summary>
    public partial class winNewComponentImplementation : Window
    {
        public winNewComponentImplementation()
        {
            InitializeComponent();

            // Populate representation combobox
            foreach (ComponentDescription description in ComponentHelper.ComponentDescriptions)
            {
                if (description.Metadata.Configurations.Count == 0)
                {
                    ImplementationConversion item = new ImplementationConversion();
                    item.ToName = description.ComponentName;
                    item.ToGUID = description.Metadata.GUID;
                    item.ToIcon = description.Metadata.Icon as ImageSource;
                    cbxRepresentation.Items.Add(item);
                }
                else
                {
                    foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                    {
                        ImplementationConversion item = new ImplementationConversion();
                        item.ToName = description.ComponentName;
                        item.ToGUID = description.Metadata.GUID;
                        item.ToIcon = configuration.Icon as ImageSource;
                        item.ToConfiguration = configuration.Name;
                        cbxRepresentation.Items.Add(item);
                    }
                }
            }
        }

        public string ImplementationSet
        {
            get { return tbxImplementSet.Text; }
            set { tbxImplementSet.Text = value; }
        }

        public ImplementationConversion GetChosenComponent()
        {
            ImplementationConversion item = cbxRepresentation.SelectedItem as ImplementationConversion;
            item.ImplementationName = tbxImplementItem.Text;
            return item;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
