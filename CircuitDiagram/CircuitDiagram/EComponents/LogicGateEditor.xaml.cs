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
    using T = LogicGate;

    /// <summary>
    /// Interaction logic for ResistorEditor.xaml
    /// </summary>
    public partial class LogicGateEditor : ComponentEditor<T>
    {
        public LogicGateEditor(T component)
            :base(component)
        {
            InitializeComponent();
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            radLogicAND.IsChecked = false;
            radLogicOR.IsChecked = false;
            radLogicNAND.IsChecked = false;
            radLogicNOR.IsChecked = false;
            radLogicXOR.IsChecked = false;
            radLogicNOT.IsChecked = false;
            radLogicSchmitt.IsChecked = false;
            switch (Component.LogicType)
            {
                case LogicType.AND:
                    radLogicAND.IsChecked = true;
                    break;
                case LogicType.OR:
                    radLogicOR.IsChecked = true;
                    break;
                case LogicType.NAND:
                    radLogicNAND.IsChecked = true;
                    break;
                case LogicType.NOR:
                    radLogicNOR.IsChecked = true;
                    break;
                case LogicType.XOR:
                    radLogicXOR.IsChecked = true;
                    break;
                case LogicType.NOT:
                    radLogicNOT.IsChecked = true;
                    break;
                case LogicType.Schmitt:
                    radLogicSchmitt.IsChecked = true;
                    break;
            }
            IsLoadingComponent = false;
        }

        private void RadioChecked(object sender, EventArgs e)
        {
            string previousData = GetComponentData();
            if (radLogicAND.IsChecked == true)
                Component.LogicType = LogicType.AND;
            else if (radLogicOR.IsChecked == true)
                Component.LogicType = LogicType.OR;
            else if (radLogicNAND.IsChecked == true)
                Component.LogicType = LogicType.NAND;
            else if (radLogicNOR.IsChecked == true)
                Component.LogicType = LogicType.NOR;
            else if (radLogicXOR.IsChecked == true)
                Component.LogicType = LogicType.XOR;
            else if (radLogicNOT.IsChecked == true)
                Component.LogicType = LogicType.NOT;
            else if (radLogicSchmitt.IsChecked == true)
                Component.LogicType = LogicType.Schmitt;
            base.CallComponentUpdated(previousData);
        }
    }
}
