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
using System.Threading.Tasks;
using System.Windows;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;

namespace CircuitDiagram.View.Editor
{
    class CircuitEditorBase : FrameworkElement
    {
        public static readonly DependencyProperty CircuitProperty = DependencyProperty.Register(
            "Circuit", typeof(CircuitDocument), typeof(CircuitEditorBase), new PropertyMetadata(default(CircuitDocument), CircuitChanged));

        public static readonly DependencyProperty IsGridVisibleProperty = DependencyProperty.Register(
            "IsGridVisible", typeof(bool), typeof(CircuitEditorBase), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(
            "GridSize", typeof(double), typeof(CircuitEditorBase), new PropertyMetadata(10.0));

        public static readonly DependencyProperty DescriptionLookupProperty = DependencyProperty.Register(
            "DescriptionLookup", typeof(IComponentDescriptionLookup), typeof(CircuitEditorBase), new PropertyMetadata(default(IComponentDescriptionLookup), DescriptionLookupChanged));

        public CircuitDocument Circuit
        {
            get { return (CircuitDocument)GetValue(CircuitProperty); }
            set { SetValue(CircuitProperty, value); }
        }

        public bool IsGridVisible
        {
            get { return (bool)GetValue(IsGridVisibleProperty); }
            set { SetValue(IsGridVisibleProperty, value); }
        }

        public double GridSize
        {
            get { return (double)GetValue(GridSizeProperty); }
            set { SetValue(GridSizeProperty, value); }
        }

        public IComponentDescriptionLookup DescriptionLookup
        {
            get { return (IComponentDescriptionLookup)GetValue(DescriptionLookupProperty); }
            set { SetValue(DescriptionLookupProperty, value); }
        }

        protected virtual void OnCircuitChanged()
        {
        }

        protected virtual void OnDescriptionLookupChanged()
        {
        }

        private static void CircuitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (CircuitEditorVisual)d;
            instance.OnCircuitChanged();
        }

        private static void DescriptionLookupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (CircuitEditorVisual)d;
            instance.OnDescriptionLookupChanged();
        }
    }
}
