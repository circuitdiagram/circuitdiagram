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

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winSaveOptions.xaml
    /// </summary>
    public partial class winSaveOptions : Window
    {
        private SaveOptionsEditor m_editor;

        public winSaveOptions(SaveOptionsEditor editor, ISaveOptions options)
        {
            InitializeComponent();

            m_editor = editor;
            stpPlugin.Children.Add(editor);
            editor.SetOptions(options);
        }

        public ISaveOptions Options { get { return m_editor.GetOptions(); } }

        public bool AlwaysUseSettings { get { return chbAlwaysUseSettings.IsChecked == true; } }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
