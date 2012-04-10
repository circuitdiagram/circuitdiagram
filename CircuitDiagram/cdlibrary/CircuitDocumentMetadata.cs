// CircuitDocumentMetadata.cs
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
    public class CircuitDocumentMetadata
    {
        /// <summary>
        /// File format of the document.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The path to the saved document.
        /// </summary>
        public string Path { get; set; }

        public DublinCoreMetadata DublinCore { get; private set; }

        public ExtendedMetadata Extended { get; private set; }
        
        public CircuitDocumentMetadata()
        {
            DublinCore = new DublinCoreMetadata();
            Extended = new ExtendedMetadata();
        }
    }

    public class DublinCoreMetadata
    {
        public string Contributor { get; set; }
        public string Creator { get; set; }
        /// <summary>
        /// Date the document was created.
        /// </summary>
        public DateTime? Date { get; set; }
        /// <summary>
        /// Description of the document.
        /// </summary>
        public string Description { get; set; }
        public string Format { get; set; }
        /// <summary>
        /// The document title.
        /// </summary>
        public string Title { get; set; }
    }

    public class ExtendedMetadata
    {
        /// <summary>
        /// Name of the application used to create the document.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Version of the application used to create the document.
        /// </summary>
        public string AppVersion { get; set; }
    }
}
