// MicrocontrollerEditor.xaml.cs
//
// Circuit Diagram http://circuitdiagram.codeplex.com/
//
// Copyright (C) 2011  Sam Fisher
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CircuitDiagram.EComponents
{
    /// <summary>
    /// Interaction logic for MicrocontrollerEditor.xaml
    /// </summary>
    public partial class MicrocontrollerEditor : ComponentEditor
    {
        public MicrocontrollerEditor()
        {
            InitializeComponent();
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
            base.OnPreviewTextInput(e);
        }

        private bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsNumber(c)) return false;
            }

            return true;
        }

        public override void LoadComponent(EComponent component)
        {
            tbxNumInputs.Text = ((Microcontroller)component).Inputs.ToString();
            tbxNumOutputs.Text = ((Microcontroller)component).Outputs.ToString();
            chbADCInput.IsChecked = ((Microcontroller)component).ADC;
            chbDisplayPIC.IsChecked = ((Microcontroller)component).DisplayPIC;
        }

        public override void UpdateChanges(EComponent component)
        {
            try
            {
                Microcontroller microcontroller = (Microcontroller)component;
                microcontroller.Inputs = int.Parse(tbxNumInputs.Text);
                microcontroller.Outputs = int.Parse(tbxNumOutputs.Text);
                microcontroller.ADC = chbADCInput.IsChecked.Value;
                microcontroller.DisplayPIC = chbDisplayPIC.IsChecked.Value;
            }
            catch (Exception)
            {
            }
        }
    }
}
