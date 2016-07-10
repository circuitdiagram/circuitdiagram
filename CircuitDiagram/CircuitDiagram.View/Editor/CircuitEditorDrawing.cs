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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Connections;
using CircuitDiagram.View.Controls;
using CircuitDiagram.View.Editor.Operations;

namespace CircuitDiagram.View.Editor
{
    class CircuitEditorDrawing : CircuitEditorVisual
    {
        private static readonly DependencyPropertyKey SelectedElementsPropertyKey = DependencyProperty.RegisterReadOnly(
            "SelectedElements", typeof(ObservableCollection<IPositionalElement>), typeof(CircuitEditorDrawing), new PropertyMetadata(default(ICollection<IPositionalElement>)));

        public static readonly DependencyProperty SelectedElementsProperty = SelectedElementsPropertyKey.DependencyProperty;

        private VisualiseCircuitConnections visualiseConnections;

        public CircuitEditorDrawing()
        {
            SelectedElements = new ObservableCollection<IPositionalElement>();
        }

        /// <summary>
        /// Gets the collection of elements that have either been clicked on to select them, or have
        /// been selected by dragging a selection box around them.
        /// </summary>
        public ObservableCollection<IPositionalElement> SelectedElements
        {
            get { return (ObservableCollection<IPositionalElement>)GetValue(SelectedElementsProperty); }
            private set { SetValue(SelectedElementsPropertyKey, value); }
        }

        

        private void RenderStaticElements()
        {
            if (Circuit != null)
            {
                Width = Circuit.Size.Width;
                Height = Circuit.Size.Height;
            }
            else
            {
                Width = 0.0;
                Height = 0.0;
            }

            RenderBackground();
        }

        private void RenderBackground()
        {
            using (DrawingContext dc = BackgroundVisual.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(0.0);
                guidelines.GuidelinesY.Add(0.0);
                dc.PushGuidelineSet(guidelines);

                dc.DrawRectangle(Brushes.White, null, new Rect(new Size(Width, Height)));

                if (IsGridVisible)
                {
                    for (double x = GridSize; x < Width; x += GridSize)
                    {
                        Pen pen = new Pen(Brushes.LightBlue, 1.0d);
                        if (x % (5 * GridSize) == 0)
                            pen = new Pen(Brushes.LightGray, 1.5d);
                        dc.DrawLine(pen, new Point(x, 0), new Point(x, Height));
                    }
                    for (double y = GridSize; y < Height; y += GridSize)
                    {
                        Pen pen = new Pen(Brushes.LightBlue, 1.0d);
                        if (y % (5 * GridSize) == 0)
                            pen = new Pen(Brushes.LightGray, 1.5d);
                        dc.DrawLine(pen, new Point(0, y), new Point(Width, y));
                    }
                }

                dc.Pop();
            }
        }

        protected void RenderCircuit()
        {
            if (DescriptionLookup == null)
                return;

            ClearCircuit();

            if (Circuit == null)
                return;

            // Add ElementVisuals
            foreach (var element in Circuit.PositionalElements)
            {
                var renderer = new CircuitRenderer(DescriptionLookup);
                var visual = new ElementDrawingVisual(renderer, element);
                ElementVisuals.Add(element, visual);
                AddChild(visual);
            }

            // Add HighlightVisuals
            foreach (var elementVisual in ElementVisuals)
            {
                var visual = new ElementHighlightBoxVisual(elementVisual.Value)
                {
                    Opacity = 0.0
                };
                HighlightBoxVisuals.Add(elementVisual.Key, visual);
                AddChild(visual);
            }
            
            RenderConnections();
        }

        protected void RenderConnections()
        {
            var layoutOptions = new LayoutOptions
            {
                GridSize = GridSize
            };
            var connections = visualiseConnections.VisualiseConnections(Circuit.PositionalComponents, layoutOptions);
            ConnectionsVisual.ConnectionPoints = connections;
        }

        private void ClearCircuit()
        {
            foreach (var visual in ElementVisuals)
            {
                RemoveVisualChild(visual.Value);
                RemoveLogicalChild(visual.Value);
            }
            ElementVisuals.Clear();

            foreach (var visual in HighlightBoxVisuals)
            {
                RemoveVisualChild(visual.Value);
                RemoveLogicalChild(visual.Value);
            }
            HighlightBoxVisuals.Clear();
        }

        protected override void OnCircuitChanged()
        {
            base.OnCircuitChanged();

            RenderStaticElements();
            RenderCircuit();
        }

        protected override void OnDescriptionLookupChanged()
        {
            base.OnDescriptionLookupChanged();

            visualiseConnections = new VisualiseCircuitConnections(DescriptionLookup,
                                                                   new ConnectionPositioner(),
                                                                   new ConnectionVisualiser());
            RenderCircuit();
        }
    }
}
