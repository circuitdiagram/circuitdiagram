// CircuitDocumentIO.cs
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
using CircuitDiagram.Components;
using CircuitDiagram.Elements;
using CircuitDiagram.IO;
using System.Windows;

namespace CircuitDiagram
{
    /// <summary>
    /// Provides functions for converting a CircuitDocument to and from an IODocument
    /// </summary>
    public static class CircuitDocumentIO
    {
        /// <summary>
        /// Converts a <see cref="CircuitDocument"/> to an <see cref="IODocument"/>.
        /// </summary>
        /// <param name="document">The CircuitDocument to convert.</param>
        /// <returns>An IODocument constructed from the CircuitDocument.</returns>
        public static IODocument ToIODocument(this CircuitDocument document, out IDictionary<IOComponentType, EmbedComponentData> embedComponents)
        {
            IODocument ioDocument = new IODocument();
            ioDocument.Size = document.Size;

            // Set metadata
            ioDocument.Metadata = document.Metadata;

            // Get connections
            Dictionary<Component, Dictionary<string, string>> connections = ConnectionHelper.RemoveWires(document);

            // Generate types
            Dictionary<ComponentIdentifier, IOComponentType> componentTypes = new Dictionary<ComponentIdentifier,IOComponentType>();
            foreach(Component component in document.Components)
            {
                if (ComponentHelper.IsWire(component))
                    continue; // Skip wires

                ComponentIdentifier identifier = new ComponentIdentifier(component.Description, component.Configuration());
                if (!componentTypes.ContainsKey(identifier))
                {
                    IOComponentType ioType = new IOComponentType(component.Description.Metadata.ImplementSet,
                        (identifier.Configuration != null && !String.IsNullOrEmpty(identifier.Configuration.ImplementationName) ? identifier.Configuration.ImplementationName : component.Description.Metadata.ImplementItem));
                    ioType.Name = component.Description.ComponentName;
                    ioType.GUID = component.Description.Metadata.GUID;
                    componentTypes.Add(identifier, ioType);
                }
            }

            // Add visible components
            int idCounter = 0; // generate component IDs
            foreach (IComponentElement component in document.Elements.Where(component => component is IComponentElement && !ComponentHelper.IsWire(component)))
            {
                IOComponent ioComponent = new IOComponent();
                ioComponent.ID = idCounter.ToString();
                ioComponent.Size = component.Size;
                ioComponent.Location = new Point(component.Location.X, component.Location.Y);
                ioComponent.IsFlipped = component.IsFlipped;
                ioComponent.Orientation = component.Orientation;
                IOComponentType ioType = new IOComponentType(component.ImplementationCollection, component.ImplementationItem);

                if (component is Component)
                {
                    Component cComponent = component as Component;
                    ioType.Name = cComponent.Description.ComponentName;
                    ioType.GUID = cComponent.Description.Metadata.GUID;
                    ioComponent.Type = ioType;

                    // Set connections
                    if (connections.ContainsKey(cComponent))
                        foreach (var connection in connections[cComponent])
                            ioComponent.Connections.Add(connection.Key, connection.Value);
                }

                // Set properties
                foreach (var property in component.Properties)
                    ioComponent.Properties.Add(new IOComponentProperty(property.Key, property.Value, true)); // TODO: implement IsStandard

                ioDocument.Components.Add(ioComponent);

                idCounter++;
            }

            // Add unavailable components
            foreach (DisabledComponent component in document.DisabledComponents)
            {
                Point? location = null;
                if (component.Location.HasValue)
                    location = new Point(component.Location.Value.X, component.Location.Value.Y);
                IOComponent ioComponent = new IOComponent(idCounter.ToString(), location, component.Size, component.IsFlipped, component.Orientation, new IOComponentType(component.ImplementationCollection, component.ImplementationItem));

                // Set properties
                foreach (var property in component.Properties)
                    ioComponent.Properties.Add(new IOComponentProperty(property.Key, property.Value, true));

                ioDocument.Components.Add(ioComponent);

                idCounter++;
            }

            // Add wires
            foreach (IComponentElement wire in document.Components.Where(component => ComponentHelper.IsWire(component)))
            {
                IOWire ioWire = new IOWire(new Point(wire.Location.X, wire.Location.Y), wire.Size, wire.Orientation);
                ioDocument.Wires.Add(ioWire);
            }

            // Embed components
            embedComponents = new Dictionary<IOComponentType, EmbedComponentData>();
            foreach (EmbedDescription embedItem in document.Metadata.EmbedComponents.Where(item => item.IsEmbedded == true))
            {
                if (!String.IsNullOrEmpty(embedItem.Description.Source.Path) && System.IO.File.Exists(embedItem.Description.Source.Path))
                {
                    EmbedComponentData embedData = new EmbedComponentData();
                    embedData.Stream = System.IO.File.OpenRead(embedItem.Description.Source.Path);
                    embedData.FileExtension = System.IO.Path.GetExtension(embedItem.Description.Source.Path);

                    switch (embedData.FileExtension)
                    {
                        case ".xml":
                            embedData.ContentType = "application/xml";
                            break;
                        case ".cdcom":
                            embedData.ContentType = IO.CDDX.ContentTypeNames.BinaryComponent;
                            break;
                    }

                    List<IOComponentType> associatedTypes = new List<IOComponentType>();
                    foreach (var item in componentTypes)
                    {
                        if (item.Key.Description == embedItem.Description && !embedComponents.ContainsKey(item.Value))
                            embedComponents.Add(item.Value, embedData);
                    }
                }
            }

            return ioDocument;
        }

        /// <summary>
        /// Converts a <see cref="IODocument"/> to a <see cref="CircuitDocument"/>.
        /// </summary>
        /// <param name="document">The IODocument to convert.</param>
        /// <returns>A CircuitDocument constructed from the IODocument.</returns>
        public static CircuitDocument ToCircuitDocument(this IODocument document, IDocumentReader reader, out List<IOComponentType> unavailableComponents)
        {
            CircuitDocument circuitDocument = new CircuitDocument();
            circuitDocument.Size = document.Size;

            // Set metadata
            circuitDocument.Metadata = new CircuitDocumentMetadata(null, null, document.Metadata);

            // Add components
            unavailableComponents = new List<IOComponentType>();
            foreach (IOComponent component in document.Components)
            {
                ComponentIdentifier identifier = null;
                
                // Find description
                if (component.Type.GUID != Guid.Empty && ComponentHelper.IsDescriptionAvailable(component.Type.GUID))
                    identifier = new ComponentIdentifier(ComponentHelper.FindDescription(component.Type.GUID));
                if (identifier == null && reader.IsDescriptionEmbedded(component.Type))
                    identifier = LoadDescription(reader.GetEmbeddedDescription(component.Type), component.Type);
                if (identifier == null && component.Type.IsStandard)
                    identifier = ComponentHelper.GetStandardComponent(component.Type.Collection, component.Type.Item);

                if (identifier != null)
                {
                    // Add full component

                    Dictionary<string, object> properties = new Dictionary<string,object>();
                    foreach(var property in component.Properties)
                        properties.Add(property.Key, property.Value);

                    Component addComponent = Component.Create(identifier, properties);
                    addComponent.Layout(component.Location.Value.X, component.Location.Value.Y, (component.Size.HasValue ? component.Size.Value : identifier.Description.MinSize), component.Orientation.Value, component.IsFlipped == true);
                    addComponent.ImplementMinimumSize(addComponent.Description.MinSize);
                    FlagOptions flagOptions = ComponentHelper.ApplyFlags(addComponent);
                    if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && component.Orientation == Orientation.Vertical)
                        addComponent.Orientation = Orientation.Horizontal;
                    else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Orientation == Orientation.Horizontal)
                        addComponent.Orientation = Orientation.Vertical;
                    circuitDocument.Elements.Add(addComponent);
                }
                else
                {
                    // Add disabled component
                    // TODO
                    if (!unavailableComponents.Contains(component.Type))
                        unavailableComponents.Add(component.Type);
                }
            }

            // Add wires
            foreach (IOWire wire in document.Wires)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>(4);
                properties.Add("@x", wire.Location.X);
                properties.Add("@y", wire.Location.Y);
                properties.Add("@orientation", wire.Orientation == Orientation.Horizontal);
                properties.Add("@size", wire.Size);

                Component wireComponent = Component.Create(ComponentHelper.WireDescription, properties);
                wireComponent.Layout(wire.Location.X, wire.Location.Y, wire.Size, wire.Orientation, false);
                wireComponent.ApplyConnections(circuitDocument);
                circuitDocument.Elements.Add(wireComponent);
            }

            // Connections
            foreach (Component component in circuitDocument.Components)
                component.ApplyConnections(circuitDocument);

            return circuitDocument;
        }

        /// <summary>
        /// Loads a component description from the stream and adds it to the component descriptions store.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <param name="type">The type to find within the description.</param>
        /// <returns>A configuration with the loaded component description if it was available, null if it could not be loaded.</returns>
        private static ComponentIdentifier LoadDescription(EmbedComponentData data, IOComponentType type)
        {
            if (data.ContentType == IO.CDDX.ContentTypeNames.BinaryComponent)
            {
                // Binary component
                BinaryLoader loader = new BinaryLoader();
                if (loader.Load(data.Stream))
                {
                    var descriptions = loader.GetDescriptions();
                    if (descriptions.Length > 0)
                        return FindIdentifier(type, descriptions[0]);
                    else
                        return null;
                }
                else
                {
                    // Load failed
                    return null;
                }
            }
            else if (data.ContentType == "application/xml")
            {
                // XML component
                XmlLoader loader = new XmlLoader();
                if (loader.Load(data.Stream))
                {
                    var descriptions = loader.GetDescriptions();
                    if (descriptions.Length > 0)
                        return FindIdentifier(type, descriptions[0]);
                    else
                        return null;
                }
                else
                {
                    // Load failed
                    return null;
                }
            }
            else
            {
                // Unknown type
                return null;
            }
        }

        /// <summary>
        /// Finds the component implementation which represents the specified type.
        /// </summary>
        /// <param name="type">The type to find.</param>
        /// <param name="description">The description to search.</param>
        /// <returns>The ComponentConfiguration if one could be found, null otherwise.</returns>
        private static ComponentIdentifier FindIdentifier(IOComponentType type, ComponentDescription description)
        {
            // Check implementation
            if (description.Metadata.ImplementSet == type.Collection && description.Metadata.ImplementItem == type.Item)
                return new ComponentIdentifier(description);
            // Check configurations
            else if (description.Metadata.ImplementItem == type.Collection)
            {
                foreach (ComponentConfiguration configuration in description.Metadata.Configurations)
                {
                    if (configuration.ImplementationName == type.Item)
                        return new ComponentIdentifier(description, configuration);
                }
            }
            // Check GUID
            if (description.Metadata.GUID != Guid.Empty && description.Metadata.GUID == type.GUID)
                return new ComponentIdentifier(description);
            // Check name
            else if (description.ComponentName == type.Name)
                return new ComponentIdentifier(description);
            else
                return null; // Incorrect
        }
    }
}
