// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace ComponentCompiler
{
    class ManifestGenerator
    {
        public static void WriteManifest(IList<CompileResult> compiledEntries, Stream destination)
        {
            var writer = XmlWriter.Create(destination, new XmlWriterSettings
            {
                Indent = true
            });

            writer.WriteStartDocument();
            writer.WriteStartElement("components");

            foreach (var entry in compiledEntries.Where(c => c.Success))
            {
                writer.WriteStartElement("component");
                writer.WriteAttributeString("name", entry.ComponentName);
                writer.WriteAttributeString("author", entry.Author);
                writer.WriteAttributeString("guid", entry.Guid.ToString());
                writer.WriteAttributeString("input", entry.Input);

                foreach (var metaEntry in entry.Metadata)
                {
                    writer.WriteStartElement("meta");
                    writer.WriteAttributeString("name", metaEntry.Key);
                    writer.WriteAttributeString("value", metaEntry.Value);
                    writer.WriteEndElement();
                }

                foreach (var output in entry.Outputs)
                {
                    writer.WriteStartElement("output");
                    writer.WriteAttributeString("format", output.Key);
                    writer.WriteValue(output.Value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndDocument();

            writer.Flush();
            writer.Dispose();
        }
    }
}
