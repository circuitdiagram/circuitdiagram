// LegacyCDDXReader.cs
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
using System.IO.Compression;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Reads a Circuit Diagram 1.x format CDDX document.
    /// </summary>
    static class LegacyCDDXReader
    {
        public static bool Read(Stream stream, out IODocument circuitDocument, out DocumentLoadResult loadResult)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);
                int magicNumber = reader.ReadInt32();
                int formatVersion = reader.ReadInt32();
                string appVersion = reader.ReadString();
                CDDXContentEncoding contentFlags = (CDDXContentEncoding)reader.ReadInt32();
                uint contentOffset = 0;
                if (formatVersion >= 2)
                    contentOffset = reader.ReadUInt32(); // offset to content

                if (formatVersion >= 2)
                    stream.Seek(contentOffset, SeekOrigin.Begin);
                CircuitDocument newDocument = new CircuitDocument();
                if ((contentFlags & CDDXContentEncoding.Deflate) == CDDXContentEncoding.Deflate)
                {
                    DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress);

                    Xml.XmlReader xmlReader = new Xml.XmlReader();
                    bool result = xmlReader.Load(deflateStream);
                    circuitDocument = xmlReader.Document;
                    loadResult = xmlReader.LoadResult;
                    loadResult.Format = "CDDX (Legacy 1.0)";
                    return result;
                }
                else
                {
                    Xml.XmlReader xmlReader = new Xml.XmlReader();
                    bool result = xmlReader.Load(stream);
                    circuitDocument = xmlReader.Document;
                    loadResult = xmlReader.LoadResult;
                    loadResult.Format = "CDDX (Legacy 1.0)";
                    return result;
                }
            }
            catch (Exception)
            {
                circuitDocument = null;
                loadResult = new DocumentLoadResult();
                loadResult.Type = DocumentLoadResultType.FailIncorrectFormat;
                return false;
            }
        }

        [Flags]
        enum CDDXContentEncoding
        {
            None = 0,
            /// <summary>
            /// The data is stored in XML format.
            /// </summary>
            XML = 1,
            /// <summary>
            /// The content is compressed using the DEFLATE algorithm.
            /// </summary>
            Deflate = 2
        }
    }
}
