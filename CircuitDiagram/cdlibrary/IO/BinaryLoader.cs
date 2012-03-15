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
using CircuitDiagram.Components.Render;
using System.Security.Cryptography;
using CircuitDiagram.Components.Render.Path;

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

        public bool Load(Stream stream, RSAParameters? key)
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
            bool isSigned = reader.ReadBoolean();
            byte[] sha1Sig = null;
            if (isSigned)
                sha1Sig = reader.ReadBytes(128);

            if (reader.BaseStream.Position != offsetToContent)
                reader.BaseStream.Seek(offsetToContent, SeekOrigin.Begin);

            bool validSignature = false;
            if (isSigned)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(key.Value);
                byte[] buffer = new byte[stream.Length - stream.Position];
                stream.Read(buffer, 0, buffer.Length);
                validSignature = rsa.VerifyData(buffer, new SHA1CryptoServiceProvider(), sha1Sig);

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
                            descriptionMetadata.GUID = new Guid(reader.ReadBytes(16));
                            descriptionMetadata.Author = reader.ReadString();
                            descriptionMetadata.Version = new Version(reader.ReadUInt16(), reader.ReadUInt16());
                            descriptionMetadata.AdditionalInformation = reader.ReadString();
                            descriptionMetadata.ImplementSet = reader.ReadString();
                            descriptionMetadata.ImplementItem = reader.ReadString();
                            int iconResource = reader.ReadInt32();
                            if (iconResource != -1)
                                descriptionMetadata.IconData = BitConverter.GetBytes(iconResource);
                        }
                        #endregion
                        #region Flags
                        else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Flags)
                        {
                            uint numFlagGroups = reader.ReadUInt32();
                            for (uint j = 0; j < numFlagGroups; j++)
                            {
                                ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                                int numConditions = reader.ReadInt32();
                                for (int k = 0; k < numConditions; k++)
                                {
                                    ConditionType type = (ConditionType)reader.ReadUInt32();
                                    ConditionComparison comparison = (ConditionComparison)reader.ReadUInt32();
                                    string variableName = reader.ReadString();
                                    BinaryType conditionType;
                                    object compareTo = reader.ReadType(out conditionType);
                                    conditions.Add(new ComponentDescriptionCondition(type, variableName, comparison, compareTo));
                                }

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
                                    ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                                    int numConditions = reader.ReadInt32();
                                    for (int l = 0; l < numConditions; l++)
                                    {
                                        ConditionType conditionType = (ConditionType)reader.ReadUInt32();
                                        ConditionComparison comparison = (ConditionComparison)reader.ReadUInt32();
                                        string variableName = reader.ReadString();
                                        BinaryType binType;
                                        object compareTo = reader.ReadType(out binType);
                                        conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                                    }

                                    string formatRule = reader.ReadString();
                                    formatRules.Add(new ComponentPropertyFormat(formatRule, conditions));
                                }

                                // Other conditions
                                uint numOtherConditions = reader.ReadUInt32();
                                Dictionary<PropertyOtherConditionType, ComponentDescriptionConditionCollection> otherConditions = new Dictionary<PropertyOtherConditionType, ComponentDescriptionConditionCollection>((int)numOtherConditions);
                                for (uint k = 0; k < numOtherConditions; k++)
                                {
                                    uint uintConditionType = reader.ReadUInt32();
                                    ComponentDescriptionConditionCollection conditions = reader.ReadConditionCollection();

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
                                ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                                uint numConditions = reader.ReadUInt32();
                                for (uint l = 0; l < numConditions; l++)
                                {
                                    ConditionType conditionType = (ConditionType)reader.ReadUInt32();
                                    ConditionComparison comparison = (ConditionComparison)reader.ReadUInt32();
                                    string variableName = reader.ReadString();
                                    BinaryType binType;
                                    object compareTo = reader.ReadType(out binType);
                                    conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                                }

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
                                ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                                int numConditions = reader.ReadInt32();
                                for (int l = 0; l < numConditions; l++)
                                {
                                    ConditionType conditionType = (ConditionType)reader.ReadUInt32();
                                    ConditionComparison comparison = (ConditionComparison)reader.ReadUInt32();
                                    string variableName = reader.ReadString();
                                    BinaryType binType;
                                    object compareTo = reader.ReadType(out binType);
                                    conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                                }

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

                                                renderCommands.Add(new CircuitDiagram.Components.Render.Path.Path(start, thickness, fill, pathCommands));
                                            }
                                            continue;
                                        case RenderCommandType.Text:
                                            {
                                                ComponentPoint location = reader.ReadComponentPoint();
                                                TextAlignment alignment = (TextAlignment)reader.ReadUInt32();
                                                double size = reader.ReadDouble();
                                                string value = reader.ReadString();
                                                renderCommands.Add(new Text(location, alignment, size, value));
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

            #region Old
            /*
            // Read header
            int magicNumber = reader.ReadInt32();
            int formatVersion = reader.ReadInt32();
            int compatibilityVersion = reader.ReadInt32();
            CDDBinaryHeaderFlags headerFlags = (CDDBinaryHeaderFlags)reader.ReadUInt32();
            uint contentOffset = reader.ReadUInt32();
            uint resourcesOffset = reader.ReadUInt32();
            bool isSigned = reader.ReadBoolean(); // is signed
            // [space for additional header data]

            // Load resources
            if (resourcesOffset != 0)
            {
                reader.BaseStream.Seek(resourcesOffset, SeekOrigin.Begin);
                int resourcesFlags = reader.ReadInt32(); // not supported
                int numResources = reader.ReadInt32();
                m_resources = new BinaryResource[numResources];
                for (int i = 0; i < numResources; i++)
                {
                    string resourceId = reader.ReadString();
                    string resourceType = reader.ReadString();
                    int bufferLength = reader.ReadInt32();
                    byte[] buffer = new byte[bufferLength];
                    reader.Read(buffer, 0, bufferLength);

                    m_resources[i] = new BinaryResource(resourceId, resourceType, buffer);
                }
            }

            // Load descriptions
            if (contentOffset != 0)
            {
                reader.BaseStream.Seek(contentOffset, SeekOrigin.Begin);
                CDDBinaryContentFlags contentFlags = (CDDBinaryContentFlags)reader.ReadInt32();
                int numComponentDescriptions = reader.ReadInt32();
                BinaryReader contentReader = reader;
                if ((contentFlags & CDDBinaryContentFlags.Compress) == CDDBinaryContentFlags.Compress)
                    contentReader = new BinaryReader(new DeflateStream(stream, CompressionMode.Decompress));
                List<ComponentDescription> descriptions = new List<ComponentDescription>();
                for (int i = 0; i < numComponentDescriptions; i++)
                {
                    ComponentDescriptionMetadata metadata = new ComponentDescriptionMetadata();

                    string descriptionID = reader.ReadString();

                    #region Metadata
                    // Read METADATA
                    int numMetadataProperties = reader.ReadInt32();
                    Dictionary<MetadataProperty, object> metadataProperties = new Dictionary<MetadataProperty, object>();
                    for (int j = 0; j < numMetadataProperties; j++)
                    {
                        MetadataProperty property = (MetadataProperty)reader.ReadInt32();
                        switch (property)
                        {
                            case MetadataProperty.ComponentName:
                                metadataProperties.Add(property, reader.ReadString());
                                break;
                            case MetadataProperty.CanResize:
                                metadataProperties.Add(property, reader.ReadBoolean());
                                break;
                            case MetadataProperty.CanFlip:
                                metadataProperties.Add(property, reader.ReadBoolean());
                                break;
                            case MetadataProperty.MinSize:
                                metadataProperties.Add(property, reader.ReadDouble());
                                break;
                            case MetadataProperty.IconResourceID:
                                {
                                    string iconResourceID = reader.ReadString();
                                    try
                                    {
                                        BinaryResource iconResource = m_resources.First(resource => resource.ID == iconResourceID);
                                        if (iconResource.ResourceType == "image/png")
                                        {
                                            using (MemoryStream tempStream = new MemoryStream(iconResource.Buffer))
                                            {
                                                var tempIcon = new System.Windows.Media.Imaging.BitmapImage();
                                                tempIcon.BeginInit();
                                                tempIcon.StreamSource = tempStream;
                                                tempIcon.EndInit();
                                                metadata.Icon = tempIcon;
                                            }
                                            metadata.IconMimeType = "image/png";
                                            metadata.IconData = iconResource.Buffer;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Invalid icon
                                    }
                                }
                                break;
                            case MetadataProperty.GUID:
                                metadata.GUID = new Guid(reader.ReadBytes(16));
                                break;
                            case MetadataProperty.Author:
                                metadata.Author = reader.ReadString();
                                break;
                            case MetadataProperty.AdditionalInformation:
                                metadata.AdditionalInformation = reader.ReadString();
                                break;
                            case MetadataProperty.Version:
                                metadata.Version = new Version(reader.ReadString());
                                break;
                            case MetadataProperty.ImplementSet:
                                metadata.ImplementSet = reader.ReadString();
                                break;
                            case MetadataProperty.ImplementItem:
                                metadata.ImplementItem = reader.ReadString();
                                break;
                        }
                    }

                    string name = metadataProperties[MetadataProperty.ComponentName] as string;
                    bool canResize = (bool)metadataProperties[MetadataProperty.CanResize];
                    bool canFlip = (bool)metadataProperties[MetadataProperty.CanFlip];
                    double minSize = (double)metadataProperties[MetadataProperty.MinSize];
                    #endregion

                    #region Flags
                    // Read FLAGS
                    uint numFlagGroups = contentReader.ReadUInt32();
                    List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
                    for (uint j = 0; j < numFlagGroups; j++)
                    {
                        ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                        int numConditions = contentReader.ReadInt32();
                        for (int k = 0; k < numConditions; k++)
                        {
                            ConditionType type = (ConditionType)contentReader.ReadUInt32();
                            ConditionComparison comparison = (ConditionComparison)contentReader.ReadUInt32();
                            string variableName = contentReader.ReadString();
                            BinaryType conditionType;
                            object compareTo = contentReader.ReadType(out conditionType);
                            conditions.Add(new ComponentDescriptionCondition(type, variableName, comparison, compareTo));
                        }

                        FlagOptions value = (FlagOptions)contentReader.ReadUInt32();
                        flagOptions.Add(new Conditional<FlagOptions>(value, conditions));
                    }
                    #endregion

                    #region Properties
                    // Read PROPERTIES
                    uint numProperties = contentReader.ReadUInt32();
                    List<ComponentProperty> properties = new List<ComponentProperty>();
                    for (uint j = 0; j < numProperties; j++)
                    {
                        string propertyName = contentReader.ReadString();
                        string serializedName = contentReader.ReadString();
                        string displayName = contentReader.ReadString();
                        BinaryType propType;
                        object defaultValue = contentReader.ReadType(out propType);
                        string[] enumOptions = null;
                        if (propType == BinaryType.Enum)
                        {
                            enumOptions = new string[contentReader.ReadInt32()];
                            for (int k = 0; k < enumOptions.Length; k++)
                                enumOptions[k] = contentReader.ReadString();
                        }

                        // Format rules
                        List<ComponentPropertyFormat> formatRules = new List<ComponentPropertyFormat>();
                        int numFormatRules = contentReader.ReadInt32();
                        for (int k = 0; k < numFormatRules; k++)
                        {
                            ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                            int numConditions = contentReader.ReadInt32();
                            for (int l = 0; l < numConditions; l++)
                            {
                                ConditionType conditionType = (ConditionType)contentReader.ReadUInt32();
                                ConditionComparison comparison = (ConditionComparison)contentReader.ReadUInt32();
                                string variableName = contentReader.ReadString();
                                BinaryType binType;
                                object compareTo = contentReader.ReadType(out binType);
                                conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                            }

                            string formatRule = contentReader.ReadString();
                            formatRules.Add(new ComponentPropertyFormat(formatRule, conditions));
                        }

                        properties.Add(new ComponentProperty(propertyName, serializedName, displayName, BinaryIOExtentions.BinaryTypeToType(propType), defaultValue, formatRules.ToArray(), enumOptions));
                    }
                    #endregion

                    #region Configurations
                    // Read CONFIGURATIONS
                    int numConfigurations = reader.ReadInt32();
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

                        string iconID = reader.ReadString();
                        string iconMimeType = null;
                        byte[] iconData = null;
                        BitmapSource icon = null;
                        if (!String.IsNullOrEmpty(iconID))
                        {
                            BinaryResource iconResource = m_resources.First(resource => resource.ID == iconID);
                            if (iconResource.ResourceType == "image/png")
                            {
                                using (MemoryStream tempStream = new MemoryStream(iconResource.Buffer))
                                {
                                    PngBitmapDecoder decoder = new PngBitmapDecoder(tempStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                    icon = decoder.Frames[0];
                                }
                                iconMimeType = iconResource.ResourceType;
                                iconData = iconResource.Buffer;
                            }
                        }
                        metadata.Configurations.Add(new ComponentConfiguration(implementationName, configurationName, setters) { Icon = icon, IconMimeType = iconMimeType, IconData = iconData });
                    }
                    #endregion

                    #region Connections
                    // Read CONNECTIONS
                    int numConnectionGroups = contentReader.ReadInt32();
                    List<ConnectionGroup> connectionGroups = new List<ConnectionGroup>();
                    for (int j = 0; j < numConnectionGroups; j++)
                    {
                        ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                        int numConditions = contentReader.ReadInt32();
                        for (int l = 0; l < numConditions; l++)
                        {
                            ConditionType conditionType = (ConditionType)contentReader.ReadUInt32();
                            ConditionComparison comparison = (ConditionComparison)contentReader.ReadUInt32();
                            string variableName = contentReader.ReadString();
                            BinaryType binType;
                            object compareTo = contentReader.ReadType(out binType);
                            conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                        }

                        int numConnections = reader.ReadInt32();
                        List<ConnectionDescription> connections = new List<ConnectionDescription>();
                        for (int k = 0; k < numConnections; k++)
                        {
                            connections.Add(new ConnectionDescription(reader.ReadComponentPoint(), reader.ReadComponentPoint(), (ConnectionEdge)reader.ReadInt32(), reader.ReadString()));
                        }

                        connectionGroups.Add(new ConnectionGroup(conditions, connections.ToArray()));
                    }
                    #endregion

                    #region Render
                    // Read RENDER
                    uint numRenderGroups = contentReader.ReadUInt32();
                    List<RenderDescription> renderDescriptions = new List<RenderDescription>();
                    for (uint j = 0; j < numRenderGroups; j++)
                    {
                        ComponentDescriptionConditionCollection conditions = new ComponentDescriptionConditionCollection();
                        int numConditions = contentReader.ReadInt32();
                        for (int l = 0; l < numConditions; l++)
                        {
                            ConditionType conditionType = (ConditionType)contentReader.ReadUInt32();
                            ConditionComparison comparison = (ConditionComparison)contentReader.ReadUInt32();
                            string variableName = contentReader.ReadString();
                            BinaryType binType;
                            object compareTo = contentReader.ReadType(out binType);
                            conditions.Add(new ComponentDescriptionCondition(conditionType, variableName, comparison, compareTo));
                        }

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
                                        System.Windows.Media.Color fillColour = (reader.ReadUInt32() == 0 ? System.Windows.Media.Colors.Transparent : System.Windows.Media.Colors.Black);
                                        renderCommands.Add(new Rectangle(location, width, height, thickness, fillColour));
                                    }
                                    continue;
                                case RenderCommandType.Ellipse:
                                    {
                                        ComponentPoint centre = reader.ReadComponentPoint();
                                        double radiusX = reader.ReadDouble();
                                        double radiusY = reader.ReadDouble();
                                        double thickness = reader.ReadDouble();
                                        System.Windows.Media.Color fillColour = (reader.ReadUInt32() == 0 ? System.Windows.Media.Colors.Transparent : System.Windows.Media.Colors.Black);
                                        renderCommands.Add(new Ellipse(centre, radiusX, radiusY, thickness, fillColour));
                                    }
                                    continue;
                                case RenderCommandType.Path:
                                    {
                                        ComponentPoint start = reader.ReadComponentPoint();
                                        double thickness = reader.ReadDouble();
                                        System.Windows.Media.Color fillColour = (reader.ReadUInt32() == 0 ? System.Windows.Media.Colors.Transparent : System.Windows.Media.Colors.Black);

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

                                        renderCommands.Add(new CircuitDiagram.Components.Render.Path.Path(start, thickness, fillColour, pathCommands));
                                    }
                                    continue;
                                case RenderCommandType.Text:
                                    {
                                        ComponentPoint location = reader.ReadComponentPoint();
                                        TextAlignment alignment = (TextAlignment)reader.ReadUInt32();
                                        double size = reader.ReadDouble();
                                        string value = reader.ReadString();
                                        renderCommands.Add(new Text(location, alignment, size, value));
                                    }
                                    continue;
                            }
                        }

                        renderDescriptions.Add(new RenderDescription(conditions, renderCommands.ToArray()));
                    }
                    #endregion

                    metadata.Type = "Binary (*.cdcom)";
                    descriptions.Add(new ComponentDescription(descriptionID, name, canResize, canFlip, minSize, properties.ToArray(), connectionGroups.ToArray(), renderDescriptions.ToArray(), flagOptions.ToArray(), metadata));
                }
                m_descriptions = descriptions.ToArray();
            }

            if (isSigned && key != null)
            {
                // signature is the last 10 bytes
                reader.BaseStream.Seek(-128, SeekOrigin.End);
                byte[] signature = new byte[128];
                reader.Read(signature, 0, 128);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                byte[] data = reader.ReadBytes((int)reader.BaseStream.Length - 128);
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(key.Value);
                bool verified = rsa.VerifyData(data, new SHA1CryptoServiceProvider(), signature);

                if (verified)
                    foreach (ComponentDescription description in m_descriptions)
                        description.Metadata.Signed = true;
            }
            */
            #endregion

            return true;
        }
    }
}
