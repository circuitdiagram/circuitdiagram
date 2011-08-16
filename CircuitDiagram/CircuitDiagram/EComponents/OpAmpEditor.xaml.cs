// OpAmpEditor.xaml.cs
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
    using T = OpAmp;

    /// <summary>
    /// Interaction logic for OpAmpEditor.xaml
    /// </summary>
    public partial class OpAmpEditor : ComponentEditor<T>
    {
        public OpAmpEditor(T component)
            :base(component)
        {
            InitializeComponent();
        }

        public override void LoadComponent()
        {
            IsLoadingComponent = true;
            chbFlipPN.IsChecked = ((OpAmp)Component).FlipInputs;
            IsLoadingComponent = false;
        }

        private void FlipInputTerminals_Checked(object sender, EventArgs e)
        {
            string previousData = GetComponentData();
            Component.FlipInputs = chbFlipPN.IsChecked.Value;
            base.CallComponentUpdated(previousData);
        }
    }
}
