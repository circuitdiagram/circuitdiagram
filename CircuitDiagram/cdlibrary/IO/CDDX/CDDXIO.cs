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
using CircuitDiagram.Render;

namespace CircuitDiagram.IO.CDDX
{
    public static class CDDXIO
    {
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
                    // Document
                    Uri documentUri = PackUriHelper.CreatePartUri(new Uri("circuitdiagram\\document.xml", UriKind.Relative));
                    PackagePart documentPart = package.CreatePart(documentUri, ContentTypeNames.MainDocument, CompressionOption.Normal);
                    package.CreateRelationship(documentPart.Uri, TargetMode.Internal, RelationshipTypes.Document);
                    CDDX.CDDXDocumentWriter.WriteCDDX(document, package, documentPart, saveOptions);

                    // Metadata
                    MetadataWriter.WriteMetadata(document, package);

                    // Thumbnail
                    if (saveOptions.EmbedThumbnail)
                    {
                        Uri thumbnailUri = PackUriHelper.CreatePartUri(new Uri("docProps\\thumbnail.xml", UriKind.Relative));
                        PackagePart thumbnailPart = package.CreatePart(thumbnailUri, XmlRenderer.PreviewContentType, CompressionOption.Normal);
                        package.CreateRelationship(thumbnailPart.Uri, TargetMode.Internal, RelationshipTypes.Thumbnail);
                        WriteThumbnail(document, thumbnailPart);
                    }
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
                    PackageRelationship documentRelationship = package.GetRelationshipsByType(RelationshipTypes.Document).FirstOrDefault();
                    PackagePart documentPart = package.GetPart(documentRelationship.TargetUri);
                    DocumentLoadResult result = CDDX.CDDXDocumentLoader.LoadCDDX(package, documentPart, out document);

                    if (result.Type == DocumentLoadResultType.Success || result.Type == DocumentLoadResultType.SuccessNewerVersion)
                        MetadataReader.ReadMetadata(package, document);

                    return result;
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

        private static void WriteThumbnail(CircuitDocument document, PackagePart thumbnailPart)
        {
            XmlRenderer renderer = new XmlRenderer(document.Size);
            renderer.Begin();
            document.Render(renderer);
            renderer.End();
            using (var stream = thumbnailPart.GetStream(FileMode.Create))
            {
                renderer.WriteXmlTo(stream);
            }
        }
    }

    [Serializable]
    public class CDDXSaveOptions : System.Runtime.Serialization.ISerializable
    {
        public bool IncludeConnections { get; set; }
        public bool IncludeLayout { get; set; }
        public bool EmbedThumbnail { get; set; }
        public ComponentsToEmbed EmbedComponents { get; set; }
        public List<CircuitDiagram.Components.ComponentDescription> CustomEmbedComponents { get; private set; }

        public CDDXSaveOptions()
        {
            IncludeConnections = true;
            IncludeLayout = true;
            EmbedComponents = ComponentsToEmbed.Automatic;
            CustomEmbedComponents = new List<Components.ComponentDescription>();
            EmbedThumbnail = true;
        }

        public CDDXSaveOptions(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            try
            {
                IncludeConnections = info.GetBoolean("IncludeConnections");
                IncludeLayout = info.GetBoolean("IncludeLayout");
                EmbedComponents = (ComponentsToEmbed)info.GetInt32("EmbedComponents");
                CustomEmbedComponents = new List<Components.ComponentDescription>();
                EmbedThumbnail = info.GetBoolean("EmbedThumbnail");
            }
            catch { }
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
            info.AddValue("EmbedThumbnail", EmbedThumbnail);
        }

        public static bool operator ==(CDDXSaveOptions a, CDDXSaveOptions b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return (a.IncludeConnections == b.IncludeConnections &&
                a.IncludeLayout == b.IncludeLayout &&
                a.EmbedThumbnail == b.EmbedThumbnail &&
                a.EmbedComponents == b.EmbedComponents && a.EmbedComponents != ComponentsToEmbed.Custom);
        }

        public static bool operator !=(CDDXSaveOptions a, CDDXSaveOptions b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CDDXSaveOptions))
                return false;

            return (this == obj as CDDXSaveOptions);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region Presets
        public static CDDXSaveOptions Default
        {
            get
            {
                return new CDDXSaveOptions();
            }
        }

        public static CDDXSaveOptions MinSize
        {
            get
            {
                CDDXSaveOptions options = new CDDXSaveOptions();
                options.EmbedComponents = ComponentsToEmbed.None;
                options.IncludeConnections = false;
                options.EmbedThumbnail = false;
                return options;
            }
        }
        #endregion
    }
}
