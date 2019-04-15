#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.Circuit;

namespace CircuitDiagram.TypeDescription
{
    public class ComponentDescription
    {
        /// <summary>
        /// A unique ID for this description during runtime.
        /// </summary>
        public int RuntimeID { get; set; }

        public string ID { get; set; }
        public string ComponentName { get; set; }
        public double MinSize { get; set; }
        public ComponentProperty[] Properties { get; set; }
        public ConnectionGroup[] Connections { get; set; }
        public RenderDescription[] RenderDescriptions { get; set; }
        public List<Conditional<FlagOptions>> Flags { get; } = new List<Conditional<FlagOptions>>();
        public ComponentDescriptionMetadata Metadata { get; set; }
        public ComponentDescriptionSource Source { get; set; }  

        public ComponentDescription()
        {
            Metadata = new ComponentDescriptionMetadata();
        }

        public ComponentDescription(string id, string componentName, double minSize, ComponentProperty[] properties, ConnectionGroup[] connections, RenderDescription[] renderDescriptions, Conditional<FlagOptions>[] flags, ComponentDescriptionMetadata metadata)
        {
            ID = id;
            ComponentName = componentName;
            MinSize = minSize;
            Properties = properties;
            Connections = connections;
            RenderDescriptions = renderDescriptions;
            Flags = flags.ToList();
            Metadata = metadata;
        }

        public bool GetDefaultFlag(FlagOptions option)
        {
            foreach (var flagSet in Flags)
            {
                if (flagSet.Conditions.Equals(ConditionTree.Empty) && (flagSet.Value & option) == option)
                    return true;
            }

            return false;
        }

        public void SetDefaultFlag(FlagOptions option, bool enabled)
        {
            var defaultFlags = Flags.FirstOrDefault(x => x.Conditions.Equals(ConditionTree.Empty));
            if (defaultFlags == null)
            {
                defaultFlags = new Conditional<FlagOptions>();
                Flags.Insert(0, defaultFlags);
            }

            if (enabled)
                defaultFlags.Value = defaultFlags.Value | option;
            else
                defaultFlags.Value = defaultFlags.Value & ~option;
        }

        public FlagOptions DetermineFlags(PositionalComponent component)
        {
            FlagOptions returnOptions = FlagOptions.None;

            foreach (Conditional<FlagOptions> option in Flags)
            {
                if (option.Conditions.IsMet(component, this))
                    returnOptions |= option.Value;
            }

            return returnOptions;
        }

        public PropertyValue GetProperty(Component instance, string variableName)
        {
            var property = Properties.First(c => c.Name == variableName);

            PropertyValue value;
            if (instance.Properties.TryGetValue(property.SerializedName, out value) && value.PropertyType != PropertyValue.Type.Unset)
                return value;

            return property.Default;
        }
    }
}