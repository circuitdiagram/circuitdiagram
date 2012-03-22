// ImplementationConversion.cs
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
using System.Collections.ObjectModel;

namespace CircuitDiagram
{
    public class ImplementationConversionCollection
    {
        public string ImplementationSet { get; set; }
        public ObservableCollection<ImplementationConversion> Items { get; private set; }

        public ImplementationConversionCollection()
        {
            Items = new ObservableCollection<ImplementationConversion>();
        }

        public override string ToString()
        {
            return ImplementationSet;
        }
    }

    public class ImplementationConversion
    {
        public string ImplementationName { get; set; }

        public string ToName { get; set; }
        public Guid ToGUID { get; set; }
        public string ToConfiguration { get; set; }
        public System.Windows.Media.ImageSource ToIcon { get; set; }

        public bool NoConfiguration { get { return String.IsNullOrEmpty(ToConfiguration); } }
        public bool HasConfiguration { get { return !NoConfiguration; } }
    }
}
