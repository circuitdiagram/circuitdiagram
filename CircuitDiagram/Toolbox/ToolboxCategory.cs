// ToolboxCategory.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2013  Sam Fisher
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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Toolbox
{
    [TemplatePart(Name = "PART_SelectedItem", Type = typeof(UIElement))]
    [StyleTypedPropertyAttribute(Property = "ItemContainerStyle", StyleTargetType = typeof(ToolboxItem))]
    public class ToolboxCategory : System.Windows.Controls.Primitives.Selector
    {
        public static readonly DependencyProperty IsPressedProperty;
        public static readonly DependencyProperty IsExpandedProperty;
        public static readonly DependencyProperty IsExpanderVisibleProperty;
        public static readonly DependencyProperty IsSelectedCategoryProperty;
        public static readonly DependencyProperty MainItemProperty;
        public static readonly DependencyProperty MainItemTemplateProperty;
        public static readonly DependencyProperty ItemDisplayTemplateProperty;
        public static readonly RoutedEvent LongPressEvent;

        DispatcherTimer m_longPressTimer = new DispatcherTimer();
        MouseButtonEventArgs m_tempMouseButtonEventArgs;

        ObservableCollection<object> m_expanderItems = new ObservableCollection<object>();

        UIElement PartSelectedItem;

        static ToolboxCategory()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxCategory), new FrameworkPropertyMetadata(typeof(ToolboxCategory)));
            ToolboxCategory.LongPressEvent = EventManager.RegisterRoutedEvent("LongPress", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToolboxCategory));
            ToolboxCategory.IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ToolboxCategory));
            ToolboxCategory.IsExpanderVisibleProperty = DependencyProperty.Register("IsExpanderVisible", typeof(bool), typeof(ToolboxCategory));
            ToolboxCategory.IsSelectedCategoryProperty = DependencyProperty.Register("IsSelectedCategory", typeof(bool), typeof(ToolboxCategory));
            ToolboxCategory.MainItemProperty = DependencyProperty.Register("MainItem", typeof(object), typeof(ToolboxCategory));
            ToolboxCategory.MainItemTemplateProperty = DependencyProperty.Register("MainItemTemplate", typeof(object), typeof(ToolboxCategory));
            ToolboxCategory.ItemDisplayTemplateProperty = DependencyProperty.Register("ItemDisplayTemplate", typeof(DataTemplate), typeof(ToolboxCategory));
            ToolboxCategory.IsPressedProperty = DependencyProperty.Register("IsPressed", typeof(bool), typeof(ToolboxCategory));
        }

        public ToolboxCategory()
        {
            this.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(ToolboxCategory_MouseDown));
            this.AddHandler(FrameworkElement.PreviewMouseUpEvent, new MouseButtonEventHandler(ToolboxCategory_PreviewMouseUp));
            this.AddHandler(FrameworkElement.MouseLeaveEvent, new MouseEventHandler(ToolboxCategory_MouseLeave));
            this.AddHandler(FrameworkElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(ToolboxCategory_RightMouseButtonDown));
            m_longPressTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            m_longPressTimer.Tick += new EventHandler(LongPressTimer_Tick);
            SelectedIndex = 0;
        }

        /// <summary>
        /// Gets or sets the Toolbox that owns this ToolboxCategory.
        /// </summary>
        public Toolbox ToolboxOwner { get; set; }

        public DataTemplate ItemDisplayTemplate
        {
            get { return (DataTemplate)GetValue(ToolboxCategory.ItemDisplayTemplateProperty); }
            set { SetValue(ToolboxCategory.ItemDisplayTemplateProperty, value); }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (Items.Count > 1)
                IsExpanderVisible = true;
            else
                IsExpanderVisible = false;
        }

        /// <summary>
        /// A virtual function that is called when the selection is changed. Default behavior
        /// is to raise a SelectionChangedEvent
        /// </summary> 
        /// <param name="e">The inputs for this event. Can be raised (default behavior) or processed
        ///   in some other way.</param> 
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectedItem == null)
                return;
            SelectedItemUpdated();
        }

        void GenerateItemContainers()
        {
            IItemContainerGenerator generator = ItemContainerGenerator;
            GeneratorPosition position = generator.GeneratorPositionFromIndex(0);
            using (generator.StartAt(position, GeneratorDirection.Forward, true))
            {
                foreach (object o in Items)
                {
                    DependencyObject dp = generator.GenerateNext();
                    generator.PrepareItemContainer(dp);
                }
            }
        }

        void SelectedItemUpdated()
        {
            MainItem = SelectedItem;
            return;
        }

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        public object MainItem
        {
            get { return GetValue(ToolboxCategory.MainItemProperty); }
            set { SetValue(ToolboxCategory.MainItemProperty, value); }
        }

        public object MainItemTemplate
        {
            get { return GetValue(ToolboxCategory.MainItemTemplateProperty); }
            set { SetValue(ToolboxCategory.MainItemTemplateProperty, value); }
        }

        public bool IsExpanderVisible
        {
            get { return (bool)GetValue(ToolboxCategory.IsExpanderVisibleProperty); }
            set { SetValue(ToolboxCategory.IsExpanderVisibleProperty, value); }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(ToolboxCategory.IsExpandedProperty); }
            set { SetValue(ToolboxCategory.IsExpandedProperty, value); }
        }

        public bool IsSelectedCategory
        {
            get { return (bool)GetValue(ToolboxCategory.IsSelectedCategoryProperty); }
            set { SetValue(ToolboxCategory.IsSelectedCategoryProperty, value); }
        }

        void LongPressTimer_Tick(object sender, EventArgs e)
        {
            // long press event
            m_longPressTimer.Stop();
            RaiseEvent(new RoutedEventArgs(LongPressButton.LongPressEvent, this));
            if (Items.Count > 1)
            {
                IsExpanded = true;
                m_mouseDownCausedExpand = true;
            }
        }

        public override void OnApplyTemplate()
        {
            PartSelectedItem = base.GetTemplateChild("PART_SelectedItem") as UIElement;
        }

        void ToolboxCategory_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_longPressTimer.Stop();
        }

        bool m_mouseDownCausedExpand = false;
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            IsPressed = false;

            if (!m_mouseDownCausedExpand && (PartSelectedItem == null || PartSelectedItem.IsAncestorOf(e.OriginalSource as DependencyObject)))
            {
                // Clicked

                SelectItem(SelectedItem);
                IsExpanded = false;

                var toolboxParent = VisualTreeHelperExtensions.FindParent<Toolbox>(this);
                if (toolboxParent != null)
                    toolboxParent.SetSelected(this);
            }
            m_mouseDownCausedExpand = false;
        }

        void ToolboxCategory_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Right)
                return;

            if (!IsExpanded && (PartSelectedItem == null || PartSelectedItem.IsAncestorOf(e.OriginalSource as DependencyObject)))
            {
                m_tempMouseButtonEventArgs = e;
                m_longPressTimer.Start();
            }

            if (PartSelectedItem != null && PartSelectedItem.IsAncestorOf(e.OriginalSource as DependencyObject))
                IsPressed = true;

            if (SelectedItem != null)
            {
                // call event handlers for selected item
                Control selectedItemControl = SelectedItem as Control;
                if (selectedItemControl != null)
                {
                    MouseButtonEventArgs newEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice);
                    newEventArgs.RoutedEvent = MouseDownEvent;
                    newEventArgs.Source = this;
                    selectedItemControl.RaiseEvent(newEventArgs);
                }
            }
        }

        void ToolboxCategory_RightMouseButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsExpanded && (PartSelectedItem == null || PartSelectedItem.IsAncestorOf(e.OriginalSource as DependencyObject)))
            {
                m_tempMouseButtonEventArgs = e;
                m_mouseDownCausedExpand = true;
                IsExpanded = true;
            }

            if (PartSelectedItem != null && PartSelectedItem.IsAncestorOf(e.OriginalSource as DependencyObject))
                IsPressed = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (PartSelectedItem == null)
                return;
            if (!PartSelectedItem.IsMouseOver)
                IsPressed = false;
            else if (e.LeftButton == MouseButtonState.Pressed && PartSelectedItem.IsMouseOver)
                IsPressed = true;

            base.OnMouseMove(e);
        }

        void ToolboxCategory_MouseLeave(object sender, MouseEventArgs e)
        {
            m_lastMouseUp = null;
            m_mouseDownCausedExpand = false;
            IsExpanded = false;
            m_longPressTimer.Stop();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// true if the item is (or is eligible to be) its own container; otherwise, false.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ToolboxItem;
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolboxItem() { ToolboxItemContentTemplate = ItemDisplayTemplate };
        }

        void SelectItem(object item)
        {
            SelectedItem = item;
            SelectedItemUpdated();
        }

        ToolboxItem m_lastMouseUp;

        // Called by ToolboxItem when it receives a MouseDown
        internal void NotifyToolboxItemMouseDown(ToolboxItem toolboxItem)
        {
            m_lastMouseUp = toolboxItem;
        }

        // Called by ToolboxItem when it receives a MouseUp
        internal void NotifyToolboxItemMouseUp(ToolboxItem toolboxItem)
        {
            if (!toolboxItem.IsEnabled)
                return;

            if (SelectedItem != toolboxItem || m_lastMouseUp == toolboxItem)
            {
                var item = ItemContainerGenerator.ItemFromContainer(toolboxItem);
                SelectItem(item);

                if (ToolboxOwner != null)
                    ToolboxOwner.SetSelected(this);
            }

            IsExpanded = false; // close expander
        }
    }
}
