// CircuitDisplay.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using CircuitDiagram;
using System.Windows.Media;
using CircuitDiagram.Components;
using CircuitDiagram.Elements;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace CircuitDiagram
{
    public class CircuitDisplay : FrameworkElement
    {
        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(Selector.SelectionChangedEvent, value); }
            remove { RemoveHandler(Selector.SelectionChangedEvent, value); }
        }

        public string NewComponentData { get; set; }
        public UndoManager UndoManager { get; set; }

        private Dictionary<Component, string> m_undoManagerBeforeData;
        private Dictionary<ICircuitElement, CircuitElementDrawingVisual> m_elementVisuals;

        private DrawingVisual m_backgroundVisual;
        private DrawingVisual m_selectedVisual;
        private DrawingVisual m_connectionsVisual;
        private DrawingVisual m_resizeVisual;
        private Component m_tempComponent;
        private Component m_resizingComponent;
        private CircuitDocument m_document;
        Point ComponentInternalMousePos;
        List<Component> m_selectedComponents { get; set; }
        bool m_placingComponent = false;
        Point m_mouseDownPos;

        public bool ShowConnectionPoints { get; set; }

        public bool ShowGrid { get; set; }

        public ReadOnlyCollection<Component> SelectedComponents { get { return m_selectedComponents.AsReadOnly(); } }

        public CircuitDocument Document
        {
            get { return m_document; }
            set
            {
                if (Document != null)
                    foreach (CircuitDiagram.Elements.ICircuitElement element in Document.Elements)
                    {
                        RemoveLogicalChild(m_elementVisuals[element]);
                        RemoveVisualChild(m_elementVisuals[element]);
                    }
                m_elementVisuals.Clear();
                m_document = value;
                DocumentChanged();
                m_document.Elements.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Components_CollectionChanged);
            }
        }

        void Components_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Component item in e.NewItems)
                {
                    m_elementVisuals.Add(item, new CircuitElementDrawingVisual(item));

                    AddVisualChild(m_elementVisuals[item]);
                    AddLogicalChild(m_elementVisuals[item]);
                    m_elementVisuals[item].UpdateVisual();
                    item.Updated += new EventHandler(Component_Updated);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Component item in e.OldItems)
                {
                    RemoveVisualChild(m_elementVisuals[item]);
                    RemoveLogicalChild(m_elementVisuals[item]);
                    m_elementVisuals.Remove(item);
                    item.Updated -= new EventHandler(Component_Updated);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                DocumentChanged();
        }

        void Component_Updated(object sender, EventArgs e)
        {
            if (m_selectedComponents.Count == 1 && sender == m_selectedComponents[0])
            {
                m_elementVisuals[m_selectedComponents[0]].UpdateVisual();
                if (m_elementVisuals[m_selectedComponents[0]].ContentBounds != Rect.Empty)
                {
                    using (DrawingContext dc = m_selectedVisual.RenderOpen())
                    {
                        Pen stroke = new Pen(Brushes.Gray, 1d);
                        Rect rect = VisualTreeHelper.GetContentBounds(m_elementVisuals[sender as ICircuitElement]);
                        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), stroke, Rect.Inflate(rect, new Size(2, 2)));
                    }
                }
                m_selectedVisual.Offset = m_selectedComponents[0].Location;
            }
        }

        public void RedrawComponent(Component component)
        {
            if (m_elementVisuals.ContainsKey(component))
                m_elementVisuals[component].UpdateVisual();
            if (m_selectedComponents.Contains(component))
            {
                if (m_elementVisuals[component].ContentBounds != Rect.Empty)
                {
                    using (DrawingContext dc = m_selectedVisual.RenderOpen())
                    {
                        Pen stroke = new Pen(Brushes.Gray, 1d);
                        Rect rect = VisualTreeHelper.GetContentBounds(m_elementVisuals[component as ICircuitElement]);
                        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), stroke, Rect.Inflate(rect, new Size(2, 2)));
                    }
                }
                m_selectedVisual.Offset = component.Location;
            }
        }

        public void DocumentSizeChanged()
        {
            this.Width = Document.Size.Width;
            this.Height = Document.Size.Height;

            using (DrawingContext dc = m_backgroundVisual.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(0d);
                guidelines.GuidelinesY.Add(0d);
                dc.PushGuidelineSet(guidelines);

                dc.DrawRectangle(Brushes.White, null, new Rect(Document.Size));

                dc.Pop();
            }

            DrawConnections();
        }

        void DocumentChanged()
        {
            if (Document == null)
                return;

            m_resizing = ComponentResizeMode.None;
            m_resizingComponent = null;

            this.Width = Document.Size.Width;
            this.Height = Document.Size.Height;

            RenderBackground();

            foreach (CircuitDiagram.Elements.ICircuitElement element in Document.Elements)
            {
                m_elementVisuals.Add(element, new CircuitElementDrawingVisual(element));

                AddVisualChild(m_elementVisuals[element]);
                AddLogicalChild(m_elementVisuals[element]);
                m_elementVisuals[element].UpdateVisual();
                element.Updated += new EventHandler(Component_Updated);
            }
        }

        public void SetSelectedComponents(IEnumerable<Component> selected)
        {
            List<Component> removedItems = new List<Component>(m_selectedComponents);
            m_selectedComponents.Clear();
            m_selectedComponents.AddRange(selected);

            m_selectedVisual.Offset = new Vector(0, 0);
            using (DrawingContext dc = m_selectedVisual.RenderOpen())
            {
                enclosingRect = Rect.Empty;

                foreach (Component component in m_selectedComponents)
                {
                    Rect rect = VisualTreeHelper.GetContentBounds(m_elementVisuals[component]);
                    dc.PushTransform(new TranslateTransform((m_elementVisuals[component]).Offset.X, (m_elementVisuals[component]).Offset.Y));
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), null, rect);
                    dc.Pop();

                    if (enclosingRect.IsEmpty)
                    {
                        rect.Offset((m_elementVisuals[component]).Offset.X, (m_elementVisuals[component]).Offset.Y);
                        enclosingRect = rect;
                    }
                    else
                    {
                        rect.Offset((m_elementVisuals[component]).Offset.X, (m_elementVisuals[component]).Offset.Y);
                        enclosingRect.Union(rect);
                    }
                }

                dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 1d), enclosingRect);
            }

            RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, removedItems, m_selectedComponents));
        }

        public void RenderBackground()
        {
            using (DrawingContext dc = m_backgroundVisual.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(0d);
                guidelines.GuidelinesY.Add(0d);
                dc.PushGuidelineSet(guidelines);

                dc.DrawRectangle(Brushes.White, null, new Rect(Document.Size));

                if (ShowGrid)
                {
                    for (double x = ComponentHelper.GridSize; x < this.Width; x += ComponentHelper.GridSize)
                    {
                        Pen pen = new Pen(Brushes.LightBlue, 1.0d);
                        if (x % (5 * ComponentHelper.GridSize) == 0)
                            pen = new Pen(Brushes.LightGray, 1.5d);
                        dc.DrawLine(pen, new Point(x, 0), new Point(x, this.Height));
                    }
                    for (double y = ComponentHelper.GridSize; y < this.Height; y += ComponentHelper.GridSize)
                    {
                        Pen pen = new Pen(Brushes.LightBlue, 1.0d);
                        if (y % (5 * ComponentHelper.GridSize) == 0)
                            pen = new Pen(Brushes.LightGray, 1.5d);
                        dc.DrawLine(pen, new Point(0, y), new Point(this.Width, y));
                    }
                }

                dc.Pop();
            }
        }

        static CircuitDisplay()
        {
            Selector.SelectionChangedEvent.AddOwner(typeof(CircuitDisplay));
        }

        public CircuitDisplay()
        {
            m_backgroundVisual = new DrawingVisual();
            m_selectedVisual = new DrawingVisual();
            m_connectionsVisual = new DrawingVisual();
            m_resizeVisual = new DrawingVisual();
            AddVisualChild(m_backgroundVisual);
            AddLogicalChild(m_backgroundVisual);
            AddVisualChild(m_selectedVisual);
            AddLogicalChild(m_selectedVisual);
            AddVisualChild(m_connectionsVisual);
            AddLogicalChild(m_connectionsVisual);
            AddVisualChild(m_resizeVisual);
            AddLogicalChild(m_resizeVisual);
            this.SnapsToDevicePixels = true;
            m_selectedComponents = new List<Component>();
            m_undoManagerBeforeData = new Dictionary<Component, string>();
            m_elementVisuals = new Dictionary<ICircuitElement, CircuitElementDrawingVisual>();
        }

        #region Commands
        public void DeleteComponentCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (m_selectedComponents.Count > 0)
            {
                UndoAction undoAction = new UndoAction(UndoCommand.DeleteComponents, "delete", m_selectedComponents.ToArray());
                UndoManager.AddAction(undoAction);

                foreach (Component component in m_selectedComponents)
                {
                    Document.Elements.Remove(component);
                }
                m_selectedComponents.Clear();
                foreach (Component component in Document.Components)
                    component.DisconnectConnections();
                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);
                DrawConnections();

                // Clear selection box
                using (DrawingContext dc = m_selectedVisual.RenderOpen())
                {
                }
                // Clear resize visual
                using (DrawingContext dc = m_resizeVisual.RenderOpen())
                {
                }

            }
            else if (m_resizingComponent != null)
            {
                UndoAction undoAction = new UndoAction(UndoCommand.DeleteComponents, "delete", new Component[] { m_resizingComponent });
                UndoManager.AddAction(undoAction);

                Document.Elements.Remove(m_resizingComponent);
                m_resizingComponent = null;
                foreach (Component component in Document.Components)
                    component.DisconnectConnections();
                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);
                DrawConnections();

                // Clear selection box
                using (DrawingContext dc = m_selectedVisual.RenderOpen())
                {
                }
                // Clear resize visual
                using (DrawingContext dc = m_resizeVisual.RenderOpen())
                {
                }
            }
        }

        public void DeleteComponentCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (m_selectedComponents.Count > 0 || m_resizingComponent != null);
        }

        public void FlipComponentCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (m_selectedComponents.Count == 1 && m_selectedComponents[0].Description.CanFlip)
            {
                m_selectedComponents[0].IsFlipped = !m_selectedComponents[0].IsFlipped;
                m_elementVisuals[m_selectedComponents[0]].UpdateVisual();
                m_selectedComponents[0].ResetConnections();
                m_selectedComponents[0].ApplyConnections(Document);

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Flip component", new Component[] { m_selectedComponents[0] });
                undoAction.AddData("before", m_undoManagerBeforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_selectedComponents[0], m_selectedComponents[0].SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();
            }
            else if (m_resizingComponent != null && m_resizingComponent.Description.CanFlip)
            {
                Dictionary<Component, string> beforeData = new Dictionary<Component, string>();
                beforeData.Add(m_resizingComponent, m_resizingComponent.SerializeToString());

                m_resizingComponent.IsFlipped = !m_resizingComponent.IsFlipped;
                m_elementVisuals[m_resizingComponent].UpdateVisual();
                m_resizingComponent.ResetConnections();
                m_resizingComponent.ApplyConnections(Document);

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Flip component", new Component[] { m_resizingComponent });
                undoAction.AddData("before", beforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_resizingComponent, m_resizingComponent.SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();
            }
        }

        public void FlipComponentCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (m_selectedComponents.Count == 1 && m_selectedComponents[0].Description.CanFlip) || (m_resizingComponent != null && m_resizingComponent.Description.CanFlip);
        }

        public void RotateComponentCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (m_selectedComponents.Count != 1 && m_resizingComponent == null)
            {
                e.CanExecute = false;
                return;
            }

            if (m_selectedComponents.Count == 1)
            {
                foreach (Conditional<FlagOptions> flags in m_selectedComponents[0].Description.Flags)
                {
                    if (flags.Conditions.ConditionsAreMet(m_selectedComponents[0]))
                    {
                        if ((flags.Value & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly || (flags.Value & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly)
                        {
                            e.CanExecute = false;
                            return;
                        }
                    }
                }
            }

            if (m_resizingComponent != null)
            {
                foreach (Conditional<FlagOptions> flags in m_resizingComponent.Description.Flags)
                {
                    if (flags.Conditions.ConditionsAreMet(m_resizingComponent))
                    {
                        if ((flags.Value & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly || (flags.Value & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly)
                        {
                            e.CanExecute = false;
                            return;
                        }
                    }
                }
            }

            e.CanExecute = true;
        }

        public void RotateComponentCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (m_selectedComponents.Count == 1)
            {
                m_selectedComponents[0].Orientation = m_selectedComponents[0].Orientation.Reverse();
                m_selectedComponents[0].ResetConnections();
                RedrawComponent(m_selectedComponents[0]);
                foreach (Component component in Document.Components)
                    component.DisconnectConnections();
                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);
                DrawConnections();

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Rotate component", new Component[] { m_selectedComponents[0] });
                undoAction.AddData("before", m_undoManagerBeforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_selectedComponents[0], m_selectedComponents[0].SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();
            }
            else if (m_resizingComponent != null)
            {
                Dictionary<Component, string> beforeData = new Dictionary<Component, string>();
                beforeData.Add(m_resizingComponent, m_resizingComponent.SerializeToString());

                m_resizingComponent.Orientation = m_resizingComponent.Orientation.Reverse();
                m_resizingComponent.ResetConnections();
                RedrawComponent(m_resizingComponent);
                foreach (Component component in Document.Components)
                    component.DisconnectConnections();
                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);
                DrawConnections();

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Rotate component", new Component[] { m_resizingComponent });
                undoAction.AddData("before", beforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_resizingComponent, m_resizingComponent.SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();
            }
        }
        #endregion

        bool m_selectionBox = false;
        bool m_movingMouse = false;
        ComponentResizeMode m_resizing = ComponentResizeMode.None;
        Point m_resizeComponentOriginalStartEnd;
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Point mousePos = e.GetPosition(this);
            m_mouseDownPos = mousePos;

            Rect resizingRect1 = Rect.Empty;
            Rect resizingRect2 = Rect.Empty;
            if (m_resizingComponent != null && m_resizingComponent.Orientation == Orientation.Horizontal && m_resizingComponent.Description.CanResize)
            {
                // Resizing a horizontal component
                resizingRect1 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.X - 2d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d);
                resizingRect2 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Right - 4d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d);
            }
            else if (m_resizingComponent != null && m_resizingComponent.Description.CanResize)
            {
                // Resizing a vertical component
                resizingRect1 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Y - 2d, 6d, 6d);
                resizingRect2 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Bottom - 4d, 6d, 6d);
            }

            if (NewComponentData == null && (resizingRect1.IntersectsWith(new Rect(mousePos, new Size(1, 1))) || resizingRect2.IntersectsWith(new Rect(mousePos, new Size(1, 1)))))
            {
                // Enter resizing mode
                
                m_undoManagerBeforeData[m_resizingComponent] = m_resizingComponent.SerializeToString();

                if (resizingRect1.IntersectsWith(new Rect(mousePos, new Size(1, 1))))
                {
                    if (m_resizingComponent.Orientation == Orientation.Horizontal)
                    {
                        m_resizing = ComponentResizeMode.Left;
                        m_resizeComponentOriginalStartEnd = new Point(m_resizingComponent.Location.X + m_resizingComponent.Size, m_resizingComponent.Location.Y);
                    }
                    else
                    {
                        m_resizing = ComponentResizeMode.Top;
                        m_resizeComponentOriginalStartEnd = new Point(m_resizingComponent.Location.X, m_resizingComponent.Location.Y + m_resizingComponent.Size);
                    }
                }
                else
                {
                    if (m_resizingComponent.Orientation == Orientation.Horizontal)
                    {
                        m_resizing = ComponentResizeMode.Right;
                        m_resizeComponentOriginalStartEnd = new Point(m_resizingComponent.Location.X, m_resizingComponent.Location.Y);
                    }
                    else
                    {
                        m_resizing = ComponentResizeMode.Bottom;
                        m_resizeComponentOriginalStartEnd = new Point(m_resizingComponent.Location.X, m_resizingComponent.Location.Y);
                    }
                }
            }
            else if (m_selectedComponents.Count == 0)
            {
                bool foundHit = false;

                if (NewComponentData == null)
                {
                    // Check if user is selecting a component

                    VisualTreeHelper.HitTest(this, new HitTestFilterCallback(delegate(DependencyObject testObject)
                    {
                        if (testObject is CircuitElementDrawingVisual)
                            return HitTestFilterBehavior.ContinueSkipChildren;
                        else
                            return HitTestFilterBehavior.ContinueSkipSelf;
                    }),
                    new HitTestResultCallback(delegate(HitTestResult result)
                    {
                        if (result.VisualHit is CircuitElementDrawingVisual)
                        {
                            m_selectedComponents.Add((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component);
                            m_undoManagerBeforeData.Add((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component, ((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component).SerializeToString());
                            m_originalOffsets.Add((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component, (result.VisualHit as CircuitElementDrawingVisual).Offset);
                            ComponentInternalMousePos = new Point(mousePos.X - m_selectedComponents[0].Location.X, mousePos.Y - m_selectedComponents[0].Location.Y);

                            using (DrawingContext dc = m_selectedVisual.RenderOpen())
                            {
                                Pen stroke = new Pen(Brushes.Gray, 1d);
                                Rect rect = VisualTreeHelper.GetContentBounds(result.VisualHit as Visual);
                                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), stroke, Rect.Inflate(rect, new Size(2, 2)));
                            }
                            m_selectedVisual.Offset = m_selectedComponents[0].Location;
                            m_originalSelectedVisualOffset = m_selectedVisual.Offset;

                            List<Component> removedItems = new List<Component>();
                            List<Component> addedItems = new List<Component>();
                            addedItems.Add(m_selectedComponents[0]);
                            RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, removedItems, addedItems));
                        }

                        foundHit = true;

                        return HitTestResultBehavior.Stop;
                    }), new PointHitTestParameters(e.GetPosition(this)));
                }

                m_movingMouse = foundHit;

                if (!foundHit)
                {
                    if (NewComponentData != null)
                    {
                        m_placingComponent = true;
                        m_tempComponent = Component.Create(NewComponentData);
                        m_elementVisuals.Add(m_tempComponent, new CircuitElementDrawingVisual(m_tempComponent));
                        AddVisualChild(m_elementVisuals[m_tempComponent]);
                        AddLogicalChild(m_elementVisuals[m_tempComponent]);

                        ComponentHelper.SizeComponent(m_tempComponent, m_mouseDownPos, e.GetPosition(this));
                        m_elementVisuals[m_tempComponent].Offset = m_tempComponent.Location;
                        m_elementVisuals[m_tempComponent].UpdateVisual();
                    }
                    else
                    {
                        // Selection box
                        m_originalOffsets.Clear();
                        m_selectedComponents.Clear();
                        m_selectedVisual.Offset = new Vector();
                        m_selectionBox = true;
                    }
                }
            }
            else if (enclosingRect.IntersectsWith(new Rect(e.GetPosition(this), new Size(1, 1))))
            {
                m_movingMouse = true;
                m_originalOffsets.Clear();
                m_undoManagerBeforeData.Clear();
                foreach (Component component in m_selectedComponents)
                {
                    m_originalOffsets.Add(component, component.Location);
                    m_undoManagerBeforeData.Add(component, component.SerializeToString());
                }

                using (var dc = m_selectedVisual.RenderOpen())
                    dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), new Pen(Brushes.Gray, 1d), new Rect(0, 0, enclosingRect.Width, enclosingRect.Height));
                
                m_selectedVisual.Offset = new Vector(enclosingRect.X, enclosingRect.Y);
                m_originalSelectedVisualOffset = m_selectedVisual.Offset;
            }
            else
            {
                List<Component> removedItems = new List<Component>(m_selectedComponents);

                m_selectedComponents.Clear();
                m_originalOffsets.Clear();
                m_undoManagerBeforeData.Clear();

                enclosingRect = Rect.Empty;

                using (DrawingContext dc = m_selectedVisual.RenderOpen())
                {
                }

                RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, removedItems, new List<Component>()));
            }
        }

        public void DrawConnections()
        {
            using (DrawingContext dc = m_connectionsVisual.RenderOpen())
            {
                List<ConnectionCentre> connections = new List<ConnectionCentre>();
                Dictionary<Point, bool> connectionPoints = new Dictionary<Point, bool>();
                foreach (Component component in Document.Components)
                {
                    foreach (KeyValuePair<Point, Connection> connection in component.GetConnections())
                    {
                        if (connection.Value.IsConnected && !connections.Contains(connection.Value.Centre))
                        {
                            bool draw = false;
                            if (connection.Value.ConnectedTo.Length >= 3)
                                draw = true;
                            foreach (Connection connectedConnection in connection.Value.ConnectedTo)
                            {
                                if ((connectedConnection.Flags & ConnectionFlags.Horizontal) == ConnectionFlags.Horizontal && (connection.Value.Flags & ConnectionFlags.Vertical) == ConnectionFlags.Vertical && (connection.Value.Flags & ConnectionFlags.Edge) != (connectedConnection.Flags & ConnectionFlags.Edge))
                                    draw = true;
                                else if ((connectedConnection.Flags & ConnectionFlags.Vertical) == ConnectionFlags.Vertical && (connection.Value.Flags & ConnectionFlags.Horizontal) == ConnectionFlags.Horizontal && (connection.Value.Flags & ConnectionFlags.Edge) != (connectedConnection.Flags & ConnectionFlags.Edge))
                                    draw = true;
                                if (draw)
                                    break;
                            }

                            connections.Add(connection.Value.Centre);
                            connectionPoints.Add(new Point(connection.Key.X + component.Location.X, connection.Key.Y + component.Location.Y), draw);
                        }
                        if (ShowConnectionPoints && (connection.Value.Flags & ConnectionFlags.Edge) == ConnectionFlags.Edge)
                            dc.DrawEllipse(Brushes.Blue, new Pen(Brushes.Transparent, 0d), Point.Add(connection.Key, component.Location), 2d, 2d);
                    }
                }

                int connectionIdCounter = 0;
                foreach (var connectionPoint in connectionPoints)
                {
                    if (connectionPoint.Value)
                        dc.DrawEllipse(Brushes.Black, new Pen(Brushes.Black, 1d), connectionPoint.Key, 3d, 3d);
                    if (ShowConnectionPoints)
                        dc.DrawText(new FormattedText((connectionIdCounter++).ToString(), System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                            new Typeface("Arial"), 10d, Brushes.OrangeRed), connectionPoint.Key);
                }
            }
        }

        Dictionary<Component, Vector> m_originalOffsets = new Dictionary<Component, Vector>();
        Vector m_originalSelectedVisualOffset;
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (m_resizing == ComponentResizeMode.None)
                this.Cursor = System.Windows.Input.Cursors.Arrow;

            if (m_selectedComponents.Count > 0 && m_movingMouse)
            {
                // Clear resize visual
                using (DrawingContext dc = m_resizeVisual.RenderOpen())
                {
                }

                Point mousePos = e.GetPosition(this);

                Vector offsetDelta = new Vector(mousePos.X - m_mouseDownPos.X, mousePos.Y - m_mouseDownPos.Y);

                foreach (Component component in m_selectedComponents)
                {
                    Vector newOffset = m_originalOffsets[component] + offsetDelta;

                    // Keep within bounds
                    if (newOffset.X + m_elementVisuals[component].ContentBounds.Left < 0)
                        newOffset = new Vector(1 - m_elementVisuals[component].ContentBounds.Left, newOffset.Y);
                    if (newOffset.Y + m_elementVisuals[component].ContentBounds.Top < 0)
                        newOffset = new Vector(newOffset.X, 1 - m_elementVisuals[component].ContentBounds.Top);
                    if (newOffset.X + m_elementVisuals[component].ContentBounds.Right > this.Width)
                        newOffset = new Vector(this.Width - m_elementVisuals[component].ContentBounds.Right, newOffset.Y);
                    if (newOffset.Y + m_elementVisuals[component].ContentBounds.Bottom > this.Height)
                        newOffset = new Vector(newOffset.X, this.Height - m_elementVisuals[component].ContentBounds.Bottom);

                    // Snap to grid
                    if (Math.IEEERemainder(newOffset.X, 20d) != 0)
                        newOffset.X = ComponentHelper.Snap(new Point(newOffset.X, newOffset.Y), ComponentHelper.GridSize).X;
                    if (Math.IEEERemainder(newOffset.Y, 20d) != 0)
                        newOffset.Y = ComponentHelper.Snap(new Point(newOffset.X, newOffset.Y), ComponentHelper.GridSize).Y;

                    offsetDelta = newOffset - m_originalOffsets[component];
                }

                // Update component positions
                foreach (Component component in m_selectedComponents)
                {
                    (component as Component).Location = m_originalOffsets[component] + offsetDelta;
                    m_elementVisuals[component].Offset = component.Location;
                }

                // update selection rect
                m_selectedVisual.Offset = m_originalSelectedVisualOffset + offsetDelta;
                if (enclosingRect != Rect.Empty)
                {
                    enclosingRect.X = m_selectedVisual.Offset.X;
                    enclosingRect.Y = m_selectedVisual.Offset.Y;
                }

                // update connections
                foreach (Component component in Document.Components)
                    component.DisconnectConnections();
                foreach (Component component in Document.Components)
                    component.ApplyConnections(Document);
                DrawConnections();
            }
            else if (m_placingComponent)
            {
                ComponentHelper.SizeComponent(m_tempComponent, m_mouseDownPos, e.GetPosition(this));

                // Flip if necessary
                if (m_tempComponent.Orientation == Orientation.Horizontal && m_tempComponent.Description.CanFlip)
                {
                    if (m_mouseDownPos.X > e.GetPosition(this).X)
                        m_tempComponent.IsFlipped = true;
                    else
                        m_tempComponent.IsFlipped = false;
                }
                else if (m_tempComponent.Description.CanFlip)
                {
                    if (m_mouseDownPos.Y > e.GetPosition(this).Y)
                        m_tempComponent.IsFlipped = true;
                    else
                        m_tempComponent.IsFlipped = false;
                }

                m_elementVisuals[m_tempComponent].Offset = m_tempComponent.Location;
                m_elementVisuals[m_tempComponent].UpdateVisual();
            }
            else if (m_selectionBox)
            {
                m_selectedComponents.Clear();
                m_originalOffsets.Clear();

                using (DrawingContext dc = m_selectedVisual.RenderOpen())
                {
                    VisualTreeHelper.HitTest(this, new HitTestFilterCallback(delegate(DependencyObject testObject)
                    {
                        if (testObject is CircuitElementDrawingVisual)
                            return HitTestFilterBehavior.ContinueSkipChildren;
                        else
                            return HitTestFilterBehavior.ContinueSkipSelf;
                    }),
                new HitTestResultCallback(delegate(HitTestResult result)
                {
                    if (result.VisualHit is CircuitElementDrawingVisual)
                    {
                        Rect rect = VisualTreeHelper.GetContentBounds(result.VisualHit as Visual);
                        dc.PushTransform(new TranslateTransform(((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component).Location.X, ((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component).Location.Y));
                        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), null, rect);
                        dc.Pop();
                    }

                    return HitTestResultBehavior.Continue;
                }), new GeometryHitTestParameters(new RectangleGeometry(new Rect(m_mouseDownPos, e.GetPosition(this)))));

                    dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 1d), new Rect(m_mouseDownPos, e.GetPosition(this)));
                }
            }
            else if (m_resizing != ComponentResizeMode.None)
            {
                Point mousePos = e.GetPosition(this);

                if (m_resizing == ComponentResizeMode.Left)
                    ComponentHelper.SizeComponent(m_resizingComponent, new Point(mousePos.X, m_resizingComponent.Location.Y), m_resizeComponentOriginalStartEnd);
                else if (m_resizing == ComponentResizeMode.Top)
                    ComponentHelper.SizeComponent(m_resizingComponent, new Point(m_resizingComponent.Location.X, mousePos.Y), m_resizeComponentOriginalStartEnd);
                else if (m_resizing == ComponentResizeMode.Right)
                    ComponentHelper.SizeComponent(m_resizingComponent, m_resizeComponentOriginalStartEnd, new Point(mousePos.X, m_resizingComponent.Location.Y));
                else if (m_resizing == ComponentResizeMode.Bottom)
                    ComponentHelper.SizeComponent(m_resizingComponent, m_resizeComponentOriginalStartEnd, new Point(m_resizingComponent.Location.X, mousePos.Y));

                m_resizeVisual.Offset = m_resizingComponent.Location;
                using (DrawingContext dc = m_resizeVisual.RenderOpen())
                {
                    if (m_resizingComponent.Orientation == Orientation.Horizontal)
                    {
                        dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.X - 2d, m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d));
                        dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Right - 4d, m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d));
                    }
                    else
                    {
                        dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_elementVisuals[m_resizingComponent].ContentBounds.Y - 2d, 6d, 6d));
                        dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_elementVisuals[m_resizingComponent].ContentBounds.Bottom - 4d, 6d, 6d));
                    }
                }

                m_elementVisuals[m_resizingComponent].Offset = m_resizingComponent.Location;
                m_elementVisuals[m_resizingComponent].UpdateVisual();
                DrawConnections();
            }
            else if (m_selectedComponents.Count == 0 && NewComponentData == null)
            {
                bool foundHit = false;
                VisualTreeHelper.HitTest(this, new HitTestFilterCallback(delegate(DependencyObject testObject)
                {
                    if (testObject is CircuitElementDrawingVisual)
                        return HitTestFilterBehavior.ContinueSkipChildren;
                    else
                        return HitTestFilterBehavior.ContinueSkipSelf;
                }),
                new HitTestResultCallback(delegate(HitTestResult result)
                {
                    if (result.VisualHit is CircuitElementDrawingVisual)
                    {
                        Point mousePos = e.GetPosition(this);
                        ComponentInternalMousePos = new Point(mousePos.X - (result.VisualHit as CircuitElementDrawingVisual).Offset.X, mousePos.Y - (result.VisualHit as CircuitElementDrawingVisual).Offset.Y);

                        using (DrawingContext dc = m_selectedVisual.RenderOpen())
                        {
                            Pen stroke = new Pen(Brushes.Gray, 1d);
                            //stroke.DashStyle = new DashStyle(new double[] { 2, 2 }, 0);
                            Rect rect = VisualTreeHelper.GetContentBounds(result.VisualHit as Visual);
                            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), stroke, Rect.Inflate(rect, new Size(2, 2)));
                        }
                        m_selectedVisual.Offset = (result.VisualHit as CircuitElementDrawingVisual).Offset;
                        if (((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component).Description.CanResize)
                            m_resizingComponent = (result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component;
                    }

                    foundHit = true;

                    return HitTestResultBehavior.Stop;
                }), new PointHitTestParameters(e.GetPosition(this)));

                if (!foundHit)
                {
                    // Clear selection box
                    using (DrawingContext dc = m_selectedVisual.RenderOpen())
                    {
                    }
                    // Clear resize visual
                    using (DrawingContext dc = m_resizeVisual.RenderOpen())
                    {
                    }
                }
                else if (foundHit && m_resizingComponent != null)
                {
                    // If only 1 component selected, can resize

                    m_resizeVisual.Offset = m_resizingComponent.Location;
                    using (DrawingContext dc = m_resizeVisual.RenderOpen())
                    {
                        if (m_resizingComponent.Orientation == Orientation.Horizontal)
                        {
                            dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.X - 2d, m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d));
                            dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Right - 4d, m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d));
                        }
                        else
                        {
                            dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_elementVisuals[m_resizingComponent].ContentBounds.Y - 2d, 6d, 6d));
                            dc.DrawRectangle(Brushes.DarkBlue, null, new Rect(m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_elementVisuals[m_resizingComponent].ContentBounds.Bottom - 4d, 6d, 6d));
                        }
                    }

                    // Check if cursor should be changed to resizing
                    Rect resizingRect1 = Rect.Empty;
                    Rect resizingRect2 = Rect.Empty;
                    if (m_resizingComponent != null && m_resizingComponent.Orientation == Orientation.Horizontal && m_resizingComponent.Description.CanResize)
                    {
                        // Resizing a horizontal component
                        resizingRect1 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.X - 2d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d);
                        resizingRect2 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Right - 4d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Top + m_elementVisuals[m_resizingComponent].ContentBounds.Height / 2 - 3d, 6d, 6d);
                    }
                    else if (m_resizingComponent != null && m_resizingComponent.Description.CanResize)
                    {
                        // Resizing a vertical component
                        resizingRect1 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Y - 2d, 6d, 6d);
                        resizingRect2 = new Rect(m_resizingComponent.Location.X + m_elementVisuals[m_resizingComponent].ContentBounds.Left + m_elementVisuals[m_resizingComponent].ContentBounds.Width / 2 - 3d, m_resizingComponent.Location.Y + m_elementVisuals[m_resizingComponent].ContentBounds.Bottom - 4d, 6d, 6d);
                    }

                    Rect mouseRect = new Rect(e.GetPosition(this), new Size(1,1));
                    if (resizingRect1.IntersectsWith(mouseRect))
                    {
                        if (m_resizingComponent.Orientation == Orientation.Horizontal)
                            this.Cursor = System.Windows.Input.Cursors.SizeWE;
                        else
                            this.Cursor = System.Windows.Input.Cursors.SizeNS;
                    }
                    else if (resizingRect2.IntersectsWith(mouseRect))
                    {
                        if (m_resizingComponent.Orientation == Orientation.Horizontal)
                            this.Cursor = System.Windows.Input.Cursors.SizeWE;
                        else
                            this.Cursor = System.Windows.Input.Cursors.SizeNS;
                    }
                }
            }
        }

        Rect enclosingRect = Rect.Empty;
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            m_movingMouse = false;
            if (m_resizing != ComponentResizeMode.None)
            {
                m_resizingComponent.ResetConnections();
                m_resizingComponent.ApplyConnections(Document);
                DrawConnections();

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Move component", new Component[] { m_resizingComponent });
                undoAction.AddData("before", m_undoManagerBeforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_resizingComponent, m_resizingComponent.SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();
            }
            m_resizing = ComponentResizeMode.None;
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            m_resizingComponent = null;

            if (m_placingComponent)
            {
                Component newComponent = Component.Create(NewComponentData);
                ComponentHelper.SizeComponent(newComponent, m_mouseDownPos, e.GetPosition(this));

                // Flip if necessary
                if (newComponent.Orientation == Orientation.Horizontal && newComponent.Description.CanFlip)
                {
                    if (m_mouseDownPos.X > e.GetPosition(this).X)
                        newComponent.IsFlipped = true;
                    else
                        newComponent.IsFlipped = false;
                }
                else if (newComponent.Description.CanFlip)
                {
                    if (m_mouseDownPos.Y > e.GetPosition(this).Y)
                        newComponent.IsFlipped = true;
                    else
                        newComponent.IsFlipped = false;
                }

                Document.Elements.Add(newComponent);
                newComponent.ApplyConnections(Document);
                DrawConnections();
                m_placingComponent = false;

                UndoAction undoAction = new UndoAction(UndoCommand.AddComponents, "Add component", new Component[] { newComponent });
                UndoManager.AddAction(undoAction);

                RemoveVisualChild(m_elementVisuals[m_tempComponent]);
                RemoveLogicalChild(m_elementVisuals[m_tempComponent]);
                m_elementVisuals.Remove(m_tempComponent);
                m_tempComponent = null;
            }
            else if (m_selectedComponents.Count > 0)
            {
                Dictionary<Component, string> afterData = new Dictionary<Component, string>();

                foreach (Component component in m_selectedComponents)
                {
                    string afterDataString = component.SerializeToString();
                    if (afterDataString == m_undoManagerBeforeData[component])
                        break;

                    afterData.Add(component, afterDataString);
                }

                if (afterData.Count > 0)
                {
                    UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "move", m_selectedComponents.ToArray());
                    undoAction.AddData("before", m_undoManagerBeforeData);
                    undoAction.AddData("after", afterData);
                    UndoManager.AddAction(undoAction);
                    m_undoManagerBeforeData = new Dictionary<Component, string>();
                }
            }
            else if (m_selectionBox)
            {
                using (DrawingContext dc = m_selectedVisual.RenderOpen())
                {
                    enclosingRect = Rect.Empty;

                    VisualTreeHelper.HitTest(this, new HitTestFilterCallback(delegate(DependencyObject testObject)
                    {
                        if (testObject is CircuitElementDrawingVisual)
                            return HitTestFilterBehavior.ContinueSkipChildren;
                        else
                            return HitTestFilterBehavior.ContinueSkipSelf;
                    }),
                    new HitTestResultCallback(delegate(HitTestResult result)
                    {
                        m_selectedComponents.Add((result.VisualHit as CircuitElementDrawingVisual).CircuitElement as Component);
                        
                        if (result.VisualHit is CircuitElementDrawingVisual)
                        {
                            Rect rect = VisualTreeHelper.GetContentBounds(result.VisualHit as Visual);
                            dc.PushTransform(new TranslateTransform((result.VisualHit as CircuitElementDrawingVisual).Offset.X, (result.VisualHit as CircuitElementDrawingVisual).Offset.Y));
                            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 0, 0, 100)), null, rect);
                            dc.Pop();

                            if (enclosingRect.IsEmpty)
                            {
                                rect.Offset((result.VisualHit as CircuitElementDrawingVisual).Offset.X, (result.VisualHit as CircuitElementDrawingVisual).Offset.Y);
                                enclosingRect = rect;
                            }
                            else
                            {
                                rect.Offset((result.VisualHit as CircuitElementDrawingVisual).Offset.X, (result.VisualHit as CircuitElementDrawingVisual).Offset.Y);
                                enclosingRect.Union(rect);
                            }
                        }

                        return HitTestResultBehavior.Continue;
                    }), new GeometryHitTestParameters(new RectangleGeometry(new Rect(m_mouseDownPos, e.GetPosition(this)))));

                    dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 1d), enclosingRect);
                }

                m_selectionBox = false;
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (m_resizing != ComponentResizeMode.None)
            {
                m_resizingComponent.ResetConnections();
                m_resizingComponent.ApplyConnections(Document);
                DrawConnections();

                UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "Move component", new Component[] { m_resizingComponent });
                undoAction.AddData("before", m_undoManagerBeforeData);
                Dictionary<Component, string> afterDictionary = new Dictionary<Component, string>(1);
                afterDictionary.Add(m_resizingComponent, m_resizingComponent.SerializeToString());
                undoAction.AddData("after", afterDictionary);
                UndoManager.AddAction(undoAction);
                m_undoManagerBeforeData = new Dictionary<Component, string>();

                m_resizing = ComponentResizeMode.None;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }

            m_selectionBox = false;
            m_placingComponent = false;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                if (Document == null)
                    return 4;
                else if (m_tempComponent != null)
                    return Document.Elements.Count + 5;
                else
                    return Document.Elements.Count + 4;
            }
        }

        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            /* [0] => background
             * [n] => document
             * [n+1] => selectedVisual
             * [n+2] => connectionVisual
             * [n+3] => resizeVisual
             * [n+4] => tempComponent
             */

            int n = 0;
            if (Document != null)
                n = Document.Elements.Count;

            if (index == 0)
                return m_backgroundVisual;
            else if (index == n + 1)
                return m_selectedVisual;
            else if (index == n + 2)
                return m_connectionsVisual;
            else if (index == n + 3)
                return m_resizeVisual;
            else if (index == n + 4)
                return m_elementVisuals[m_tempComponent];
            else
                return m_elementVisuals[Document.Elements[index - 1]];
        }

        /// <summary>
        /// Describes which way to resize a component.
        /// </summary>
        enum ComponentResizeMode
        {
            None,
            Left,
            Right,
            Top,
            Bottom
        }
    }
}
