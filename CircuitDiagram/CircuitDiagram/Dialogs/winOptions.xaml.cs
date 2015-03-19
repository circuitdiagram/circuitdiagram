// winOptions.xaml.cs
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
using CircuitDiagram.Settings;
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using MahApps.Metro.Controls;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winOptions.xaml
    /// </summary>
    public partial class winOptions : MetroWindow
    {
        public winOptions()
        {
            InitializeComponent();

            // General
            chbShowConnectionPoints.IsChecked = Settings.Settings.ReadBool("showConnectionPoints");
            chbShowGrid.IsChecked = Settings.Settings.ReadBool("showEditorGrid");
            chbShowToolboxScrollBar.IsChecked = Settings.Settings.ReadBool("showToolboxScrollBar");
            chbCheckForUpdatesAutomatically.IsChecked = Settings.Settings.ReadBool("CheckForUpdatesOnStartup");

            // CDDX
            cbxEmbedComponents.SelectedIndex = 0;
            if (Settings.Settings.HasSetting("EmbedComponents"))
                cbxEmbedComponents.SelectedIndex = (int)Settings.Settings.Read("EmbedComponents");
            chbShowCDDXOptions.IsChecked = !Settings.Settings.ReadBool("CDDX.AlwaysUseSettings");
            chbCreatorUseComputerUserName.IsChecked = true;
            if (Settings.Settings.HasSetting("CreatorUseComputerUserName"))
                chbCreatorUseComputerUserName.IsChecked = Settings.Settings.ReadBool("CreatorUseComputerUserName");
            string creatorName = Settings.Settings.Read("ComputerUserName") as string;
            if (Settings.Settings.HasSetting("CreatorName"))
                tbxCreatorName.Text = Settings.Settings.Read("CreatorName") as string;

            // Plugins
            foreach (var item in PluginManager.Plugins)
                lbxPlugins.Items.Add(new PluginListItem(item, item.Name, item.Author, item.Version, PluginManager.IsPluginEnabled(item)));
        }

        public List<ImplementationConversionCollection> ComponentRepresentations
        {
            get { return tabImplementations.DataContext as List<ImplementationConversionCollection>; }
            set { tabImplementations.DataContext = value; }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // General
            Settings.Settings.Write("showConnectionPoints", chbShowConnectionPoints.IsChecked.Value);
            Settings.Settings.Write("showEditorGrid", chbShowGrid.IsChecked.Value);
            Settings.Settings.Write("showToolboxScrollBar", chbShowToolboxScrollBar.IsChecked.Value);
            Settings.Settings.Write("CheckForUpdatesOnStartup", chbCheckForUpdatesAutomatically.IsChecked.Value);

            // CDDX
            Settings.Settings.Write("EmbedComponents", cbxEmbedComponents.SelectedIndex);
            ComponentHelper.EmbedOptions = (ComponentEmbedOptions)cbxEmbedComponents.SelectedIndex;
            Settings.Settings.Write("CDDX.AlwaysUseSettings", chbShowCDDXOptions.IsChecked.Value == false);
            Settings.Settings.Write("CreatorUseComputerUserName", chbCreatorUseComputerUserName.IsChecked == true);
            Settings.Settings.Write("CreatorName", tbxCreatorName.Text);

            // Plugins
            foreach (PluginListItem item in lbxPlugins.Items)
            {
                PluginManager.SetPluginEnabled(item.Tag as IPlugin, item.IsEnabled);
            }
            PluginManager.Update();
            PluginManager.SaveSettings();

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnImplementationsNew_Click(object sender, RoutedEventArgs e)
        {
            winNewComponentImplementation newComImplementation = new winNewComponentImplementation();
            newComImplementation.Owner = this;
            newComImplementation.ImplementationSet = cbxImplementationsSet.Text;
            if (newComImplementation.ShowDialog() == true)
            {
                if (cbxImplementationsSet.SelectedItem == null)
                {
                    ComponentRepresentations.Add(new ImplementationConversionCollection() { ImplementationSet = cbxImplementationsSet.Text });
                    cbxImplementationsSet.SelectedItem = ComponentRepresentations.Last();
                }

                ImplementationConversion conversion = newComImplementation.GetChosenComponent();

                // Don't allow duplicates
                if ((cbxImplementationsSet.SelectedItem as ImplementationConversionCollection).Items.FirstOrDefault(item => item.ImplementationName == conversion.ImplementationName) != null)
                    TaskDialogInterop.TaskDialog.ShowMessage(this, "The item could not be added because it is already present.\r\n\r\nYou can add this item again if you delete its current representation first.", "Could Not Add Item", TaskDialogInterop.TaskDialogCommonButtons.Close, TaskDialogInterop.VistaTaskDialogIcon.Warning);
                else
                {
                    (cbxImplementationsSet.SelectedItem as ImplementationConversionCollection).Items.Add(newComImplementation.GetChosenComponent());
                    
                    // Add to ComponentHelper's list of conversions
                    ComponentDescription description = ComponentHelper.FindDescription(conversion.ToGUID);
                    ComponentConfiguration theConfiguration = description.Metadata.Configurations.FirstOrDefault(check => check.Name == conversion.ToConfiguration);
                    ComponentHelper.SetStandardComponent(cbxImplementationsSet.Text, conversion.ImplementationName, description, theConfiguration);
                }
            }
        }

        private void btnImplementationsDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbxImplementationsComponents.SelectedItem != null)
                (cbxImplementationsSet.SelectedItem as ImplementationConversionCollection).Items.Remove(lbxImplementationsComponents.SelectedItem as ImplementationConversion);
        }

        private void btnClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            Settings.Settings.Write("recentfiles", new string[] { });
            if (this.Owner is MainWindow)
            {
                (this.Owner as MainWindow).RecentFiles.Clear();
                (this.Owner as MainWindow).RecentFiles.Add("(empty)");
            }

            btnClearRecentFiles.IsEnabled = false;
        }
    }

    class PluginListItem
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public object Tag { get; set; }

        public PluginListItem(object tag, string name, string author, string version, bool isEnabled)
        {
            Tag = tag;
            Name = name;
            Author = author;
            Version = version;
            IsEnabled = isEnabled;
        }
    }
}
