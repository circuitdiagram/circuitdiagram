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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CircuitDiagram.IO;
using CircuitDiagram.IO.CDDX;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for CDDXSaveOptionsEditor.xaml
    /// </summary>
    public partial class CDDXSaveOptionsEditor : UserControl, ISaveOptionsEditor
    {
        private CDDXSaveOptions m_saveOptions;

        public CDDXSaveOptionsEditor()
        {
            InitializeComponent();

            this.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                LoadSaveOptions(m_saveOptions);
            };
        }

        public ISaveOptions Options {
            get { return m_saveOptions; }
            set
            {
                m_saveOptions = value as CDDXSaveOptions;
                LoadSaveOptions(m_saveOptions);
            }
        }

        bool m_autoApplying = false;
        public void LoadSaveOptions(CDDXSaveOptions options)
        {
            if (!IsLoaded)
                return;

            m_autoApplying = true;
            if (options == new CDDXSaveOptions(true, true, true))
                cbxSavePreset.SelectedIndex = 0;
            else if (options == new CDDXSaveOptions(false, true, false))
                cbxSavePreset.SelectedIndex = 1;
            else
                cbxSavePreset.SelectedIndex = 2;
            m_autoApplying = false;

            ApplySaveOptions();
        }

        private void ApplySaveOptions()
        {
            m_autoApplying = true;

            chbIncludeConnections.IsChecked = m_saveOptions.Connections;
            chbIncludeLayout.IsChecked = m_saveOptions.Layout;
            chbEmbedThumbnail.IsChecked = m_saveOptions.Thumbnail;

            m_autoApplying = false;
        }

        private void cbxSavePreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (cbxSavePreset.SelectedIndex == 0)
            {
                // Default settings
                m_saveOptions = new CDDXSaveOptions(true, true, true);
                ApplySaveOptions();
            }
            else if (cbxSavePreset.SelectedIndex == 1)
            {
                // Minimum size
                m_saveOptions = new CDDXSaveOptions(false, true, false);
                ApplySaveOptions();
            }
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

            m_saveOptions.Connections = chbIncludeConnections.IsChecked.Value;

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

            m_saveOptions.Layout = chbIncludeLayout.IsChecked.Value;

            OptionChanged();
        }

        private void chbEmbedThumbnail_Checked(object sender, RoutedEventArgs e)
        {
            m_saveOptions.Thumbnail = chbEmbedThumbnail.IsChecked.Value;

            OptionChanged();
        }

        void OptionChanged()
        {
            if (m_autoApplying)
                return;

            // Set custom preset
            cbxSavePreset.SelectedIndex = 2;
        }
    }
}
