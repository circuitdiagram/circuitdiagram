// OutputDeviceEditor.xaml.cs
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
    using T = OutputDevice;

    /// <summary>
    /// Interaction logic for ResistorEditor.xaml
    /// </summary>
    partial class OutputDeviceEditor : ComponentEditor<T>
    {
        public OutputDeviceEditor(T component)
            :base(component)
        {
            InitializeComponent();
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            radLoudspeaker.IsChecked = false;
            radBuzzer.IsChecked = false;
            radMotor.IsChecked = false;
            radHeater.IsChecked = false;
            switch (Component.Type)
            {
                case OutputDeviceType.Loudspeaker:
                    radLoudspeaker.IsChecked = true;
                    break;
                case OutputDeviceType.Buzzer:
                    radBuzzer.IsChecked = true;
                    break;
                case OutputDeviceType.Motor:
                    radMotor.IsChecked = true;
                    break;
                case OutputDeviceType.Heater:
                    radHeater.IsChecked = true;
                    break;
            }
            IsLoadingComponent = false;
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            string previousData = GetComponentData();
            if (radLoudspeaker.IsChecked == true)
                Component.Type = OutputDeviceType.Loudspeaker;
            else if (radBuzzer.IsChecked == true)
                Component.Type = OutputDeviceType.Buzzer;
            else if (radMotor.IsChecked == true)
                Component.Type = OutputDeviceType.Motor;
            else if (radHeater.IsChecked == true)
                Component.Type = OutputDeviceType.Heater;
            base.CallComponentUpdated(previousData);
        }
    }
}
