// BinaryWriter.cs
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

/* Binary file format:
 * 
 * All values little-endian
 *  
 *  _HEADER_
 *  offset  length  description
 *  00      4B      Magic number (...)  
 *  02      1B      Version number      (unsigned byte)
 *  03      4B      MD5 checksum
 *  07      4B      Flags               (unsigned int)
 *  09      4B      Length of file including header      (unsigned int)
 *  13      4B      Offset to data      (unsigned int)
 *  17      4B      Number of content items (unsigned int)
 *  21      4B      SHA-1 signature (RSA)
 *  TODO: add signature
 *  
 *  _CONTENT_ITEM_
 *  offset  length  description
 *  00      2B      Type (component|resource)
 *  02      4B      Length
 *  
 *  _RESOURCE_ITEM_
 *  offset  length  description
 *  06      4B      ID                  (unsigned int)
 *  10      4B      Mime-type           (unsigned int)
 *  14      ...     Data
 *  
 *  _COMPONENT_ITEM_
 *  offset  length  description
 *  06      4B      ID
 *  10      4B      Number of sections
 *  14      ...     Sections
 *  
 *  _COMPONENT_ITEM_SECTION_
 *  offset  length  description
 *  00      2B      Section type (metadata|flags|properties|configurations|connections|render)
 *  02      4B      Length
 *  
 *  _METADATA_SECTION_
 *  offset  length  description
 *                  Component name
 *                  Can resize
 *                  Minimum size
 *                  GUID
 *                  Author
 *                  Version
 *                  AdditionalInformation
 *                  Implement set
 *                  Implement item
 *                  IconresourceID
 * 
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CircuitDiagram.Components;
using CircuitDiagram.Render.Path;
using BW = System.IO.BinaryWriter;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CircuitDiagram.TypeDescriptionIO.Util;
using CircuitDiagram.IO;

namespace CircuitDiagram.TypeDescriptionIO.Binary
{
    public static class BinaryConstants
    {
        public const byte FormatVersion = 2;
        public const byte FormattedTextVersion = 1;

        public enum ContentItemType : ushort
        {
            Component = 1,
            Resource = 2
        }

        public enum ComponentSectionType : ushort
        {
            Metadata = 1,
            Flags = 2,
            Properties = 3,
            Configurations = 4,
            Connections = 5,
            Render = 6
        }

        public enum MetadataPropertyType : ushort
        {
            ComponentName = 1,
            CanResize = 2,
            CanFlip = 3,
            MinSize = 4,
            GUID = 5,
            Author = 6,
            Version = 7,
            AdditionalInformation = 8,
            ImplementSet = 9,
            ImplementItem = 10,
            // 5 - 20 reserved
            IconResourceID = 21
        }

        public enum ResourceMimeType : uint
        {
            Image_Png = 11,
            Image_Jpeg = 12,
            Image_Gif = 13
        }

        public static string ResourceMimeTypeToString(uint type)
        {
            try
            {
                switch ((ResourceMimeType)type)
                {
                    case ResourceMimeType.Image_Png:
                        return "image/png";
                    case ResourceMimeType.Image_Jpeg:
                        return "image/jpeg";
                    case ResourceMimeType.Image_Gif:
                        return "image/gif";
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public sealed class BinaryWriter
    {
        private Stream m_finalStream;

        private MemoryStream m_contentStream;
        private MemoryStream m_resourcesStream;

        public List<ComponentDescription> Descriptions { get; private set; }
        public List<BinaryResource> Resources { get; private set; }
        public BinaryWriterSettings Settings { get; private set; }

        public BinaryWriter(Stream stream, BinaryWriterSettings settings)
        {
            m_contentStream = new MemoryStream();
            m_resourcesStream = new MemoryStream();
            m_finalStream = stream;
            Descriptions = new List<ComponentDescription>();
            Resources = new List<BinaryResource>();
            Settings = settings;
        }

        private void WriteHeader(System.IO.BinaryWriter writer)
        {
            using (MemoryStream allContentStream = new MemoryStream((int)m_contentStream.Length + (int)m_resourcesStream.Length))
            {
                m_contentStream.WriteTo(allContentStream);
                m_resourcesStream.WriteTo(allContentStream);

                // calculate MD5 hash of allContentStream
                MD5 md5 = MD5.Create();
                byte[] md5hash = md5.ComputeHash(allContentStream);

                // calculate signature
                byte[] sha1signature = new byte[128];
                byte[] certData = null;
                if (Settings.Certificate != null)
                {
                    certData = Settings.Certificate.Export(X509ContentType.Cert);

                    var dsa = Settings.Certificate.GetRSAPrivateKey();
                    sha1signature = dsa.SignData(allContentStream.ToArray(), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                }

                writer.Write(new byte[] { 184, 67, 68, 67, 79, 77, 13, 10 }); // magic number ( 184, c, d, c, o, m, \r, \n )
                writer.Write(BinaryConstants.FormatVersion); // format version
                writer.Write(md5hash); // MD5 hash of content (16-bytes)
                writer.Write(0u); // flags/reserved
                writer.Write((uint)((Settings.Certificate != null ? 8u + sha1signature.Length + certData.Length : 0) + 42u + m_contentStream.Length + m_resourcesStream.Length)); // length of file including header
                writer.Write((uint)(Settings.Certificate != null ? 8u + +sha1signature.Length + certData.Length : 0) + 42u); // offset to data from start of file (in bytes)
                writer.Write((uint)(Descriptions.Count + Resources.Count)); // number of content items
                writer.Write(Settings.Certificate != null); // is signed
                if (Settings.Certificate != null)
                {
                    writer.Write(sha1signature.Length);
                    writer.Write(sha1signature); // SHA-1 (RSA) signature of content (128-bytes)
                    writer.Write(certData.Length);
                    writer.Write(certData);
                }
            }
        }

        /// <summary>
        /// Generates a unique resource ID.
        /// </summary>
        /// <returns>A unique resource ID.</returns>
        public uint NewResourceID()
        {
            uint counter = 0;
            while (Resources.Exists(item => item.ID == counter))
                counter++;
            return counter;
        }

        private void WriteDescriptions()
        {
            BW mainWriter = new BW(m_contentStream);

            foreach (ComponentDescription description in Descriptions)
            {
                using (MemoryStream tempStream = new MemoryStream())
                {
                    BW writer = new BW(tempStream);

                    writer.Write(NewResourceID()); // ID
                    writer.Write(6u); // 6 sections

                    #region Metadata
                    // Write METADATA
                    /* Component name (string)
                     *  Can resize (bool)
                     *  Minimum size (bool)
                     *  GUID (byte[16])
                     *  Author (string)
                     *  Version (major, minor) (uint, uint)
                     *  AdditionalInformation (string)
                     *  Implement set (string)
                     *  Implement item (string)
                     *  IconresourceID (int)
                     *  Timestamp (long) */
                    using (MemoryStream metadatSectionStream = new MemoryStream())
                    {
                        BW metadataWriter = new BW(metadatSectionStream);
                        metadataWriter.WriteNullString(description.ComponentName);
                        metadataWriter.Write(description.CanResize);
                        metadataWriter.Write(description.CanFlip);
                        metadataWriter.Write(description.MinSize);
                        metadataWriter.Write(description.Metadata.GUID.ToByteArray());
                        metadataWriter.WriteNullString(description.Metadata.Author);
                        metadataWriter.Write((ushort)description.Metadata.Version.Major);
                        metadataWriter.Write((ushort)description.Metadata.Version.Minor);
                        metadataWriter.WriteNullString(description.Metadata.AdditionalInformation);
                        metadataWriter.WriteNullString(description.Metadata.ImplementSet);
                        metadataWriter.WriteNullString(description.Metadata.ImplementItem);
                        if (description.Metadata.Icon != null)
                        {
                            uint iconResourceID = NewResourceID();
                            foreach (var icon in description.Metadata.Icon as MultiResolutionImage)
                            {
                                BinaryResource iconResource = new BinaryResource();
                                iconResource.ID = iconResourceID;
                                // Only PNG images can be written at the moment
                                iconResource.ResourceType = BinaryResourceType.PNGImage;
                                iconResource.Buffer = icon.Data;
                                Resources.Add(iconResource);
                            }
                            metadataWriter.Write((int)iconResourceID);
                        }
                        else
                            metadataWriter.Write(-1);
                        metadataWriter.Write(DateTime.Now.ToBinary());

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Metadata);
                        writer.Write((uint)metadatSectionStream.Length);
                        writer.Write(metadatSectionStream.ToArray());
                    }
                    #endregion

                    #region Flags
                    // Write FLAGS
                    using (MemoryStream flagsSectionStream = new MemoryStream())
                    {
                        BW flagsWriter = new BW(flagsSectionStream);
                        flagsWriter.Write((uint)description.Flags.Length);
                        foreach (Conditional<FlagOptions> flags in description.Flags)
                        {
                            flagsWriter.Write(flags.Conditions);
                            flagsWriter.Write((uint)flags.Value);
                        }

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Flags);
                        writer.Write((uint)flagsSectionStream.Length);
                        writer.Write(flagsSectionStream.ToArray());
                    }
                    #endregion

                    #region Properties
                    // Write PROPERTIES
                    using (MemoryStream propertiesSectionStream = new MemoryStream())
                    {
                        BW propertiesWriter = new BW(propertiesSectionStream);

                        propertiesWriter.Write((uint)description.Properties.Length);
                        foreach (ComponentProperty property in description.Properties)
                        {
                            propertiesWriter.Write(property.Name);
                            propertiesWriter.Write(property.SerializedName.Value);
                            propertiesWriter.Write(property.DisplayName);
                            propertiesWriter.WriteType(property.Default, property.EnumOptions != null);
                            if (property.EnumOptions != null)
                            {
                                propertiesWriter.Write(property.EnumOptions.Length);
                                foreach (string option in property.EnumOptions)
                                    propertiesWriter.Write(option);
                            }

                            // Format rules
                            propertiesWriter.Write((uint)property.FormatRules.Length);
                            foreach (ComponentPropertyFormat formatRule in property.FormatRules)
                            {
                                propertiesWriter.Write(formatRule.Conditions);
                                propertiesWriter.Write(formatRule.Value);
                            }

                            // Other conditions
                            propertiesWriter.Write((uint)property.OtherConditions.Count);
                            foreach (KeyValuePair<PropertyOtherConditionType, IConditionTreeItem> otherCondition in property.OtherConditions)
                            {
                                propertiesWriter.Write((uint)otherCondition.Key);
                                propertiesWriter.Write(otherCondition.Value);
                            }
                        }

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Properties);
                        writer.Write((uint)propertiesSectionStream.Length);
                        writer.Write(propertiesSectionStream.ToArray());
                    }
                    #endregion

                    #region Configurations
                    // Write CONFIGURATIONS
                    using (MemoryStream configurationsStream = new MemoryStream())
                    {
                        BW configurationWriter = new BW(configurationsStream);

                        configurationWriter.Write((uint)description.Metadata.Configurations.Count);
                        foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                        {
                            configurationWriter.Write(configuration.Name);
                            configurationWriter.Write((configuration.ImplementationName ?? ""));

                            configurationWriter.Write(configuration.Setters.Count);
                            foreach (var setter in configuration.Setters)
                            {
                                foreach (ComponentProperty property in description.Properties)
                                    if (property.SerializedName == setter.Key)
                                    {
                                        configurationWriter.Write(property.SerializedName.Value);
                                        break;
                                    }
                                configurationWriter.WriteType(setter.Value);
                            }

                            if (!Settings.IgnoreIcons && configuration.Icon != null)
                            {
                                uint iconResourceID = NewResourceID();
                                foreach (var icon in configuration.Icon as MultiResolutionImage)
                                {
                                    BinaryResource iconResource = new BinaryResource();
                                    iconResource.ID = iconResourceID;
                                    // Only PNG images can be written at the moment
                                    iconResource.ResourceType = BinaryResourceType.PNGImage;
                                    iconResource.Buffer = icon.Data;
                                    Resources.Add(iconResource);
                                }
                                configurationWriter.Write((int)iconResourceID);
                            }
                            else
                                configurationWriter.Write(-1);
                        }

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Configurations);
                        writer.Write((uint)configurationsStream.Length);
                        writer.Write(configurationsStream.ToArray());
                    }
                    #endregion

                    #region Connections
                    // Write CONNECTIONS
                    using (MemoryStream connectionsStream = new MemoryStream())
                    {
                        BW connectionsWriter = new BW(connectionsStream);

                        connectionsWriter.Write((uint)description.Connections.Length);
                        foreach (ConnectionGroup connectionGroup in description.Connections)
                        {
                            connectionsWriter.Write(connectionGroup.Conditions);
                            connectionsWriter.Write((uint)connectionGroup.Value.Length);
                            foreach (ConnectionDescription connection in connectionGroup.Value)
                            {
                                connectionsWriter.Write(connection.Start);
                                connectionsWriter.Write(connection.End);
                                connectionsWriter.Write((int)connection.Edge);
                                connectionsWriter.Write(connection.Name.Value);
                            }
                        }

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Connections);
                        writer.Write((uint)connectionsStream.Length);
                        writer.Write(connectionsStream.ToArray());
                    }
                    #endregion

                    #region Render
                    // Write RENDER
                    using (MemoryStream renderStream = new MemoryStream())
                    {
                        BW renderWriter = new BW(renderStream);

                        renderWriter.Write(description.RenderDescriptions.Length);
                        foreach (RenderDescription renderDescription in description.RenderDescriptions)
                        {
                            renderWriter.Write(renderDescription.Conditions);
                            renderWriter.Write(renderDescription.Value.Length); // number of render commands
                            foreach (IRenderCommand command in renderDescription.Value)
                            {
                                renderWriter.Write((uint)command.Type); // command type
                                switch (command.Type)
                                {
                                    case RenderCommandType.Line:
                                        {
                                            Line line = command as Line;
                                            renderWriter.Write(line.Start);
                                            renderWriter.Write(line.End);
                                            renderWriter.Write(line.Thickness);
                                        }
                                        break;
                                    case RenderCommandType.Rect:
                                        {
                                            Rectangle rect = command as Rectangle;
                                            renderWriter.Write(rect.Location);
                                            renderWriter.Write(rect.Width);
                                            renderWriter.Write(rect.Height);
                                            renderWriter.Write(rect.StrokeThickness);
                                            renderWriter.Write((rect.Fill ? (uint)1 : (uint)0)); // 0 for transparent, 1 for filled
                                        }
                                        break;
                                    case RenderCommandType.Ellipse:
                                        {
                                            Ellipse ellipse = command as Ellipse;
                                            renderWriter.Write(ellipse.Centre);
                                            renderWriter.Write(ellipse.RadiusX);
                                            renderWriter.Write(ellipse.RadiusY);
                                            renderWriter.Write(ellipse.Thickness);
                                            renderWriter.Write((ellipse.Fill ? (uint)1 : (uint)0)); // 0 for transparent, 1 for filled
                                        }
                                        break;
                                    case RenderCommandType.Path:
                                        {
                                            RenderPath path = command as RenderPath;
                                            renderWriter.Write(path.Start);
                                            renderWriter.Write(path.Thickness);
                                            renderWriter.Write((path.Fill ? (uint)1 : (uint)0)); // 0 for transparent, 1 for filled

                                            renderWriter.Write(path.Commands.Count);
                                            foreach (IPathCommand pCommand in path.Commands)
                                            {
                                                renderWriter.Write((int)pCommand.Type);
                                                pCommand.Write(renderWriter);
                                            }
                                        }
                                        break;
                                    case RenderCommandType.Text:
                                        {
                                            var text = command as RenderText;
                                            renderWriter.Write(BinaryConstants.FormattedTextVersion); // Formatted text version
                                            renderWriter.Write(text.Location); // Text location
                                            renderWriter.Write((uint)text.Alignment); // Text alignment

                                            renderWriter.Write((uint)text.TextRuns.Count); // Number of text runs
                                            foreach (TextRun run in text.TextRuns)
                                            {
                                                renderWriter.Write((uint)run.Formatting.FormattingType); // Run formatting type
                                                renderWriter.Write(run.Formatting.Size); // Run text size
                                                renderWriter.Write(run.Text); // Text
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        writer.Write((ushort)BinaryConstants.ComponentSectionType.Render);
                        writer.Write((uint)renderStream.Length);
                        writer.Write(renderStream.ToArray());
                    }
                    #endregion

                    mainWriter.Write((ushort)BinaryConstants.ContentItemType.Component);
                    mainWriter.Write((uint)tempStream.Length);
                    mainWriter.Write(tempStream.ToArray());
                }
            }
        }

        private void WriteResources()
        {
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(m_resourcesStream);
            foreach (BinaryResource resource in Resources)
            {
                writer.Write((ushort)BinaryConstants.ContentItemType.Resource);
                resource.Write(writer);
            }
        }

        public void Write()
        {
            WriteDescriptions();
            WriteResources();

            MemoryStream tempStream = new MemoryStream();
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(tempStream);
            WriteHeader(writer);
            writer.Write(m_contentStream.ToArray());
            writer.Write(m_resourcesStream.ToArray());

            #region Old
            //if (Settings.Key.HasValue)
            //{
            //    // write signature
            //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //    string data = rsa.ToXmlString(false);
            //    rsa.ImportParameters(Settings.Key.Value);
            //    byte[] dataToSign = tempStream.ToArray();
            //    byte[] signature = rsa.SignData(dataToSign, new SHA1CryptoServiceProvider());
            //    writer.Write(signature);
            //}
            #endregion

            tempStream.WriteTo(m_finalStream);
        }

        public class BinaryWriterSettings
        {
            /// <summary>
            /// Don't embed icons.
            /// </summary>
            public bool IgnoreIcons { get; set; }
            public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate { get; set; }
        }
    }
}
