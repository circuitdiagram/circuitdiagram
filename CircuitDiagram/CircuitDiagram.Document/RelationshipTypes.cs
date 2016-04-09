// RelationshipTypes.cs
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

namespace CircuitDiagram.Document
{
    /// <summary>
    /// Holds the relationship types for a CDDX document.
    /// </summary>
    public static class RelationshipTypes
    {
        /// <summary>
        /// Relationship for the main document part.
        /// </summary>
        public const string Document = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument";

        /// <summary>
        /// Relationship for an embedded component, referenced from within the main document part.
        /// </summary>
        public const string IncludedComponent = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/component";

        /// <summary>
        /// Relationship for the embedded thumbnail.
        /// </summary>
        public const string Thumbnail = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/thumbnail";

        /// <summary>
        /// Relationship for the core properties.
        /// </summary>
        public const string CoreProperties = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/core-properties";

        /// <summary>
        /// Relationship for the extended properties.
        /// </summary>
        public const string ExtendedProperties = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/extended-properties";

        /// <summary>
        /// Relationship for format properties.
        /// </summary>
        public const string FormatProperties = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/format/properties";
    }
}
