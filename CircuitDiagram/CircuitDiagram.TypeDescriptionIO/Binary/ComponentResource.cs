using CircuitDiagram.Components;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Path;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Util;
using ComponentConfiguration = CircuitDiagram.TypeDescription.ComponentConfiguration;

namespace CircuitDiagram.TypeDescriptionIO.Binary
{
    public class ComponentResource : BinaryDescriptionContentItem
    {
        public uint? MainIconResource { get; private set; }
        Dictionary<ComponentConfiguration, uint> iconResources = new Dictionary<ComponentConfiguration, uint>();

        /// <summary>
        /// Gets the binary content item type ID for this item.
        /// </summary>
        public override BinaryConstants.ContentItemType ItemType => BinaryConstants.ContentItemType.Component;

        /// <summary>
        /// Gets or sets the component description contained within this resource.
        /// </summary>
        public ComponentDescription ComponentDescription { get; set; }

        internal override void Read(System.IO.BinaryReader reader, BinaryReadInfo readInfo)
        {
            uint length = reader.ReadUInt32();

            ID = reader.ReadUInt32();
            uint numSections = reader.ReadUInt32();

            string componentName = null;
            bool canResize = false;
            bool canFlip = false;
            double minSize = 10.0;
            List<ComponentProperty> properties = new List<ComponentProperty>();
            List<ConnectionGroup> connections = new List<ConnectionGroup>();
            List<RenderDescription> renderDescriptions = new List<RenderDescription>();
            List<Conditional<FlagOptions>> flagOptions = new List<Conditional<FlagOptions>>();
            ComponentDescriptionMetadata descriptionMetadata = new ComponentDescriptionMetadata();
            uint? iconResourceId = null;

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
                    descriptionMetadata.Type = String.Format("Binary r{0} (*.cdcom)", readInfo.FormatVersion);
                    descriptionMetadata.GUID = new Guid(reader.ReadBytes(16));
                    descriptionMetadata.Author = reader.ReadString();
                    if (readInfo.IsSignatureValid && readInfo.Certificate != null && readInfo.IsCertificateTrusted)
                        descriptionMetadata.Author = readInfo.Certificate.GetNameInfo(X509NameType.EmailName, false);
                    descriptionMetadata.Version = new Version(reader.ReadUInt16(), reader.ReadUInt16());
                    descriptionMetadata.AdditionalInformation = reader.ReadString();
                    descriptionMetadata.ImplementSet = reader.ReadString();
                    descriptionMetadata.ImplementItem = reader.ReadString();
                    int iconResource = reader.ReadInt32();
                    if (iconResource != -1)
                        iconResourceId = (uint)iconResource;
                    long created = reader.ReadInt64();
                    descriptionMetadata.Created = DateTime.FromBinary(created);
                }
                #endregion
                #region Flags
                else if (sectionType == (uint)BinaryConstants.ComponentSectionType.Flags)
                {
                    uint numFlagGroups = reader.ReadUInt32();
                    for (uint j = 0; j < numFlagGroups; j++)
                    {
                        IConditionTreeItem conditions;
                        if (readInfo.FormatVersion > 1)
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
                        object rawDefaultValue = reader.ReadType(out propType);
                        PropertyValue defaultValue = propType.ToPropertyUnion(rawDefaultValue);
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
                            if (readInfo.FormatVersion > 1)
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
                            if (readInfo.FormatVersion > 1)
                                conditions = reader.ReadConditionTree();
                            else
                                conditions = reader.ReadConditionCollection();
                            PropertyOtherConditionType conditionType = (PropertyOtherConditionType)uintConditionType;
                            otherConditions.Add(conditionType, conditions);
                        }

                        properties.Add(new ComponentProperty(propertyName, serializedName, displayName, BinaryIOExtentions.BinaryTypeToPropertyType(propType), defaultValue, formatRules.ToArray(), otherConditions, enumOptions));
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
                        var setters = new Dictionary<PropertyName, PropertyValue>(numSetters);
                        for (int k = 0; k < numSetters; k++)
                        {
                            BinaryType tempType;
                            string name = reader.ReadString();
                            var setterValue = reader.ReadType(out tempType);
                            setters.Add(name, tempType.ToPropertyUnion(setterValue));
                        }

                        int iconID = reader.ReadInt32();

                        var configuration = new ComponentConfiguration(implementationName, configurationName, setters);
                        descriptionMetadata.Configurations.Add(configuration);

                        if (iconID != -1)
                            iconResources.Add(configuration, (uint)iconID);
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
                        if (readInfo.FormatVersion > 1)
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
                        if (readInfo.FormatVersion > 1)
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

                                        renderCommands.Add(new RenderText(location, alignment, textRuns));
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

            ComponentDescription = new ComponentDescription(ID.ToString(), componentName, minSize, properties.ToArray(), connections.ToArray(), renderDescriptions.ToArray(), flagOptions.ToArray(), descriptionMetadata);

            if (canFlip)
                ComponentDescription.SetDefaultFlag(FlagOptions.FlipPrimary, true);
            if (!canResize)
                ComponentDescription.SetDefaultFlag(FlagOptions.NoResize, true);

            if (iconResourceId.HasValue)
                MainIconResource = iconResourceId.Value;
        }

        internal override void Write(System.IO.BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void SetIcons(Dictionary<uint, MultiResolutionImage> images)
        {
            // Main icon
            if (MainIconResource.HasValue)
            {
                uint iconId = MainIconResource.Value;

                if (images.ContainsKey(iconId))
                {
                    var icon = images[iconId];
                    ComponentDescription.Metadata.Icon = icon;
                }
            }

            // Configuration icons
            foreach (var configuration in ComponentDescription.Metadata.Configurations)
            {
                if (iconResources.ContainsKey(configuration))
                {
                    uint iconId = iconResources[configuration];

                    if (images.ContainsKey(iconId))
                        configuration.Icon = images[iconId];
                }
            }
        }
    }
}
