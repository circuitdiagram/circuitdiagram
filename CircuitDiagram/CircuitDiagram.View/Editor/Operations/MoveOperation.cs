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
using System.Windows.Input;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;

namespace CircuitDiagram.View.Editor.Operations
{
    class MoveOperation : IEditorOperation
    {
        private Point initialMousePosition;
        private ICollection<IPositionalElement> elements;
        private IDictionary<IPositionalElement, Primitives.Point> initialLocations;

        public MoveOperation()
        {
            elements = new IPositionalElement[0];
        }

        public bool IsActive(IEditorOperationalContext context)
        {
            return context.PlaceType?.Type == null;
        }

        public void Abort()
        {
            elements = new IPositionalElement[0];
        }

        public void OnMouseDown(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            // 1. Move selected elements if the mouse is over one of them
            if (context.SelectedElements.Contains(hitTest.Element))
                elements = context.SelectedElements.ToArray();

            // 2. Move element the mouse is over
            else if (hitTest.Element != null)
                elements = new[] {hitTest.Element};

            if (!elements.Any())
                return;

            initialMousePosition = mousePosition;
            initialLocations = elements.ToDictionary(x => x, x => x.Layout.Location);
        }

        public bool OnMouseMove(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            if (hitTest.Element != null && hitTest.ResizeHandle == null)
                context.Cursor = Cursors.SizeAll;

            if (!elements.Any())
                return false;

            var offset = new Primitives.Vector(mousePosition.X - initialMousePosition.X, mousePosition.Y - initialMousePosition.Y);
            
            foreach (var element in elements)
            {
                var newLocation = initialLocations[element].Add(offset).SnapToGrid(context.GridSize);
                element.Layout.Location = newLocation;

                context.UpdateElementPosition(element);
            }

            return true;
        }

        public void OnMouseUp(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            Abort();
            context.ReleaseExclusiveOperation();
        }
    }
}
