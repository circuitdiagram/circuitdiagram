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
using System.Windows.Input;
using System.Windows.Media;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Connections;
using CircuitDiagram.View.Editor;
using CircuitDiagram.View.Editor.Operations;
using CircuitDiagram.View.Services;
using log4net;
using ResizeHandle = CircuitDiagram.View.Editor.Operations.ResizeHandle;
using Vector = System.Windows.Vector;

namespace CircuitDiagram.View.Controls
{
    class CircuitEditor : CircuitEditorDrawing, IEditorOperationalContext
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CircuitEditor));

        public static readonly DependencyProperty PlaceTypeProperty = DependencyProperty.Register(
            "PlaceType", typeof(IComponentTypeIdentifier), typeof(CircuitEditor), new PropertyMetadata(default(IComponentTypeIdentifier)));

        //private readonly MoveOperation moveOperation = new MoveOperation();

        private readonly HashSet<IPositionalElement> highlighted = new HashSet<IPositionalElement>();
        //private readonly HashSet<IPositionalElement> activeElements = new HashSet<IPositionalElement>();

        private IList<IEditorOperation> operations = new IEditorOperation[0];

        private IEditorOperation currentOperation;

        protected override void OnDescriptionLookupChanged()
        {
            base.OnDescriptionLookupChanged();

            operations = new IEditorOperation[]
            {
                new PlaceOperation(DescriptionLookup),
                new ResizeOperation(),
                new MoveOperation(),
                new SingleSelectOperation(),
            };
        }

        ICollection<IPositionalElement> IEditorOperationalContext.SelectedElements => SelectedElements;

        void IEditorOperationalContext.AddElement(IPositionalElement element)
        {
            Circuit.Elements.Add(element);
            RenderCircuit();
        }

        void IEditorOperationalContext.UpdateElementPosition(IPositionalElement element)
        {
            ElementVisuals[element].Offset = element.Layout.Location.ToWinVector();
            HighlightBoxVisuals[element].Offset = element.Layout.Location.ToWinVector();
            RenderConnections();
        }

        public bool TakeExclusiveOperation(IEditorOperation operation)
        {
            return currentOperation == null && (currentOperation = operation) != null;
        }

        public void ReleaseExclusiveOperation()
        {
            currentOperation = null;
        }

        public bool IsExclusiveOperation(IEditorOperation operation)
        {
            return ReferenceEquals(currentOperation, operation);
        }

        void IEditorOperationalContext.ReRenderElement(IPositionalElement element)
        {
            ElementVisuals[element].UpdateVisual();
            HighlightBoxVisuals[element].UpdateVisual();
            RenderConnections();
        }

        //private MouseAction currentAction = MouseAction.None;
        //private bool isPerformingAction = false;
        //private IEditorOperation currentOperation;


        //enum MouseAction
        //{
        //    None,
        //    Move,
        //    ResizeFirst,
        //    ResizeSecond,
        //    SelectMany,
        //    Place
        //}


        public IComponentTypeIdentifier PlaceType
        {
            get { return (IComponentTypeIdentifier)GetValue(PlaceTypeProperty); }
            set { SetValue(PlaceTypeProperty, value); }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var position = e.GetPosition(this);
            var result = CircuitEditorHitTest.HitTest(this, position);
            var hitTest = ToEditorHitTest(result);

            bool operationPerformed = false;
            foreach (var operation in GetActiveOperations())
                operationPerformed |= operation.OnMouseMove(position, hitTest, this);

            if (result?.Element != null)
            {
                HighlightElement(result.Element);

                result = CircuitEditorHitTest.HitTest(this, position);
                hitTest = ToEditorHitTest(result);

                if (hitTest.ResizeHandle != null)
                {
                    foreach (var operation in GetActiveOperations())
                        operation.OnMouseMove(position, hitTest, this);
                }
            }

            if (!operationPerformed && hitTest.Element == null && hitTest.ResizeHandle == null)
            {
                Cursor = Cursors.Arrow;
                HighlightOnlySelectedElements();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var position = e.GetPosition(this);
            var result = CircuitEditorHitTest.HitTest(this, position);
            var hitTest = ToEditorHitTest(result);

            foreach (var operation in GetActiveOperations())
                operation.OnMouseDown(position, hitTest, this);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            var position = e.GetPosition(this);
            var result = CircuitEditorHitTest.HitTest(this, position);
            var hitTest = ToEditorHitTest(result);

            foreach (var operation in GetActiveOperations())
                operation.OnMouseUp(position, hitTest, this);

            HighlightOnlySelectedElements();
        }

        private IEnumerable<IEditorOperation> GetActiveOperations()
        {
            return operations.Where(x => x.IsActive(this));
        }

        private EditorOperationHitTest ToEditorHitTest(CircuitEditorHitTest.ElementHitTestResult result)
        {
            return new EditorOperationHitTest
            {
                Element = result?.Element,
                ResizeHandle = ReferenceEquals(result?.ResizeHandle, FirstResizeVisual)
                    ? ResizeHandle.Begin
                    : ReferenceEquals(result?.ResizeHandle, SecondResizeVisual) ? ResizeHandle.End : (ResizeHandle?)null
            };
        }

        private void HighlightElement(IPositionalElement element)
        {
            highlighted.Add(element);
            HighlightBoxVisuals[element].Opacity = 1.0;

            PlaceResizeVisuals(element);
            FirstResizeVisual.IsVisible = true;
            SecondResizeVisual.IsVisible = true;
        }

        void PlaceResizeVisuals(IPositionalElement element)
        {
            FirstResizeVisual.Offset = element.Layout.Location.ToWinVector();
            SecondResizeVisual.Offset = Vector.Add(element.Layout.Location.ToWinVector(),
                                                   new Vector(element.Layout.Orientation == Orientation.Horizontal ? element.Layout.Size : 0.0,
                                                              element.Layout.Orientation == Orientation.Vertical ? element.Layout.Size : 0.0));
        }

        private void HighlightOnlySelectedElements()
        {
            Cursor = Cursors.Arrow;

            FirstResizeVisual.IsVisible = false;
            SecondResizeVisual.IsVisible = false;

            foreach (var element in HighlightBoxVisuals.Where(x => !SelectedElements.Contains(x.Key)).Select(x => x.Value))
                element.Opacity = 0.0;

            highlighted.Clear();
        }
    }
}
