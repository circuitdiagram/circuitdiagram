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
    /// Interaction logic for winNewDocument.xaml
    /// </summary>
    public partial class winNewDocument : Window
    {
        Regex numMatch;

        public double DocumentWidth { get { return double.Parse(tbxDocWidth.Text); } }
        public double DocumentHeight { get { return double.Parse(tbxDocHeight.Text); } }

        public winNewDocument()
        {
            InitializeComponent();

            numMatch = new Regex("[0-9]{0,5}");

            tbxDocWidth.Text = "640";
            tbxDocHeight.Text = "480";
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

        private void ValidateNumericInput(object sender, TextCompositionEventArgs e)
        {
            Match match = numMatch.Match(((TextBox)sender).Text + e.Text);
            e.Handled = match.Length != (((TextBox)sender).Text + e.Text).Length;
        }
    }
}
