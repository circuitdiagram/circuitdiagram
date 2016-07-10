using CircuitDiagram.DPIWindow;
using CircuitDiagram.IO;
using System.Windows;
using System.Windows.Controls;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winSaveOptions.xaml
    /// </summary>
    public partial class winSaveOptions : MetroDPIWindow
    {
        private UserControl m_editor;

        public winSaveOptions(UserControl editor, ISaveOptions options)
        {
            InitializeComponent();

            m_editor = editor;
            stpPlugin.Children.Add(editor);
            ((ISaveOptionsEditor)editor).Options = options;
        }

        public ISaveOptions Options { get { return ((ISaveOptionsEditor)m_editor).Options; } }

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
