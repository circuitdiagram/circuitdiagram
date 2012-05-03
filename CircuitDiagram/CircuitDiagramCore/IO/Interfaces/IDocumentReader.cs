// IDocumentReader.cs
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
using System.IO;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides methods to read an IODocument from a stream.
    /// </summary>
    public interface IDocumentReader : IDisposable
    {
        /// <summary>
        /// The name for the plugin this reader belongs to.
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// The version of this reader.
        /// </summary>
        string ReaderVersion { get; }

        /// <summary>
        /// The unique GUID for this reader.
        /// </summary>
        Guid GUID { get; }

        /// <summary>
        /// Gets the document loaded from the stream.
        /// </summary>
        IODocument Document { get; }

        /// <summary>
        /// Gets details of the load result.
        /// </summary>
        DocumentLoadResult LoadResult { get; }

        /// <summary>
        /// Attempts to load a document from the stream.
        /// </summary>
        /// <param name="stream">Stream to load from.</param>
        /// <returns>True if succeeded, false otherwise.</returns>
        bool Load(Stream stream);

        /// <summary>
        /// Determines whether the specified component type is embedded within the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it is available, false otherwise.</returns>
        bool IsDescriptionEmbedded(IOComponentType type);

        /// <summary>
        /// Retrieves the specified component type from the document.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The data if the type is available, null otherwise.</returns>
        EmbedComponentData GetEmbeddedDescription(IOComponentType type);
    }
}
