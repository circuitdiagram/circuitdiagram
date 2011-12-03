// ResistorEditor.cs
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
    using T = Resistor;

    /// <summary>
    /// Interaction logic for ResistorEditor.xaml
    /// </summary>
    public partial class ResistorEditor : ComponentEditor<T>
    {
        public ResistorEditor(T component)
            :base(component)
        {
            InitializeComponent();
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            tbxResistance.Text = Component.Resistance.ToString();

            radTypeStandard.IsChecked = false;
            radTypeVariable.IsChecked = false;
            radTypePotentiometer.IsChecked = false;
            radTypeThermistor.IsChecked = false;
            radTypeLDR.IsChecked = false;
            radTypeUS.IsChecked = false;
            switch (Component.ResistorType)
            {
                case ResistorType.Standard:
                    radTypeStandard.IsChecked = true;
                    break;
                case ResistorType.Variable:
                    radTypeVariable.IsChecked = true;
                    break;
                case ResistorType.Potentiometer:
                    radTypePotentiometer.IsChecked = true;
                    break;
                case ResistorType.Thermistor:
                    radTypeThermistor.IsChecked = true;
                    break;
                case ResistorType.LDR:
                    radTypeLDR.IsChecked = true;
                    break;
                case ResistorType.US:
                    radTypeUS.IsChecked = true;
                    break;
            }
            IsLoadingComponent = false;
        }

        private void tbxResistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            string previousData = GetComponentData();
            double resistance;
            double.TryParse(tbxResistance.Text, out resistance);
            Component.Resistance = resistance;
            CallComponentUpdated(previousData);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ComponentEditorBase.AreAllValidNumericChars(e.Text);
            base.OnPreviewTextInput(e);
        }

        private void RadioChecked(object sender, EventArgs e)
        {
            string previousData = GetComponentData();
            if (radTypeStandard.IsChecked == true)
                Component.ResistorType = ResistorType.Standard;
            else if (radTypeVariable.IsChecked == true)
                Component.ResistorType = ResistorType.Variable;
            else if (radTypePotentiometer.IsChecked == true)
                Component.ResistorType = ResistorType.Potentiometer;
            else if (radTypeThermistor.IsChecked == true)
                Component.ResistorType = ResistorType.Thermistor;
            else if (radTypeLDR.IsChecked == true)
                Component.ResistorType = ResistorType.LDR;
            else if (radTypeUS.IsChecked == true)
                Component.ResistorType = ResistorType.US;
            base.CallComponentUpdated(previousData);
        }
    }
}
