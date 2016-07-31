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
    class SingleSelectOperation : IEditorOperation
    {
        private PositionalComponent selectionElement;
        private Primitives.Point? initialLocation;
        private double? initialSize;

        public bool IsActive(IEditorOperationalContext context)
        {
            return context.PlaceType?.Type == null;
        }

        public void Abort()
        {
            // Do nothing
        }

        public void OnMouseDown(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            selectionElement = hitTest?.Element;
            initialLocation = selectionElement?.Layout.Location;
            initialSize = selectionElement?.Layout.Size;
        }

        public bool OnMouseMove(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            // Do nothing
            return false;
        }

        public void OnMouseUp(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            if (!context.IsExclusiveOperation(null))
                return; // Something else is being performed

            if (selectionElement == hitTest?.Element && !Keyboard.IsKeyDown(Key.LeftCtrl))
                context.SelectedElements.Clear();

            if (!MatchesSelection(hitTest?.Element))
                return;

            if (!context.SelectedElements.Contains(selectionElement))
                context.SelectedElements.Add(selectionElement);
            else
                context.SelectedElements.Remove(selectionElement);
        }

        private bool MatchesSelection(PositionalComponent element)
        {
            return element != null && element == selectionElement &&
                   element.Layout.Location == initialLocation.Value &&
                   element.Layout.Size == initialSize.Value;
        }
    }
}
