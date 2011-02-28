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
using System.Text.RegularExpressions;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for NewDocument.xaml
    /// </summary>
    public partial class NewDocument : Window
    {
        Regex numMatch;

        public string TbxWidth
        {
            get { return tbxWidth.Text; }
            set { tbxWidth.Text = value; }
        }

        public string TbxHeight
        {
            get { return tbxHeight.Text; }
            set { tbxHeight.Text = value; }
        }

        public NewDocument()
        {
            InitializeComponent();
            numMatch = new Regex("[0-9]{0,5}");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ValidateNumericInput(object sender, TextCompositionEventArgs e)
        {
            Match match = numMatch.Match(((TextBox)sender).Text + e.Text);
            e.Handled = match.Length != (((TextBox)sender).Text + e.Text).Length;
        }
    }
}
