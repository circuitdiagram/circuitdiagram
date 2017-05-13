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
using System.Windows.Input;
using CircuitDiagram.Circuit;
using CircuitDiagram.Logging;
using CircuitDiagram.Primitives;
using Microsoft.Extensions.Logging;
using Point = System.Windows.Point;

namespace CircuitDiagram.View.Editor.Operations
{
    class ResizeOperation : IEditorOperation
    {
        private static readonly ILogger Log = LogManager.GetLogger<ResizeOperation>();

        private IPositionalElement element;
        private bool isResizing;
        private Point initialMousePosition;
        private ResizeHandle mode;
        private Primitives.Point initialLocation;
        private double initialSize;

        public bool IsActive(IEditorOperationalContext context)
        {
            return context.PlaceType?.Type == null;
        }

        public void Abort()
        {
            isResizing = false;
            element = null;
        }

        public void OnMouseDown(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            if (hitTest.ResizeHandle == null || element == null)
            {
                // Not resizing
                element = null;
                return;
            }

            Log.LogInformation($"Starting resize on {hitTest.ResizeHandle.Value} handle");

            initialMousePosition = mousePosition;
            initialLocation = element.Layout.Location;
            initialSize = element.Layout.Size;
            mode = hitTest.ResizeHandle.Value;
            isResizing = true;
        }

        public bool OnMouseMove(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            if (!isResizing)
            {
                if (hitTest.Element == null && hitTest.ResizeHandle == null)
                {
                    // Nothing under the cursor

                    if (element != null)
                    {
                        Log.LogInformation("Nothing prepared for resize");
                        element = null;
                    }
                    return false;
                }

                if (element == null && hitTest.Element != null)
                {
                    Log.LogInformation($"{hitTest.Element} prepared for resize");

                    // Mouse is over an element - resize it later
                    element = hitTest.Element;
                    return false;
                }

                if (hitTest.ResizeHandle != null)
                    context.Cursor = Cursors.SizeNS;

                return false;
            }

            // Resizing in progress

            var offset = new Vector(mousePosition.X - initialMousePosition.X, mousePosition.Y - initialMousePosition.Y);

            double diff = element.Layout.Orientation == Orientation.Horizontal ? offset.X : offset.Y;

            if (mode == ResizeHandle.Begin)
                diff = -diff;

            diff = SnapToGrid(diff, context.GridSize);

            element.Layout.Size = initialSize + diff;

            if (mode == ResizeHandle.Begin && element.Layout.Orientation == Orientation.Horizontal)
                element.Layout.Location = element.Layout.Location.WithNewX(initialLocation.X - diff);
            else if (mode == ResizeHandle.Begin)
                element.Layout.Location = element.Layout.Location.WithNewY(initialLocation.Y - diff);

            context.UpdateElementPosition(element);
            context.ReRenderElement(element);

            return true;
        }

        public void OnMouseUp(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            // Finish resizing if it's in progress
            Abort();
        }

        private double SnapToGrid(double val, double gridSize)
        {
            return Math.Round(val / gridSize) * gridSize;
        }
    }
}
