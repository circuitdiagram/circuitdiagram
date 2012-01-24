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


namespace CircuitDiagram.IO
{
    public static class CDDXIO
    {
        public static class RelationshipTypes
        {
            public const string IncludedComponent = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/component";
        }

        public static string AppVersion { get { return "1.0.0.0"; } }

        private static MemoryStream CreateInfoXML(CircuitDocument document, IEnumerable<CDDXPackagedComponent> components)
        {
            MemoryStream returnStream = new MemoryStream();
            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(returnStream, Encoding.UTF8);

            writer.WriteStartDocument();

            writer.WriteStartElement("cddx");
            writer.WriteAttributeString("version", "1.0"); // _info.xml version

            // Metadata
            writer.WriteStartElement("metadata");
            writer.WriteStartElement("version"); // CDDX version
            writer.WriteValue(FormatVersion);
            writer.WriteEndElement();
            writer.WriteStartElement("appversion");
            writer.WriteValue(AppVersion);
            writer.WriteEndElement();

            // Files
            writer.WriteStartElement("files");
            writer.WriteStartElement("file");
            writer.WriteAttributeString("name", "_document.xml");
            writer.WriteAttributeString("type", "document");
            writer.WriteAttributeString("content-type", "application/xml");
            writer.WriteEndElement();

            foreach (CDDXPackagedComponent component in components)
            {
                writer.WriteStartElement("file");
                writer.WriteAttributeString("name", "_components/" + component.FileName);
                writer.WriteAttributeString("type", "component");
                writer.WriteAttributeString("content-type", component.ContentType);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();

            return returnStream;
        }

        public static void Write(Stream outputStream, CircuitDocument document, CDDXSaveOptions saveOptions)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                using (Package package = ZipPackage.Open(tempStream, FileMode.Create))
                {
                    Uri documentUri = PackUriHelper.CreatePartUri(new Uri("circuitdiagram\\document.xml", UriKind.Relative));
                    PackagePart documentPart = package.CreatePart(documentUri, System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Normal);
                    package.CreateRelationship(documentPart.Uri, TargetMode.Internal, "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument");
                    CircuitDocumentWriter.WriteCDDX(document, package, documentPart, saveOptions);
                }
                tempStream.WriteTo(outputStream);
            }
        }

        public static DocumentLoadResult Read(Stream inputStream, out CircuitDocument document)
        {
            using (Package package = ZipPackage.Open(inputStream))
            {
                PackageRelationship documentRelationship = package.GetRelationshipsByType("http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument").FirstOrDefault();
                PackagePart documentPart = package.GetPart(documentRelationship.TargetUri);
                return CircuitDocumentLoader.LoadCDDX(package, documentPart, out document);
            }
        }

        #region Old
        //public static void Write(string path, CircuitDocument document)
        //{

        //    FileStream fileStream = new FileStream(path, FileMode.Create);
        //    System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fileStream);

        //    // Write header
        //    writer.Write(MagicNumber); // Magic number, 4 bytes
        //    writer.Write(FormatVersion); // Format version
        //    writer.Write(AppVersion); // Application version (always 1)
        //    writer.Write((int)(CDDXContentEncoding.XML | CDDXContentEncoding.Deflate)); // Content encoding/compression
            
        //    // Write table of contents
        //    int headerLength = 16; // 16 bytes


        //    // Write content
        //    //DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
        //    //document.Save(deflateStream);

        //    // Write other data

        //    fileStream.Close();
        //}

        //public static CircuitDocument Read(string path)
        //{
        //    try
        //    {
        //        FileStream fileStream = new FileStream(path, FileMode.Open);
        //        BinaryReader reader = new BinaryReader(fileStream);
        //        int magicNumber = reader.ReadInt32();
        //        int formatVersion = reader.ReadInt32();
        //        string appVersion = reader.ReadString();
        //        CDDXContentEncoding contentFlags = (CDDXContentEncoding)reader.ReadInt32();

        //        CircuitDocument newDocument = new CircuitDocument();
        //        DocumentLoadResult result = DocumentLoadResult.None;
        //        if ((contentFlags & CDDXContentEncoding.Deflate) == CDDXContentEncoding.Deflate)
        //        {
        //            //DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
        //            //result = newDocument.Load(deflateStream);
        //        }
        //        else
        //        {
        //            result = newDocument.Load(fileStream);
        //        }

        //        if (result == DocumentLoadResult.FailIncorrectFormat)
        //            System.Windows.MessageBox.Show("The document was not in the correct format.", "Could Not Load Document", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        //        return newDocument;
        //    }
        //    catch (Exception)
        //    {
        //        System.Windows.MessageBox.Show("The document was not in the correct format.", "Could Not Load Document", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        //        return new CircuitDocument();
        //    }
        //}

        //[Flags]
        //enum CDDXContentEncoding
        //{
        //    None = 0,
        //    /// <summary>
        //    /// The data is stored in XML format.
        //    /// </summary>
        //    XML = 1,
        //    /// <summary>
        //    /// The content is compressed using the DEFLATE algorithm.
        //    /// </summary>
        //    Deflate = 2,
        //}
        #endregion

        static readonly int MagicNumber = 6766888;
        static readonly int FormatVersion = 2;

        public class CDDXPackagedComponent
        {
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public byte[] Data { get; set; }

            public CDDXPackagedComponent(string fileName, string contentType, byte[] data)
            {
                FileName = fileName;
                ContentType = contentType;
                Data = data;
            }
        }
    }

    public enum DocumentLoadResult
    {
        None = 0,
        Success = 1,
        FailUnknown = 2,
        FailNewerVersion = 3,
        FailIncorrectFormat = 4
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
