// ToolboxItem.cs
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
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;

namespace Toolbox
{
    public class ToolboxItem : ListBoxItem
    {
        public static readonly DependencyProperty IsPressedProperty;
        public static readonly RoutedEvent ClickEvent;

        static ToolboxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
            IsPressedProperty = DependencyProperty.Register("IsPressed", typeof(bool), typeof(ToolboxItem));
            ClickEvent = Button.ClickEvent;
        }

        public ToolboxItem()
        {
        }

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(Button.ClickEvent, value); }
            remove { RemoveHandler(Button.ClickEvent, value); }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                IsPressed = true;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            IsPressed = false;

            base.OnMouseLeave(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ToolboxCategory parent = ItemsControl.ItemsControlFromItemContainer(this) as ToolboxCategory;
            if (e.OriginalSource != parent)
                IsPressed = true;

            e.Handled = true;

            if (parent != null)
            {
                parent.NotifyToolboxItemMouseDown(this);
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            IsPressed = false;

            e.Handled = true;

            ToolboxCategory parent = ItemsControl.ItemsControlFromItemContainer(this) as ToolboxCategory;
            if (parent != null)
            {
                parent.NotifyToolboxItemMouseUp(this);
            }

            base.OnMouseLeftButtonUp(e);
        }
    }
}
