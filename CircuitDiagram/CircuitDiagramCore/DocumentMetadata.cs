// DocumentMetadata.cs
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

namespace CircuitDiagram
{
    /// <summary>
    /// Represents the metadata for a circuit document.
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// Individual or entity that created the document.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// Name given to the document.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the content of the document.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date the document was created.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The date the document was last changed.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// The application that created the document.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// The version of the application that created the document.
        /// </summary>
        public string AppVersion { get; set; }
    }
}
