﻿// Circuit Diagram http://www.circuit-diagram.org/
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
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Compiler
{
    public class ComponentCompileResult
    {
        public string ComponentName { get; set; }
        public Guid Guid { get; set; }
        public string Author { get; set; }
        public bool Success { get; set; }
        public ComponentDescription Description { get; set; }
    }
}
