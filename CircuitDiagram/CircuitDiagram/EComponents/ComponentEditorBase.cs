// ComponentEditorBase.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011  Sam Fisher
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

namespace CircuitDiagram
{
    public abstract class ComponentEditorBase : UserControl
    {
        public CircuitDocument Document { get; set; }
        public delegate void ComponentUpdatedDelegate(object sender, ComponentUpdatedEventArgs e);
        public event ComponentUpdatedDelegate ComponentUpdated;

        public ComponentEditorBase()
        {
            IsLoadingComponent = false;
            ComponentUpdated += new ComponentUpdatedDelegate(ComponentEditorBase_ComponentUpdated);
        }

        protected bool IsLoadingComponent { get; set; }
        public virtual void LoadComponent()
        {
        }

        void ComponentEditorBase_ComponentUpdated(object sender, ComponentUpdatedEventArgs e)
        {
            // Do nothing
        }

        protected virtual void CallComponentUpdated(EComponent component, string previousData)
        {
            ComponentUpdated(this, new ComponentUpdatedEventArgs(component, previousData));
        }

        public static bool AreAllValidNumericChars(string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsNumber(c)) return false;
            }

            return true;
        }
    }

    public class ComponentUpdatedEventArgs : EventArgs
    {
        public EComponent Component { get; private set; }
        public string PreviousData { get; private set; }

        public ComponentUpdatedEventArgs(EComponent component, string previousData)
        {
            Component = component;
            PreviousData = previousData;
        }
    }
}
