// BinaryLoader.cs
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
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description.Render;
using System.Security.Cryptography;
using CircuitDiagram.Render.Path;
using CircuitDiagram.Render;
using CircuitDiagram.Components.Description;
using System.Security.Cryptography.X509Certificates;

namespace CircuitDiagram.IO
{
    public enum CDDResourceType
    {
        None = 0,
        PNGImage = 1,
        BitmapImage = 2,
        JPEGImage = 3,
    }

    public class BinaryLoader : IComponentDescriptionLoader
    {
        private ComponentDescription[] m_descriptions;
        private BinaryResource[] m_resources;

        public ComponentDescription[] GetDescriptions()
        {
            return m_descriptions;
        }

        public BinaryResource[] GetResources()
        {
            return m_resources;
        }

        public bool Load(Stream stream)
        {
            return Load(stream, null);
        }

        public bool Load(Stream stream, X509Chain chain)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);

                // read header
                ulong magicNumber = reader.ReadUInt64();
                byte formatVersion = reader.ReadByte();
                byte[] md5hash = reader.ReadBytes(16);
                uint reserved = reader.ReadUInt32();
                uint fileLength = reader.ReadUInt32();
                uint offsetToContent = reader.ReadUInt32();
                uint numContentItems = reader.ReadUInt32();

                // Signing
                bool isSigned = reader.ReadBoolean();
                byte[] sha1Sig = null;
                byte[] certData = null;
                if (isSigned)
                {
                    int sigLength = reader.ReadInt32();
                    sha1Sig = reader.ReadBytes(sigLength);
                    int certLength = reader.ReadInt32();
                    certData = reader.ReadBytes(certLength);
                }

                if (reader.BaseStream.Position != offsetToContent)
                    reader.BaseStream.Seek(offsetToContent, SeekOrigin.Begin);

                bool validSignature = false;
                bool certificateTrusted = false;
                X509Certificate2 certificate = null;
                if (isSigned)
                {
                    certificate = new X509Certificate2(certData);
                    certificateTrusted = chain.Build(certificate);

                    byte[] buffer = new byte[stream.Length - stream.Position];
                    stream.Read(buffer, 0, buffer.Length);

                    var dsa = certificate.PublicKey.Key as RSACryptoServiceProvider;
                    validSignature = dsa.VerifyData(buffer, new SHA1CryptoServiceProvider(), sha1Sig);

                    reader.BaseStream.Seek(offsetToContent, SeekOrigin.Begin);
                }

                List<BinaryResource> resources = new List<BinaryResource>();
                List<ComponentDescription> descriptions = new List<ComponentDescription>();

                for (uint contentCounter = 0; contentCounter < numContentItems; contentCounter++)
                {
                    ushort itemType = reader.ReadUInt16();
                    uint itemLength = reader.ReadUInt32();

                    if (itemType == (ushort)BinaryConstants.ContentItemType.Resource)
                    {
                        uint resourceID = reader.ReadUInt32();
                        uint resourceMimeType = reader.ReadUInt32();
                        byte[] buffer = reader.ReadBytes((int)itemLength - 8);
                        resources.Add(new BinaryResource(resourceID, BinaryConstants.ResourceMineTypeToString(resourceMimeType), buffer));
                    }
                    else if (itemType == (ushort)BinaryConstants.ContentItemType.Component)
                    {
                        uint descriptionID = reader.ReadUInt32();
                        uint numSections = reader.ReadUInt32();

                        string componentName = null;
                        bool canResize = false;
                        bool canFlip = false;
                        double minSize = ComponentHelper.GridSize;
                        List<ComponentProperty> properties = new List<ComponentProperty>();
                        List<ConnectionGroup> connections = new List<ConnectionGroup>();
                        List<RenderDescription> renderDescriptions = new List<RenderDescription>();
                        List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
                        ComponentDescriptionMetadata descriptionMetadata = new ComponentDescriptionMetadata();

                        for (uint sectionCounter = 0; sectionCounter < numSections; sectionCounter++)
                        {
                            ushort sectionType = reader.ReadUInt16();
                            uint sectionLength = reader.ReadUInt32();

                            #region Metadata
                            if (sectionType == (uint)BinaryConstants.ComponentSectionType.Metadata)
                            {
                                componentName = reader.ReadString();
                                canResize = reader.ReadBoolean();
                                canFlip = reader.ReadBoolean();
                                minSize = reader.ReadDouble();
                                descriptionMetadata.Type = String.Format("Binary r{0} (*.cdcom)", formatVersion);
                                descriptionMetadata.GUID = new Guid(reader.ReadBytes(16));
                                descriptionMetadata.Author = reader.ReadString();
                                if (validSignature && certificate != null && certificateTrusted)
                                    descriptionMetadata.Author = certificate.GetNameInfo(X509NameType.EmailName, false);
                                descriptionMetadata.Version = new Version(reader.ReadUInt16(), reader.ReadUInt16());
                                descriptionMetadata.AdditionalInformation = reader.ReadString();
                                descriptionMetadata.ImplementSet = reader.ReadString();
                                descriptionMetadata.ImplementItem = reader.ReadString();
                                descriptionMetadata.Signature.IsHashValid = validSignature;
                                descriptionMetadata.Signature.Certificate = certificate;
                                descriptionMetadata.Signature.IsCertificateTrusted = certificateTrusted;
                                int iconResource = reader.ReadInt32();
                                if (iconResource != -1)
                                    descriptionMetadata.IconData = BitConverter.GetBytes(iconResource);
                                long created = reader.ReadInt64();
                            }
                            #endregion
                            #region Flags
                            else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Flags)
                            {
                                uint numFlagGroups = reader.ReadUInt32();
                                for (uint j = 0; j < numFlagGroups; j++)
                                {
                                    IConditionTreeItem conditions;
                                    if (formatVersion > 1)
                                        conditions = reader.ReadConditionTree();
                                    else
                                        conditions = reader.ReadConditionCollection();

                                    FlagOptions value = (FlagOptions)reader.ReadUInt32();
                                    flagOptions.Add(new Conditional<FlagOptions>(value, conditions));
                                }
                            }
                            #endregion
                            #region Properties
                            else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Properties)
                            {
                                uint numProperties = reader.ReadUInt32();
                                for (uint j = 0; j < numProperties; j++)
                                {
                                    string propertyName = reader.ReadString();
                                    string serializedName = reader.ReadString();
                                    string displayName = reader.ReadString();
                                    BinaryType propType;
                                    object defaultValue = reader.ReadType(out propType);
                                    string[] enumOptions = null;
                                    if (propType == BinaryType.Enum)
                                    {
                                        enumOptions = new string[reader.ReadInt32()];
                                        for (int k = 0; k < enumOptions.Length; k++)
                                            enumOptions[k] = reader.ReadString();
                                    }

                                    // Format rules
                                    List<ComponentPropertyFormat> formatRules = new List<ComponentPropertyFormat>();
                                    uint numFormatRules = reader.ReadUInt32();
                                    for (uint k = 0; k < numFormatRules; k++)
                                    {
                                        IConditionTreeItem conditions;
                                        if (formatVersion > 1)
                                            conditions = reader.ReadConditionTree();
                                        else
                                            conditions = reader.ReadConditionCollection();
                                        string formatRule = reader.ReadString();
                                        formatRules.Add(new ComponentPropertyFormat(formatRule, conditions));
                                    }

                                    // Other conditions
                                    uint numOtherConditions = reader.ReadUInt32();
                                    Dictionary<PropertyOtherConditionType, IConditionTreeItem> otherConditions = new Dictionary<PropertyOtherConditionType, IConditionTreeItem>((int)numOtherConditions);
                                    for (uint k = 0; k < numOtherConditions; k++)
                                    {
                                        uint uintConditionType = reader.ReadUInt32();
                                        IConditionTreeItem conditions;
                                        if (formatVersion > 1)
                                            conditions = reader.ReadConditionTree();
                                        else
                                            conditions = reader.ReadConditionCollection();
                                        PropertyOtherConditionType conditionType = (PropertyOtherConditionType)uintConditionType;
                                        otherConditions.Add(conditionType, conditions);
                                    }

                                    properties.Add(new ComponentProperty(propertyName, serializedName, displayName, BinaryIOExtentions.BinaryTypeToType(propType), defaultValue, formatRules.ToArray(), otherConditions, enumOptions));
                                }
                            }
                            #endregion
                            #region Configurations
                            else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Configurations)
                            {
                                uint numConfigurations = reader.ReadUInt32();
                                for (int j = 0; j < numConfigurations; j++)
                                {
                                    string configurationName = reader.ReadString();
                                    string implementationName = reader.ReadString();

                                    int numSetters = reader.ReadInt32();
                                    Dictionary<string, object> setters = new Dictionary<string, object>(numSetters);
                                    for (int k = 0; k < numSetters; k++)
                                    {
                                        BinaryType tempType;
                                        setters.Add(reader.ReadString(), reader.ReadType(out tempType));
                                    }

                                    int iconID = reader.ReadInt32();
                                    byte[] iconData = null;
                                    if (iconID != -1)
                                        iconData = BitConverter.GetBytes(iconID);
                                    descriptionMetadata.Configurations.Add(new ComponentConfiguration(implementationName, configurationName, setters) { IconData = iconData });
                                }
                            }
                            #endregion
                            #region Connections
                            else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Connections)
                            {
                                uint numConnectionGroups = reader.ReadUInt32();
                                List<ConnectionGroup> connectionGroups = new List<ConnectionGroup>();
                                for (int j = 0; j < numConnectionGroups; j++)
                                {
                                    IConditionTreeItem conditions;
                                    if (formatVersion > 1)
                                        conditions = reader.ReadConditionTree();
                                    else
                                        conditions = reader.ReadConditionCollection();

                                    List<ConnectionDescription> tConnections = new List<ConnectionDescription>();
                                    uint numConnections = reader.ReadUInt32();
                                    for (uint k = 0; k < numConnections; k++)
                                    {
                                        tConnections.Add(new ConnectionDescription(reader.ReadComponentPoint(), reader.ReadComponentPoint(), (ConnectionEdge)reader.ReadInt32(), reader.ReadString()));
                                    }

                                    connections.Add(new ConnectionGroup(conditions, tConnections.ToArray()));
                                }
                            }
                            #endregion
                            #region Render
                            else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Render)
                            {
                                uint numRenderGroups = reader.ReadUInt32();
                                for (uint j = 0; j < numRenderGroups; j++)
                                {
                                    IConditionTreeItem conditions;
                                    if (formatVersion > 1)
                                        conditions = reader.ReadConditionTree();
                                    else
                                        conditions = reader.ReadConditionCollection();

                                    int numRenderCommands = (int)reader.ReadUInt32();
                                    List<IRenderCommand> renderCommands = new List<IRenderCommand>(numRenderCommands);
                                    for (int k = 0; k < numRenderCommands; k++)
                                    {
                                        RenderCommandType commandType = (RenderCommandType)reader.ReadUInt32();
                                        switch (commandType)
                                        {
                                            case RenderCommandType.Line:
                                                {
                                                    ComponentPoint start = reader.ReadComponentPoint();
                                                    ComponentPoint end = reader.ReadComponentPoint();
                                                    double thickness = reader.ReadDouble();
                                                    renderCommands.Add(new Line(start, end, thickness));
                                                }
                                                continue;
                                            case RenderCommandType.Rect:
                                                {
                                                    ComponentPoint location = reader.ReadComponentPoint();
                                                    double width = reader.ReadDouble();
                                                    double height = reader.ReadDouble();
                                                    double thickness = reader.ReadDouble();
                                                    bool fill = (reader.ReadUInt32() == 0 ? false : true);
                                                    renderCommands.Add(new Rectangle(location, width, height, thickness, fill));
                                                }
                                                continue;
                                            case RenderCommandType.Ellipse:
                                                {
                                                    ComponentPoint centre = reader.ReadComponentPoint();
                                                    double radiusX = reader.ReadDouble();
                                                    double radiusY = reader.ReadDouble();
                                                    double thickness = reader.ReadDouble();
                                                    bool fill = (reader.ReadUInt32() == 0 ? false : true);
                                                    renderCommands.Add(new Ellipse(centre, radiusX, radiusY, thickness, fill));
                                                }
                                                continue;
                                            case RenderCommandType.Path:
                                                {
                                                    ComponentPoint start = reader.ReadComponentPoint();
                                                    double thickness = reader.ReadDouble();
                                                    bool fill = (reader.ReadUInt32() == 0 ? false : true);

                                                    int numCommands = reader.ReadInt32();
                                                    List<IPathCommand> pathCommands = new List<IPathCommand>(numCommands);
                                                    for (int l = 0; l < numCommands; l++)
                                                    {
                                                        CommandType pType = (CommandType)reader.ReadInt32();
                                                        IPathCommand theCommand = null;
                                                        switch (pType)
                                                        {
                                                            case CommandType.MoveTo:
                                                                theCommand = new MoveTo();
                                                                break;
                                                            case CommandType.LineTo:
                                                                theCommand = new LineTo();
                                                                break;
                                                            case CommandType.CurveTo:
                                                                theCommand = new CurveTo();
                                                                break;
                                                            case CommandType.EllipticalArcTo:
                                                                theCommand = new EllipticalArcTo();
                                                                break;
                                                            case CommandType.QuadraticBeizerCurveTo:
                                                                theCommand = new QuadraticBeizerCurveTo();
                                                                break;
                                                            case CommandType.SmoothCurveTo:
                                                                theCommand = new SmoothCurveTo();
                                                                break;
                                                            case CommandType.SmoothQuadraticBeizerCurveTo:
                                                                theCommand = new SmoothQuadraticBeizerCurveTo();
                                                                break;
                                                            default:
                                                                theCommand = new ClosePath();
                                                                break;
                                                        }
                                                        theCommand.Read(reader);
                                                        pathCommands.Add(theCommand);
                                                    }

                                                    renderCommands.Add(new RenderPath(start, thickness, fill, pathCommands));
                                                }
                                                continue;
                                            case RenderCommandType.Text:
                                                {
                                                    byte formattedTextVersion = reader.ReadByte();
                                                    ComponentPoint location = reader.ReadComponentPoint();
                                                    TextAlignment alignment = (TextAlignment)reader.ReadUInt32();

                                                    uint numTextRuns = reader.ReadUInt32();
                                                    List<TextRun> textRuns = new List<TextRun>((int)numTextRuns);
                                                    for (uint l = 0; l < numTextRuns; l++)
                                                    {
                                                        TextRunFormattingType formattingType = (TextRunFormattingType)reader.ReadUInt32();
                                                        double runSize = reader.ReadDouble();
                                                        string runText = reader.ReadString();
                                                        textRuns.Add(new TextRun(runText, new TextRunFormatting(formattingType, runSize)));
                                                    }

                                                    renderCommands.Add(new Text(location, alignment, textRuns));
                                                }
                                                continue;
                                        }
                                    }

                                    renderDescriptions.Add(new RenderDescription(conditions, renderCommands.ToArray()));
                                }
                            }
                            #endregion
                            #region Skip
                            else
                            {
                                // Unknown type - skip
                                reader.BaseStream.Seek(sectionLength, SeekOrigin.Current);
                            }
                            #endregion
                        }

                        descriptions.Add(new ComponentDescription(descriptionID.ToString(), componentName, canResize, canFlip, minSize, properties.ToArray(), connections.ToArray(), renderDescriptions.ToArray(), flagOptions.ToArray(), descriptionMetadata));
                    }
                    else
                    {
                        // Unknown type - skip
                        reader.BaseStream.Seek(itemLength, SeekOrigin.Current);
                    }
                }

                // Load proper icons
                foreach (ComponentDescription description in descriptions)
                {
                    if (description.Metadata.IconData != null && description.Metadata.Icon == null)
                    {
                        int resourceId = BitConverter.ToInt32(description.Metadata.IconData, 0);
                        BinaryResource iconResource = resources.First(resource => resource.ID == resourceId);
                        if (iconResource != null)
                        {
                            description.Metadata.IconData = iconResource.Buffer;
                            description.Metadata.IconMimeType = iconResource.ResourceType;

                            if (ComponentHelper.LoadIcon != null)
                                description.Metadata.Icon = ComponentHelper.LoadIcon(iconResource.Buffer, iconResource.ResourceType);
                        }
                    }

                    foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                    {
                        if (configuration.IconData != null && configuration.Icon == null)
                        {
                            int resourceId = BitConverter.ToInt32(configuration.IconData, 0);
                            BinaryResource iconResource = resources.First(resource => resource.ID == resourceId);
                            if (iconResource != null)
                            {
                                configuration.IconData = iconResource.Buffer;
                                configuration.IconMimeType = iconResource.ResourceType;
                                /*MemoryStream tempStream = new MemoryStream(iconResource.Buffer);
                                var tempIcon = new System.Windows.Media.Imaging.BitmapImage();
                                tempIcon.BeginInit();
                                tempIcon.StreamSource = tempStream;
                                tempIcon.EndInit();*/
                                if (ComponentHelper.LoadIcon != null)
                                    configuration.Icon = ComponentHelper.LoadIcon(iconResource.Buffer, iconResource.ResourceType);
                            }
                        }
                    }
                }

                m_resources = resources.ToArray();
                m_descriptions = descriptions.ToArray();

                return true;
            }
            catch (Exception)
            {
                // Invalid binary file
                return false;
            }
        }
    }
}
