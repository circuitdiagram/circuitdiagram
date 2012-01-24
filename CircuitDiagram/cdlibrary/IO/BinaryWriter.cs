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
using CircuitDiagram.Components.Render;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CircuitDiagram.Components.Render.Path;
using BW = System.IO.BinaryWriter;

namespace CircuitDiagram.IO
{
    public static class BinaryConstants
    {
        public const byte FormatVersion = 1;

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

        public static string ResourceMineTypeToString(uint type)
        {
            try
            {
                switch ((ResourceMimeType)type)
                {
                    case ResourceMimeType.Image_Png:
                        return "image/png";
                    case ResourceMimeType.Image_Jpeg:
                        return System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    case ResourceMimeType.Image_Gif:
                        return System.Net.Mime.MediaTypeNames.Image.Gif;
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
                if (Settings.Key.HasValue)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.ImportParameters(Settings.Key.Value);
                    sha1signature = rsa.SignData(allContentStream, new SHA1CryptoServiceProvider());
                }

                writer.Write(new byte[] { 184, 67, 68, 67, 79, 77, 13, 10 }); // magic number ( 184, c, d, c, o, m, \r, \n )
                writer.Write(BinaryConstants.FormatVersion); // format version
                writer.Write(md5hash); // MD5 hash of content (16-bytes)
                writer.Write(0u); // flags/reserved
                writer.Write((uint)((Settings.Key.HasValue ? 170u : 42u) + m_contentStream.Length + m_resourcesStream.Length)); // length of file including header
                writer.Write((Settings.Key.HasValue ? 170u : 42u)); // offset to data from start of file (in bytes)
                writer.Write((uint)(Descriptions.Count + Resources.Count)); // number of content items
                writer.Write(Settings.Key.HasValue); // is signed
                if (Settings.Key.HasValue)
                    writer.Write(sha1signature); // SHA-1 (RSA) signature of content (128-bytes)
            }
        }

        public uint NewResourceID()
        {
            uint counter = 0;
            while (Resources.Exists(item => item.ID == counter))
                counter++;
            return counter;
        }

        public void WriteDescriptions()
        {
            BW mainWriter = new BW(m_contentStream);

            foreach (ComponentDescription description in Descriptions)
            {
                using (MemoryStream tempStream = new MemoryStream())
                {
                    BW writer = new BW(tempStream);

#warning "Proper component ID"
                    writer.Write(0u); // ID - FIX!
                    writer.Write(6u); // 6 sections

                    #region Metadata
                    // Write METADATA
                    /* Component name (string)
     *                  Can resize (bool)
     *                  Minimum size (bool)
     *                  GUID (byte[16])
     *                  Author (string)
     *                  Version (major, minor) (uint, uint)
     *                  AdditionalInformation (string)
     *                  Implement set (string)
     *                  Implement item (string)
     *                  IconresourceID (int) */
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
                        if (description.Metadata.IconData != null)
                        {
                            uint iconResourceID = NewResourceID();
                            Resources.Add(new BinaryResource(iconResourceID, description.Metadata.IconMimeType, description.Metadata.IconData));
                            metadataWriter.Write((int)iconResourceID);
                        }
                        else
                            metadataWriter.Write(-1);

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
                            flagsWriter.Write(flags.Conditions.Count); // number of conditions
                            foreach (ComponentDescriptionCondition condition in flags.Conditions)
                            {
                                flagsWriter.Write((uint)condition.Type);
                                flagsWriter.Write((uint)condition.Comparison);
                                flagsWriter.Write(condition.VariableName);
                                flagsWriter.WriteType(condition.CompareTo);
                            }

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
                            propertiesWriter.Write(property.SerializedName);
                            propertiesWriter.Write(property.DisplayName);
                            propertiesWriter.WriteType(property.Default, property.EnumOptions != null);
                            if (property.EnumOptions != null)
                            {
                                propertiesWriter.Write(property.EnumOptions.Length);
                                foreach (string option in property.EnumOptions)
                                    propertiesWriter.Write(option);
                            }

                            propertiesWriter.Write(property.FormatRules.Length);
                            foreach (ComponentPropertyFormat formatRule in property.FormatRules)
                            {
                                propertiesWriter.Write(formatRule.Conditions.Count);
                                foreach (ComponentDescriptionCondition condition in formatRule.Conditions)
                                {
                                    propertiesWriter.Write((uint)condition.Type);
                                    propertiesWriter.Write((uint)condition.Comparison);
                                    propertiesWriter.Write(condition.VariableName);
                                    propertiesWriter.WriteType(condition.CompareTo);
                                }

                                propertiesWriter.Write(formatRule.Value);
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
                            configurationWriter.Write(configuration.ImplementationName);

                            configurationWriter.Write(configuration.Setters.Count);
                            foreach (KeyValuePair<string, object> setter in configuration.Setters)
                            {
                                foreach (ComponentProperty property in description.Properties)
                                    if (property.SerializedName == setter.Key)
                                    {
                                        configurationWriter.Write(property.SerializedName);
                                        break;
                                    }
                                configurationWriter.WriteType(setter.Value);
                            }

                            if (!Settings.IgnoreIcons && configuration.IconData != null && configuration.IconMimeType != null)
                            {
                                uint resourceID = NewResourceID();
                                Resources.Add(new BinaryResource(resourceID, configuration.IconMimeType, configuration.IconData));
                                configurationWriter.Write((int)resourceID);
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
                            connectionsWriter.Write((uint)connectionGroup.Conditions.Count);
                            foreach (ComponentDescriptionCondition condition in connectionGroup.Conditions)
                            {
                                connectionsWriter.Write((uint)condition.Type);
                                connectionsWriter.Write((uint)condition.Comparison);
                                connectionsWriter.Write(condition.VariableName);
                                connectionsWriter.WriteType(condition.CompareTo);
                            }

                            connectionsWriter.Write((uint)connectionGroup.Value.Length);
                            foreach (ConnectionDescription connection in connectionGroup.Value)
                            {
                                connectionsWriter.Write(connection.Start);
                                connectionsWriter.Write(connection.End);
                                connectionsWriter.Write((int)connection.Edge);
                                connectionsWriter.Write(connection.Name); // NEW
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
                            renderWriter.Write(renderDescription.Conditions.Count);
                            foreach (ComponentDescriptionCondition condition in renderDescription.Conditions)
                            {
                                renderWriter.Write((uint)condition.Type);
                                renderWriter.Write((uint)condition.Comparison);
                                renderWriter.Write(condition.VariableName);
                                renderWriter.WriteType(condition.CompareTo);
                            }

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
                                            renderWriter.Write((rect.FillColour == System.Windows.Media.Colors.Transparent ? (uint)0 : (uint)1)); // 0 for transparent, 1 for filled
                                        }
                                        break;
                                    case RenderCommandType.Ellipse:
                                        {
                                            Ellipse ellipse = command as Ellipse;
                                            renderWriter.Write(ellipse.Centre);
                                            renderWriter.Write(ellipse.RadiusX);
                                            renderWriter.Write(ellipse.RadiusY);
                                            renderWriter.Write(ellipse.Thickness);
                                            renderWriter.Write((ellipse.FillColour == System.Windows.Media.Colors.Transparent ? (uint)0 : (uint)1)); // 0 for transparent, 1 for filled
                                        }
                                        break;
                                    case RenderCommandType.Path:
                                        {
                                            CircuitDiagram.Components.Render.Path.Path path = command as CircuitDiagram.Components.Render.Path.Path;
                                            renderWriter.Write(path.Start);
                                            renderWriter.Write(path.Thickness);
                                            renderWriter.Write((path.FillColour == System.Windows.Media.Colors.Transparent ? (uint)0 : (uint)1)); // 0 for transparent, 1 for filled

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
                                            Text text = command as Text;
                                            renderWriter.Write(text.Location);
                                            renderWriter.Write((uint)text.Alignment);
                                            renderWriter.Write(text.Size);
                                            renderWriter.Write(text.Value);
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

        public void WriteResources()
        {
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(m_resourcesStream);
            foreach (BinaryResource resource in Resources)
            {
                using (MemoryStream resourceStream = new MemoryStream())
                {
                    BW resourceWriter = new BW(resourceStream);
                    resourceWriter.Write(resource.ID);
                    resourceWriter.Write(0u);
                    resourceWriter.Write(resource.Buffer);

                    writer.Write((ushort)BinaryConstants.ContentItemType.Resource);
                    writer.Write((uint)resourceStream.Length);
                    writer.Write(resourceStream.ToArray());
                }
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
            public bool IgnoreIcons { get; set; }
            public RSAParameters? Key { get; set; }
            public bool Compress { get; private set; }
        }
    }
}
