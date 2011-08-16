// MicrocontrollerEditor.xaml.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
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
    using T = Microcontroller;

    /// <summary>
    /// Interaction logic for MicrocontrollerEditor.xaml
    /// </summary>
    partial class MicrocontrollerEditor : ComponentEditor<T>
    {
        public MicrocontrollerEditor(T component)
            : base(component)
        {
            InitializeComponent();
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
            base.OnPreviewTextInput(e);
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            tbxNumInputs.Text = ((Microcontroller)Component).Inputs.ToString();
            tbxNumOutputs.Text = ((Microcontroller)Component).Outputs.ToString();
            chbADCInput.IsChecked = ((Microcontroller)Component).ADC;
            chbDisplayPIC.IsChecked = ((Microcontroller)Component).DisplayPIC;
            IsLoadingComponent = false;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            string previousData = GetComponentData();
            try
            {
                Component.Inputs = int.Parse(tbxNumInputs.Text);
                Component.Outputs = int.Parse(tbxNumOutputs.Text);
            }
            catch (Exception)
            {
            }
            finally
            {
                Component.Inputs = Math.Min(Component.Inputs, 20);
                Component.Outputs = Math.Min(Component.Outputs, 20);
                base.CallComponentUpdated(previousData);
            }
        }

        private void CheckboxChecked(object sender, EventArgs e)
        {
            string previousData = GetComponentData();
            Component.ADC = chbADCInput.IsChecked.Value;
            Component.DisplayPIC = chbDisplayPIC.IsChecked.Value;
            base.CallComponentUpdated(previousData);
        }
    }
}
