using CircuitDiagram.DPIWindow;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winDocumentSize.xaml
    /// </summary>
    public partial class winDocumentSize : MetroDPIWindow
    {
        Regex numMatch;

        public winDocumentSize()
        {
            InitializeComponent();

            numMatch = new Regex("[0-9]{0,5}");
        }

        public double DocumentWidth
        {
            get { return double.Parse(tbxWidth.Text); }
            set { tbxWidth.Text = value.ToString(); }
        }

        public double DocumentHeight
        {
            get { return double.Parse(tbxHeight.Text); }
            set { tbxHeight.Text = value.ToString(); }
        }

        private void ValidateNumericInput(object sender, TextCompositionEventArgs e)
        {
            Match match = numMatch.Match(((TextBox)sender).Text + e.Text);
            e.Handled = match.Length != (((TextBox)sender).Text + e.Text).Length;
        }

        private void tbxDocWidthHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOK.IsEnabled = (tbxWidth.Text.Length > 0 && tbxHeight.Text.Length > 0);
        }

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
