// winToolbox.xaml.cs
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
using CircuitDiagram.Components.Description;
using CircuitDiagram.DPIWindow;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winToolbox.xaml
    /// </summary>
    public partial class winToolbox : MetroDPIWindow
    {
        private int CategoryCounter { get; set; }

        class Category
        {
            public string Name { get; set; }
            public ObservableCollection<CategoryItem> Items { get; set; }

            public Category(string name)
            {
                Name = name;
                Items = new ObservableCollection<CategoryItem>();
            }
        }

        class CategoryItem
        {
            public ComponentDescription Description { get; set; }
            public ComponentConfiguration Configuration { get; set; }
            public bool NoConfiguration { get { return Configuration == null; } }
            public bool HasConfiguration { get { return Configuration != null; } }
            public MultiResolutionImage Icon { get; private set; }
            public Key ShortcutKey { get; set; }
            public string SortText
            {
                get { return (Configuration != null ? Configuration.Name : Description.ComponentName); }
            }

            public CategoryItem(ComponentDescription description, ComponentConfiguration configuration, Key shortcutKey)
            {
                Description = description;
                Configuration = configuration;
                ShortcutKey = shortcutKey;
                
                if (configuration != null && configuration.Icon != null)
                    Icon = configuration.Icon;
                else
                    Icon = description.Metadata.Icon;

                if (description == ComponentHelper.WireDescription)
                    this.ShortcutKey = Key.W;
            }
        }

        public winToolbox()
        {
            InitializeComponent();

            XmlDocument toolboxSettings = new XmlDocument();

#if PORTABLE
            string toolboxSettingsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\settings\\toolbox.xml";
#elif DEBUG
            string toolboxSettingsPath = System.IO.Path.Combine(MainWindow.ProjectDirectory, "Components\\toolbox.xml");
#else
            string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
#endif

            toolboxSettings.Load(toolboxSettingsPath);

            #region Populate current items
            XmlNodeList categoryNodes = toolboxSettings.SelectNodes("/display/category");
            foreach (XmlNode categoryNode in categoryNodes)
            {
                ObservableCollection<CategoryItem> categoryItems = new ObservableCollection<CategoryItem>();
                foreach (XmlNode node in categoryNode.ChildNodes)
                {
                    if (node.Name == "component")
                    {
                        XmlElement element = node as XmlElement;

                        Key shortcutKey = Key.None;
                        if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                            shortcutKey = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);

                        if (element.HasAttribute("guid") && element.HasAttribute("configuration"))
                        {
                            ComponentDescription description = ComponentHelper.FindDescription(new Guid(element.Attributes["guid"].InnerText));
                            if (description != null)
                            {
                                ComponentConfiguration configuration = description.Metadata.Configurations.FirstOrDefault(configItem => configItem.Name == element.Attributes["configuration"].InnerText);
                                if (configuration != null)
                                    categoryItems.Add(new CategoryItem(description, configuration, shortcutKey));
                            }
                        }
                        else if (element.HasAttribute("guid"))
                        {
                            ComponentDescription description = ComponentHelper.FindDescription(new Guid(element.Attributes["guid"].InnerText));
                            if (description != null)
                                categoryItems.Add(new CategoryItem(description, null, shortcutKey));
                        }
                        else if (element.HasAttribute("type") && element.HasAttribute("configuration"))
                        {
                            ComponentDescription description = ComponentHelper.FindDescription(element.Attributes["type"].InnerText);
                            if (description != null)
                            {
                                ComponentConfiguration configuration = description.Metadata.Configurations.FirstOrDefault(configItem => configItem.Name == element.Attributes["configuration"].InnerText);
                                if (configuration != null)
                                    categoryItems.Add(new CategoryItem(description, configuration, shortcutKey));
                            }
                        }
                        else if (element.HasAttribute("type"))
                        {
                            ComponentDescription description = ComponentHelper.FindDescription(element.Attributes["type"].InnerText);
                            if (description != null)
                                categoryItems.Add(new CategoryItem(description, null, shortcutKey));
                        }
                    }
                }
                if (categoryItems.Count > 0)
                {
                    lbxCategories.Items.Add(new Category("Category " + CategoryCounter.ToString()) { Items = categoryItems });
                    CategoryCounter++;
                }
            }
            #endregion

            #region Populate available items
            foreach (ComponentDescription description in ComponentHelper.ComponentDescriptions)
            {
                if (!IsItemUsed(description, null) && description.Metadata.Configurations.Count == 0)
                    lbxAvailableItems.Items.Add(new CategoryItem(description, null, Key.None));
                foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                {
                    if (!IsItemUsed(description, configuration))
                    {
                        lbxAvailableItems.Items.Add(new CategoryItem(description, configuration, Key.None));
                    }
                }
            }
            #endregion

            lbxAvailableItems.Items.Filter = FilterItem;
            lbxAvailableItems.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("SortText", System.ComponentModel.ListSortDirection.Ascending));
        }

        private bool FilterItem(object item)
        {
            if (String.IsNullOrEmpty(tbxAvailableFilter.Text))
                return true;

            CategoryItem categoryItem = item as CategoryItem;
            if (categoryItem != null)
            {
                if (categoryItem.Description.ComponentName.ToLowerInvariant().Contains(tbxAvailableFilter.Text.ToLowerInvariant()))
                    return true;

                if (categoryItem.HasConfiguration && categoryItem.Configuration.Name.ToLowerInvariant().Contains(tbxAvailableFilter.Text.ToLowerInvariant()))
                    return true;
            }
            return false;
        }

        private bool IsItemUsed(ComponentDescription description, ComponentConfiguration configuration)
        {
            foreach (Category category in lbxCategories.Items)
            {
                foreach (CategoryItem item in category.Items)
                {
                    if (item.Description == description && item.Configuration == configuration)
                        return true;
                }
            }
            return false;
        }

        private void SaveConfiguration()
        {
#if PORTABLE
            string toolboxSettingsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\settings\\toolbox.xml";
#elif DEBUG
            string toolboxSettingsPath = System.IO.Path.Combine(MainWindow.ProjectDirectory, "Components\\toolbox.xml");
#else
            string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
#endif

            using (System.IO.FileStream stream = new System.IO.FileStream(toolboxSettingsPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("display");

                foreach (Category category in lbxCategories.Items)
                {
                    if (category.Items.Count == 0)
                        continue;

                    writer.WriteStartElement("category");

                    foreach (CategoryItem item in category.Items)
                    {
                        writer.WriteStartElement("component");

                        if (item.Description.Metadata.GUID != Guid.Empty)
                            writer.WriteAttributeString("guid", item.Description.Metadata.GUID.ToString());
                        else
                            writer.WriteAttributeString("type", item.Description.ComponentName);

                        if (item.Configuration != null)
                            writer.WriteAttributeString("configuration", item.Configuration.Name);

                        if (item.ShortcutKey != Key.None)
                            writer.WriteAttributeString("key", item.ShortcutKey.ToString());

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCategories.SelectedItem == null)
                return;
            foreach (CategoryItem item in (lbxCategories.SelectedItem as Category).Items)
                lbxAvailableItems.Items.Add(item);
            lbxCategories.Items.Remove(lbxCategories.SelectedItem);
            lbxAvailableItems.Items.Filter = FilterItem;
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCategories.SelectedItem != null && lbxAvailableItems.SelectedItem != null)
            {
                (lbxCategories.SelectedItem as Category).Items.Add(lbxAvailableItems.SelectedItem as CategoryItem);
                lbxItems.Items.Refresh();
                lbxAvailableItems.Items.Remove(lbxAvailableItems.SelectedItem);
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCategories.SelectedItem != null && lbxItems.SelectedItem != null)
            {
                lbxAvailableItems.Items.Add(lbxItems.SelectedItem);
                (lbxCategories.SelectedItem as Category).Items.Remove(lbxItems.SelectedItem as CategoryItem);
                lbxItems.Items.Refresh();
                lbxAvailableItems.Items.Filter = FilterItem;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnNewCategory_Click(object sender, RoutedEventArgs e)
        {
            lbxCategories.Items.Add(new Category("Category " + CategoryCounter.ToString()));
            CategoryCounter++;
        }

        private void btnItemMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbxItems.SelectedItem != null)
            {
                int oldIndex = (lbxCategories.SelectedItem as Category).Items.IndexOf(lbxItems.SelectedItem as CategoryItem);
                if (oldIndex > 0)
                    (lbxCategories.SelectedItem as Category).Items.Move(oldIndex, oldIndex - 1);
            }
        }

        private void btnItemMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbxItems.SelectedItem != null)
            {
                int oldIndex = (lbxCategories.SelectedItem as Category).Items.IndexOf(lbxItems.SelectedItem as CategoryItem);
                if (oldIndex != (lbxCategories.SelectedItem as Category).Items.Count - 1)
                    (lbxCategories.SelectedItem as Category).Items.Move(oldIndex, oldIndex + 1);
            }
        }

        private void tbxAvailableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbxAvailableItems.Items.Filter = FilterItem;
        }

        private void btnCategoryUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCategories.SelectedItem != null && lbxCategories.Items.IndexOf(lbxCategories.SelectedItem) != 0)
            {
                object selectedItem = lbxCategories.SelectedItem;
                int oldIndex = lbxCategories.Items.IndexOf(selectedItem);
                lbxCategories.Items.Remove(selectedItem);
                lbxCategories.Items.Insert(oldIndex - 1, selectedItem);
                lbxCategories.SelectedItem = selectedItem;
            }
        }

        private void btnCategoryDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCategories.SelectedItem != null && lbxCategories.Items.IndexOf(lbxCategories.SelectedItem) != lbxCategories.Items.Count - 1)
            {
                object selectedItem = lbxCategories.SelectedItem;
                int oldIndex = lbxCategories.Items.IndexOf(selectedItem);
                lbxCategories.Items.Remove(selectedItem);
                lbxCategories.Items.Insert(oldIndex + 1, selectedItem);
                lbxCategories.SelectedItem = selectedItem;
            }
        }
    }
}
