using CircuitDiagram.DPIWindow;
using CircuitDiagram.IO;
using System.Windows;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winSaveOptions.xaml
    /// </summary>
    public partial class winSaveOptions : MetroDPIWindow
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
