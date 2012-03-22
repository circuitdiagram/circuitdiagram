// DocumentLoadResult.cs
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

namespace CircuitDiagram.IO
{
    public enum DocumentLoadResultType
    {
        None = 0,
        Success = 1,
        FailUnknown = 2,
        FailNewerVersion = 3,
        FailIncorrectFormat = 4,
        SuccessNewerVersion = 5
    }

    public class DocumentLoadResult
    {
        public DocumentLoadResultType Type { get; set; }
        public List<StandardComponentRef> UnavailableComponents { get; private set; }
        public List<string> Errors { get; private set; }

        public DocumentLoadResult()
        {
            Type = DocumentLoadResultType.None;
            Errors = new List<string>();
            UnavailableComponents = new List<StandardComponentRef>();
        }
    }

    public class StandardComponentRef
    {
        public string ImplementationSet { get; set; }
        public string ImplementationItem { get; set; }

        public StandardComponentRef(string set, string item)
        {
            ImplementationSet = set;
            ImplementationItem = item;
        }
    }
}
