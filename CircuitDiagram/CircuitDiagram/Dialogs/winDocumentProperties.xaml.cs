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

using CircuitDiagram.Components;
using CircuitDiagram.DPIWindow;
using CircuitDiagram.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winDocument.xaml
    /// </summary>
    public partial class winDocumentProperties : MetroDPIWindow
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

            // Embedded components
            document.UpdateEmbedComponents();
            lbxEmbedComponents.ItemsSource = document.Metadata.EmbedComponents;

            // Components used in document - convert to tree structure
            Dictionary<string, List<ComponentUsageItem>> processed = new Dictionary<string, List<ComponentUsageItem>>();

            // Sort components into collections
            foreach (IComponentElement element in document.Elements.Where(item => item is IComponentElement))
            {
                if (element is Component && (element as Component).Description == ComponentHelper.WireDescription)
                    continue; // Ignore wires

                string key = element.ImplementationCollection;
                if (element.ImplementationCollection == CircuitDiagram.IO.ComponentCollections.Common)
                    key = "Common";
                else if (element.ImplementationCollection == CircuitDiagram.IO.ComponentCollections.Misc)
                    key = "Misc";
                else if (String.IsNullOrEmpty(element.ImplementationCollection))
                    key = "(unknown)";

                string name = element.ImplementationItem;
                bool nonstandard = false;
                if (String.IsNullOrEmpty(element.ImplementationItem))
                {
                    if (element is Component)
                        name = (element as Component).Description.ComponentName;
                    else
                        name = "Unnamed component";
                    nonstandard = true;
                }

                if (processed.ContainsKey(key) && processed[key].Find(item => item.Name == name) != null)
                    continue; // Avoid duplicates

                if (!processed.ContainsKey(key))
                    processed.Add(key, new List<ComponentUsageItem>());
                processed[key].Add(new ComponentUsageItem(name, true, nonstandard));
            }

            // Add disabled components
            foreach (DisabledComponent component in document.DisabledComponents)
            {
                string key = component.ImplementationCollection;
                if (component.ImplementationCollection == CircuitDiagram.IO.ComponentCollections.Common)
                    key = "Common";
                else if (component.ImplementationCollection == CircuitDiagram.IO.ComponentCollections.Misc)
                    key = "Misc";
                else if (String.IsNullOrEmpty(component.ImplementationCollection))
                    key = "(unknown)";

                string name = component.ImplementationItem;
                bool nonstandard = false;
                if (String.IsNullOrEmpty(component.ImplementationItem))
                {
                    if (!String.IsNullOrEmpty(component.Name))
                        name = component.Name;
                    else
                        name = String.Format("Unnamed component");
                    nonstandard = true;
                }

                if (processed.ContainsKey(key) && processed[key].Find(item => item.Name == name) != null)
                    continue; // Avoid duplicates

                if (!processed.ContainsKey(key))
                    processed.Add(key, new List<ComponentUsageItem>());
                processed[key].Add(new ComponentUsageItem(name, false, nonstandard));
            }

            // Add to treeview
            foreach (KeyValuePair<string, List<ComponentUsageItem>> item in processed)
            {
                TreeViewItem collectionItem = new TreeViewItem();
                TextBlock header = new TextBlock();
                header.Text = item.Key;
                if (item.Key == "Common")
                    header.ToolTip = CircuitDiagram.IO.ComponentCollections.Common;
                else if (item.Key == "Misc")
                    header.ToolTip = CircuitDiagram.IO.ComponentCollections.Misc;
                collectionItem.Header = header;

                item.Value.Sort(new Comparison<ComponentUsageItem>((element1, element2) => element1.Name.CompareTo(element2.Name)));
                item.Value.ForEach(leaf =>
                {
                    TreeViewItem trvItem = new TreeViewItem();
                    trvItem.Header = leaf.Name;
                    if (!leaf.IsAvailable )
                    {
                        trvItem.Foreground = Brushes.Gray;
                        trvItem.ToolTip = "not available";
                    }

                    collectionItem.Items.Add(trvItem);
                });

                collectionItem.IsExpanded = true;

                trvComponents.Items.Add(collectionItem);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        class ComponentUsageItem
        {
            public string Name { get; set; }
            public bool IsAvailable { get; set; }
            public bool IsStandard { get; set; }

            public ComponentUsageItem(string name, bool isAvailable = true, bool isStandard = true)
            {
                Name = name;
                IsAvailable = isAvailable;
                IsStandard = isStandard;
            }
        }
    }
}
