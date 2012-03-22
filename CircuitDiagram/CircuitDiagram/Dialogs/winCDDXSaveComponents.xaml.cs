// winCDDXSaveComponents.cs
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
    /// Interaction logic for winCDDXSaveComponents.xaml
    /// </summary>
    public partial class winCDDXSaveComponents : Window
    {
        private List<ComponentDescription> m_components;

        public winCDDXSaveComponents(List<ComponentDescription> components, IEnumerable<ComponentDescription> availableComponents)
        {
            InitializeComponent();

            m_components = components;

            foreach (var item in availableComponents)
                lbxAvailable.Items.Add(item);

            UpdateButtons();
        }

        void UpdateButtons()
        {
            if (lbxAvailable.Items.Count > 0)
                btnAddAll.IsEnabled = true;
            else
                btnAddAll.IsEnabled = false;

            if (lbxAvailable.SelectedItem != null)
                btnAdd.IsEnabled = true;
            else
                btnAdd.IsEnabled = false;

            if (lbxIncluded.Items.Count > 0)
                btnRemoveAll.IsEnabled = true;
            else
                btnRemoveAll.IsEnabled = false;

            if (lbxIncluded.SelectedItem != null)
                btnRemove.IsEnabled = true;
            else
                btnRemove.IsEnabled = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lbxAvailable.SelectedItem != null)
            {
                ComponentDescription item = lbxAvailable.SelectedItem as ComponentDescription;
                lbxAvailable.Items.Remove(item);
                lbxIncluded.Items.Add(item);
            }

            UpdateButtons();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbxIncluded.SelectedItem != null)
            {
                ComponentDescription item = lbxIncluded.SelectedItem as ComponentDescription;
                lbxIncluded.Items.Remove(item);
                lbxAvailable.Items.Add(item);
            }

            UpdateButtons();
        }

        private void btnAddAll_Click(object sender, RoutedEventArgs e)
        {
            while (lbxAvailable.Items.Count > 0)
            {
                ComponentDescription description = lbxAvailable.Items[0] as ComponentDescription;
                lbxAvailable.Items.Remove(description);
                lbxIncluded.Items.Add(description);
            }

            UpdateButtons();
        }

        private void btnRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            while (lbxIncluded.Items.Count > 0)
            {
                ComponentDescription description = lbxIncluded.Items[0] as ComponentDescription;
                lbxIncluded.Items.Remove(description);
                lbxAvailable.Items.Add(description);
            }

            UpdateButtons();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            m_components.AddRange(lbxIncluded.Items.OfType<ComponentDescription>());
            this.Close();
        }

        private void lbxAvailable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void lbxIncluded_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }
    }
}
