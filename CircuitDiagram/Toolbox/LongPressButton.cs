// LongPressButton.cs
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
using System.Windows.Threading;
using System.Windows.Input;

namespace Toolbox
{
    public class LongPressButton : Button
    {
        public static readonly RoutedEvent LongPressEvent;

        DispatcherTimer m_longPressTimer = new DispatcherTimer();
        MouseButtonEventArgs m_tempMouseButtonEventArgs;

        static LongPressButton()
        {
            LongPressButton.LongPressEvent = EventManager.RegisterRoutedEvent("LongPress", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LongPressButton));
        }

        public LongPressButton()
        {
            this.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(LongPressButton_MouseDown), true);
            this.AddHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(LongPressButton_MouseUp), true);

            m_longPressTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            m_longPressTimer.Tick += new EventHandler(LongPressTimer_Tick);
        }

        bool m_cancelMouseUp = false;
        void LongPressTimer_Tick(object sender, EventArgs e)
        {
            // long press event
            m_longPressTimer.Stop();
            m_cancelMouseUp = true;
            RaiseEvent(new RoutedEventArgs(LongPressButton.LongPressEvent, this));
        }

        void LongPressButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_longPressTimer.Stop();
            if (m_cancelMouseUp)
            {
                m_cancelMouseUp = false;
                e.Handled = true;
            }
        }

        void LongPressButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_tempMouseButtonEventArgs = e;
            m_longPressTimer.Start();
        }
    }
}
