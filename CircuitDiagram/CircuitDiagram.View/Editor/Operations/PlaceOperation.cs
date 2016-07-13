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
using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.View.Services;
using Point = System.Windows.Point;

namespace CircuitDiagram.View.Editor.Operations
{
    class PlaceOperation : IEditorOperation
    {
        private readonly IComponentDescriptionLookup descriptionLookup;

        private Point initialMousePosition;
        private PositionalComponent element;

        public PlaceOperation(IComponentDescriptionLookup descriptionLookup)
        {
            this.descriptionLookup = descriptionLookup;
        }

        public bool IsActive(IEditorOperationalContext context)
        {
            return context.PlaceType?.Type != null;
        }

        public void Abort()
        {
            element = null;
        }

        public void OnMouseDown(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            initialMousePosition = mousePosition;
            element = new PositionalComponent(context.PlaceType.Type, context.PlaceType.Configuration, initialMousePosition.ToCdPoint());
            SizeComponent(element, initialMousePosition.ToCdPoint(), initialMousePosition.ToCdPoint(), context.GridSize);
            SetDefaultProperties(element);
            SetConfigurationProperties(element);
            context.AddElement(element);
        }

        public bool OnMouseMove(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            if (element == null)
                return false;

            SizeComponent(element, initialMousePosition.ToCdPoint(), mousePosition.ToCdPoint(), context.GridSize);

            context.UpdateElementPosition(element);
            context.ReRenderElement(element);

            return true;
        }

        public void OnMouseUp(Point mousePosition, EditorOperationHitTest hitTest, IEditorOperationalContext context)
        {
            // Finish
            Abort();
        }

        protected void SizeComponent(PositionalComponent component, Primitives.Point start, Primitives.Point end, double gridSize)
        {
            // reverse points if necessary
            Primitives.Point newStart = start;
            Primitives.Point newEnd = end;
            bool switched = false;
            if (start.X < end.X)
            {
                newStart = end;
                newEnd = start;
                switched = true;
            }

            if (true) // snap to grid
            {
                if (Math.IEEERemainder(newStart.X, 20d) != 0)
                    newStart = newStart.WithNewX(newStart.SnapToGrid(gridSize).X);
                if (Math.IEEERemainder(newStart.Y, 20d) != 0)
                    newStart = newStart.WithNewY(newStart.SnapToGrid(gridSize).Y);
                if (Math.IEEERemainder(newEnd.X, 20d) != 0)
                    newEnd = newEnd.WithNewX(newEnd.SnapToGrid(gridSize).X);
                if (Math.IEEERemainder(newEnd.Y, 20d) != 0)
                    newEnd = newEnd.WithNewY(newEnd.SnapToGrid(gridSize).Y);
            }
            if (true) // snap to horizontal or vertical
            {
                double height = Math.Max(newStart.Y, newEnd.Y) - Math.Min(newStart.Y, newEnd.Y);
                double length = Math.Sqrt(Math.Pow(newEnd.X - newStart.X, 2d) + Math.Pow(newEnd.Y - newStart.Y, 2d));
                double bearing = Math.Acos(height / length) * (180 / Math.PI);

                if (bearing <= 45 && switched)
                    newStart = newStart.WithNewX(newEnd.X);
                else if (bearing <= 45 && !switched)
                    newEnd = newEnd.WithNewX(newStart.X);
                else if (bearing > 45 && switched)
                    newStart = newStart.WithNewY(newEnd.Y);
                else
                    newEnd = newEnd.WithNewY(newStart.Y);
            }

            if (newStart.X > newEnd.X || newStart.Y > newEnd.Y)
            {
                component.Layout.Location = new Primitives.Point(newEnd.X, newEnd.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Layout.Size = newStart.Y - newEnd.Y;
                    component.Layout.Orientation = Orientation.Vertical;
                }
                else
                {
                    component.Layout.Size = newStart.X - newEnd.X;
                    component.Layout.Orientation = Orientation.Horizontal;
                }
            }
            else
            {
                component.Layout.Location = new Primitives.Point(newStart.X, newStart.Y);
                if (newStart.X == newEnd.X)
                {
                    component.Layout.Size = newEnd.Y - newStart.Y;
                    component.Layout.Orientation = Orientation.Vertical;
                }
                else
                {
                    component.Layout.Size = newEnd.X - newStart.X;
                    component.Layout.Orientation = Orientation.Horizontal;
                }
            }

            var description = descriptionLookup.GetDescription(component.Type);

            //FlagOptions flagOptions = ApplyFlags(component);
            //if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && component.Layout.Orientation == Orientation.Vertical)
            //{
            //    component.Layout.Orientation = Orientation.Horizontal;
            //    component.Layout.Size = component.Layout.Description.MinSize;
            //}
            //else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Layout.Orientation == Orientation.Horizontal)
            //{
            //    component.Layout.Orientation = Orientation.Vertical;
            //    component.Layout.Size = component.Layout.Description.MinSize;
            //}

            double minAllowedSize = Math.Max(description.MinSize, gridSize);
            if (component.Layout.Size < minAllowedSize)
                component.Layout.Size = minAllowedSize;
        }

        private void SetDefaultProperties(Component component)
        {
            var description = descriptionLookup.GetDescription(component.Type);

            foreach (var property in description.Properties)
            {
                if (!component.Properties.Keys.Contains(property.SerializedName) ||
                    !((DependentDictionary<PropertyName, PropertyValue>)component.Properties).IsSetExplicitly(property.SerializedName))
                    component.Properties[property.SerializedName] = property.Default;
            }
        }

        private void SetConfigurationProperties(Component component)
        {
            if (component.Configuration == null)
                return;

            var description = descriptionLookup.GetDescription(component.Type);
            var configuration = description.Metadata.Configurations.FirstOrDefault(x => x.ImplementationName == component.Configuration.Implements);
            
            foreach (var property in configuration.Setters)
                component.Properties[property.Key] = property.Value;
        }
    }
}
