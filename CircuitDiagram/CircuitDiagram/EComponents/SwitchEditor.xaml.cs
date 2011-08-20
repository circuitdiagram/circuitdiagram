// LogicGateEditor.xaml.cs
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
    using T = Switch;

    /// <summary>
    /// Interaction logic for ResistorEditor.xaml
    /// </summary>
    public partial class SwitchEditor : ComponentEditor<T>
    {
        public SwitchEditor(T component)
            :base(component)
        {
            InitializeComponent();
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            radPush.IsChecked = false;
            radToggle.IsChecked = false;
            radAnalogue.IsChecked = false;
            switch (Component.Type)
            {
                case SwitchType.Push:
                    radPush.IsChecked = true;
                    break;
                case SwitchType.Toggle:
                    radToggle.IsChecked = true;
                    break;
                case SwitchType.Analogue:
                    radAnalogue.IsChecked = true;
                    break;
            }
            IsLoadingComponent = false;
        }

        private void RadioChecked(object sender, EventArgs e)
        {
            string previousData = GetComponentData();
            if (radPush.IsChecked == true)
                Component.Type = SwitchType.Push;
            else if (radToggle.IsChecked == true)
                Component.Type = SwitchType.Toggle;
            else if (radAnalogue.IsChecked == true)
                Component.Type = SwitchType.Analogue;
            base.CallComponentUpdated(previousData);
        }
    }
}
