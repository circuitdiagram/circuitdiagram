// CDDXIO.cs
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
using System.IO.Packaging;

namespace CircuitDiagram.IO.CDDX
{
    public static class CDDXIO
    {
        public const string StandardCollection = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/components/common";
        public const string MiscCollection = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/components/misc";

        public static class RelationshipTypes
        {
            public const string IncludedComponent = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/component";
        }

        /// <summary>
        /// Writes the Circuit Document in the CDDX format.
        /// </summary>
        /// <param name="outputStream">The stream to save the document to.</param>
        /// <param name="document">The document to save.</param>
        /// <param name="saveOptions">Specifies options for saving the document.</param>
        public static void Write(Stream outputStream, CircuitDocument document, CDDXSaveOptions saveOptions)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                using (Package package = ZipPackage.Open(tempStream, FileMode.Create))
                {
                    Uri documentUri = PackUriHelper.CreatePartUri(new Uri("circuitdiagram\\document.xml", UriKind.Relative));
                    PackagePart documentPart = package.CreatePart(documentUri, System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Normal);
                    package.CreateRelationship(documentPart.Uri, TargetMode.Internal, "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument");
                    CDDX.CDDXDocumentWriter.WriteCDDX(document, package, documentPart, saveOptions);
                }
                tempStream.WriteTo(outputStream);
            }
        }

        /// <summary>
        /// Loads a CDDX format document from the specified stream.
        /// </summary>
        /// <param name="inputStream">The stream to load the document from.</param>
        /// <param name="document">The loaded document.</param>
        /// <returns>Result of the read operation.</returns>
        public static DocumentLoadResult Read(Stream inputStream, out CircuitDocument document)
        {
            try
            {
                using (Package package = ZipPackage.Open(inputStream))
                {
                    PackageRelationship documentRelationship = package.GetRelationshipsByType("http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument").FirstOrDefault();
                    PackagePart documentPart = package.GetPart(documentRelationship.TargetUri);
                    return CDDX.CDDXDocumentLoader.LoadCDDX(package, documentPart, out document);
                }
            }
            catch (Exception)
            {
                document = null;

                DocumentLoadResult result = new DocumentLoadResult();
                result.Type = DocumentLoadResultType.FailIncorrectFormat;
                return result;
            }
        }
    }

    [Serializable]
    public class CDDXSaveOptions : System.Runtime.Serialization.ISerializable
    {
        public bool IncludeConnections { get; set; }
        public bool IncludeLayout { get; set; }
        public ComponentsToEmbed EmbedComponents { get; set; }
        public List<CircuitDiagram.Components.ComponentDescription> CustomEmbedComponents { get; private set; }

        public CDDXSaveOptions()
        {
            IncludeConnections = true;
            IncludeLayout = true;
            EmbedComponents = ComponentsToEmbed.Automatic;
            CustomEmbedComponents = new List<Components.ComponentDescription>();
        }

        public CDDXSaveOptions(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            IncludeConnections = info.GetBoolean("IncludeConnections");
            IncludeLayout = info.GetBoolean("IncludeLayout");
            EmbedComponents = (ComponentsToEmbed)info.GetInt32("EmbedComponents");
            CustomEmbedComponents = new List<Components.ComponentDescription>();
        }

        public enum ComponentsToEmbed
        {
            Automatic,
            All,
            None,
            Custom
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("IncludeConnections", IncludeConnections);
            info.AddValue("IncludeLayout", IncludeLayout);
            info.AddValue("EmbedComponents", (int)EmbedComponents);
        }
    }
}
