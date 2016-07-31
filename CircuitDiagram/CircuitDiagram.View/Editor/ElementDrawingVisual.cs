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
using System.Windows.Media;
using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using log4net;
using Point = System.Windows.Point;

namespace CircuitDiagram.View.Editor
{
    class ElementDrawingVisual : DrawingVisual
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ElementDrawingVisual));

        private readonly ICircuitRenderer renderer;
        private bool isHighlighted;
        private Brush resizeHandleBrush;

        public ElementDrawingVisual(ICircuitRenderer renderer, PositionalComponent circuitElement)
        {
            this.renderer = renderer;
            CircuitElement = circuitElement;
            ResizeHandleBrush = Brushes.MediumOrchid;
            Offset = circuitElement.Layout.Location.ToWinVector();
            UpdateVisual();
        }

        public PositionalComponent CircuitElement { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a highlight box and resize controls should be shown.
        /// </summary>
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set
            {
                isHighlighted = value;
                OnIsHighlightedChanged();
            }
        }

        public Brush ResizeHandleBrush
        {
            get { return resizeHandleBrush; }
            set
            {
                resizeHandleBrush = value;
                if (IsHighlighted)
                    UpdateVisual();
            }
        }

        private void OnIsHighlightedChanged()
        {
            if (IsHighlighted)
                Log.InfoFormat("{0} is highlighted", CircuitElement);

            UpdateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // Hit if within bounding box
            var pt = hitTestParameters.HitPoint;
            return new PointHitTestResult(this, pt);
        }

        public void UpdateVisual()
        {
            using (DrawingContext dc = RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(Offset.X);
                guidelines.GuidelinesY.Add(Offset.Y);
                dc.PushGuidelineSet(guidelines);

                var dr = new DrawingRenderer(dc);
                dr.Begin();

                renderer.RenderComponent((PositionalComponent)CircuitElement, dr);

                if (IsHighlighted)
                    DrawResizeHandles(dc);
                
                dr.End();
            }
        }

        private void DrawResizeHandles(DrawingContext dc)
        {
            dc.DrawEllipse(ResizeHandleBrush, null, new Point(0, 0), 3.0, 3.0);

            Point endPoint = new Point(CircuitElement.Layout.Orientation == Orientation.Horizontal ? CircuitElement.Layout.Size : 0.0,
                                       CircuitElement.Layout.Orientation == Orientation.Vertical ? CircuitElement.Layout.Size : 0.0);
            dc.DrawEllipse(ResizeHandleBrush, null, endPoint, 3.0, 3.0);
        }
    }
}
