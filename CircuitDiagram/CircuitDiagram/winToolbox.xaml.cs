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
using System.Xml;
using CircuitDiagram.Components;
using System.Collections.ObjectModel;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winToolbox.xaml
    /// </summary>
    public partial class winToolbox : Window
    {
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
            public Key ShortcutKey { get; set; }

            public CategoryItem(ComponentDescription description, ComponentConfiguration configuration, Key shortcutKey)
            {
                Description = description;
                Configuration = configuration;
                ShortcutKey = shortcutKey;
            }
        }

        public winToolbox()
        {
            InitializeComponent();

            XmlDocument toolboxSettings = new XmlDocument();
            string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
            toolboxSettings.Load(toolboxSettingsPath);

            #region Populate current items
            XmlNodeList categoryNodes = toolboxSettings.SelectNodes("/display/category");
            int categoryCounter = 0;
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
                    lbxCategories.Items.Add(new Category("Category " + categoryCounter.ToString()) { Items = categoryItems });
                    categoryCounter++;
                }
            }
            #endregion

            #region Populate available items
            foreach (ComponentDescription description in ComponentHelper.ComponentDescriptions)
            {
                if (!IsItemUsed(description, null))
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
            using (System.IO.FileStream stream = new System.IO.FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml", System.IO.FileMode.Create, System.IO.FileAccess.Write))
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
            lbxCategories.Items.Add(new Category("Category"));
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
    }
}
