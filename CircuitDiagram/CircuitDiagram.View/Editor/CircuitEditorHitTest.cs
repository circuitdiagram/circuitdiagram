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
using System.Windows.Media;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.View.Editor
{
    static class CircuitEditorHitTest
    {
        public static ElementHitTestResult HitTest(Visual container, Point mousePosition)
        {
            ElementHitTestResult result = null;

            VisualTreeHelper.HitTest(container, target =>
            {
                if (target is ElementDrawingVisual)
                    return HitTestFilterBehavior.ContinueSkipChildren;

                if ((target as ResizeHandle)?.IsVisible == true)
                    return HitTestFilterBehavior.ContinueSkipChildren;

                return HitTestFilterBehavior.ContinueSkipSelf;
            },
            hitResult =>
            {
                result = new ElementHitTestResult
                {
                    Element = (hitResult.VisualHit as ElementDrawingVisual)?.CircuitElement,
                    ResizeHandle = hitResult.VisualHit as ResizeHandle
                };

                //ComponentInternalMousePos = new Point(mousePos.X - (result.VisualHit as CircuitElementDrawingVisual).Offset.X, mousePos.Y - (result.VisualHit as CircuitElementDrawingVisual).Offset.Y);

                //using (DrawingContext dc = m_selectedVisual.RenderOpen())
                //{
                //    Pen stroke = new Pen(Brushes.Gray, 1d);
                //    //stroke.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
                //    Rect rect = VisualTreeHelper.GetContentBounds(result.VisualHit as Visual);
                //    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), stroke, Rect.Inflate(rect, new Size(2, 2)));
                //}
                //m_selectedVisual.Offset = (result.VisualHit as CircuitElementDrawingVisual).Offset;
                //if (((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component).Description.CanResize)
                //    m_resizingComponent = (result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component;

                return HitTestResultBehavior.Stop;
            }, new PointHitTestParameters(mousePosition));

            return result;
        }

        public class ElementHitTestResult
        {
            public PositionalComponent Element { get; set; }
            public ResizeHandle ResizeHandle { get; set; }
        }
    }
}
