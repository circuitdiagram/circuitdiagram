// winCDDXSave.xaml.cs
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
using CircuitDiagram.IO;
using CircuitDiagram.IO.CDDX;
using CircuitDiagram.Components;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winCDDXExport.xaml
    /// </summary>
    public partial class winCDDXSave : Window
    {
        public CDDXSaveOptions SaveOptions { get; private set; }
        public bool AlwaysUseSettings { get { return chbAlwaysUseSettings.IsChecked.Value; } set { chbAlwaysUseSettings.IsChecked = value; } }
        public IEnumerable<ComponentDescription> AvailableComponents { get; set; }

        public winCDDXSave()
        {
            InitializeComponent();

            SaveOptions = new CDDXSaveOptions();

            this.Loaded += new RoutedEventHandler(winCDDXSave_Loaded);
        }

        void winCDDXSave_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSaveOptions(SaveOptions);
        }

        bool m_autoApplying = false;
        public void LoadSaveOptions(CDDXSaveOptions options)
        {
            SaveOptions = options;

            if (!IsLoaded)
                return;

            m_autoApplying = true;
            if (SaveOptions == CDDXSaveOptions.Default)
                cbxSavePreset.SelectedIndex = 0;
            else if (SaveOptions == CDDXSaveOptions.MinSize)
                cbxSavePreset.SelectedIndex = 1;
            else
                cbxSavePreset.SelectedIndex = 2;
            m_autoApplying = false;

            ApplySaveOptions();
        }

        private void ApplySaveOptions()
        {
            m_autoApplying = true;

            chbIncludeConnections.IsChecked = SaveOptions.IncludeConnections;
            chbIncludeLayout.IsChecked = SaveOptions.IncludeLayout;
            chbEmbedThumbnail.IsChecked = SaveOptions.EmbedThumbnail;

            if (SaveOptions.EmbedComponents == CDDXSaveOptions.ComponentsToEmbed.Automatic)
                radEmbedComponnetsAutomatic.IsChecked = true;
            else if (SaveOptions.EmbedComponents == CDDXSaveOptions.ComponentsToEmbed.All)
                radEmbedComponentsAll.IsChecked = true;
            else if (SaveOptions.EmbedComponents == CDDXSaveOptions.ComponentsToEmbed.Custom)
                radEmbedComponentsCustom.IsChecked = true;
            else
                radEmbedComponentsNone.IsChecked = true;

            m_autoApplying = false;
        }

        private void chbIncludeConnections_Checked(object sender, RoutedEventArgs e)
        {
            if (chbIncludeConnections.IsChecked == true)
                chbIncludeLayout.IsEnabled = true;
            else
            {
                chbIncludeLayout.IsChecked = true;
                chbIncludeLayout.IsEnabled = false;
            }

            SaveOptions.IncludeConnections = chbIncludeConnections.IsChecked.Value;

            OptionChanged();
        }

        private void chbIncludeLayout_Checked(object sender, RoutedEventArgs e)
        {
            if (chbIncludeLayout.IsChecked == true)
                chbIncludeConnections.IsEnabled = true;
            else
            {
                chbIncludeConnections.IsChecked = true;
                chbIncludeConnections.IsEnabled = false;
            }

            SaveOptions.IncludeLayout = chbIncludeLayout.IsChecked.Value;

            OptionChanged();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (chbAlwaysUseSettings.IsEnabled == false)
                chbAlwaysUseSettings.IsChecked = false;

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void radEmbedComponents_Changed(object sender, RoutedEventArgs e)
        {
            if (radEmbedComponnetsAutomatic.IsChecked == true)
                SaveOptions.EmbedComponents = CDDXSaveOptions.ComponentsToEmbed.Automatic;
            else if (radEmbedComponentsAll.IsChecked == true)
                SaveOptions.EmbedComponents = CDDXSaveOptions.ComponentsToEmbed.All;
            else if (radEmbedComponentsCustom.IsChecked == true)
                SaveOptions.EmbedComponents = CDDXSaveOptions.ComponentsToEmbed.Custom;
            else
                SaveOptions.EmbedComponents = CDDXSaveOptions.ComponentsToEmbed.None;

            OptionChanged();
        }

        private void btnChooseEmbedComponentsCustom_Click(object sender, RoutedEventArgs e)
        {
            winCDDXSaveComponents chooseComponentsWindow = new winCDDXSaveComponents(SaveOptions.CustomEmbedComponents, AvailableComponents);
            chooseComponentsWindow.Owner = this;
            chooseComponentsWindow.ShowDialog();
        }

        private void chbEmbedThumbnail_Checked(object sender, RoutedEventArgs e)
        {
            SaveOptions.EmbedThumbnail = chbEmbedThumbnail.IsChecked.Value;

            OptionChanged();
        }

        private void cbxSavePreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (cbxSavePreset.SelectedIndex == 0)
            {
                // Default settings
                SaveOptions = CDDXSaveOptions.Default;
                ApplySaveOptions();
            }
            else if (cbxSavePreset.SelectedIndex == 1)
            {
                // Minimum size
                SaveOptions = CDDXSaveOptions.MinSize;
                ApplySaveOptions();
            }
        }

        void OptionChanged()
        {
            if (m_autoApplying)
                return;

            // Set custom preset
            cbxSavePreset.SelectedIndex = 2;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
