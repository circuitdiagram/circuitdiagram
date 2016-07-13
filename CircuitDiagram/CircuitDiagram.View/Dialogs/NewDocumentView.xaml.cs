// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CircuitDiagram.View.Dialogs
{
    /// <summary>
    /// Interaction logic for NewDocumentView.xaml
    /// </summary>
    public partial class NewDocumentView : UserControl
    {
        private readonly Regex numMatch;

        public NewDocumentView()
        {
            InitializeComponent();

            numMatch = new Regex("[0-9]{0,5}");
        }

        private void ValidateNumericInput(object sender, TextCompositionEventArgs e)
        {
            Match match = numMatch.Match(((TextBox)sender).Text + e.Text);
            e.Handled = match.Length != (((TextBox)sender).Text + e.Text).Length;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Window.GetWindow(this);
            dialog.DialogResult = false;
            dialog.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var dialog = Window.GetWindow(this);
            dialog.DialogResult = true;
            dialog.Close();
        }
    }
}
