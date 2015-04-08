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
using CircuitDiagram.Components.Description.Render;

namespace CircuitDiagram.Components.Description
{
    public class ComponentDescription
    {
        /// <summary>
        /// A unique ID for this description during runtime.
        /// </summary>
        public int RuntimeID { get; set; }

        public string ID { get; set; }
        public string ComponentName { get; set; }
        public bool CanResize { get; set; }
        public bool CanFlip { get; set; }
        public double MinSize { get; set; }
        public ComponentProperty[] Properties { get; set; }
        public ConnectionGroup[] Connections { get; set; }
        public RenderDescription[] RenderDescriptions { get; set; }
        public Conditional<FlagOptions>[] Flags { get; set; }
        public ComponentDescriptionMetadata Metadata { get; set; }
        public ComponentDescriptionSource Source { get; set; }

        internal ComponentDescription()
        {
            // Set defaults
            CanResize = true;
            CanFlip = true;
            Metadata = new ComponentDescriptionMetadata();
        }

        public ComponentDescription(string id, string componentName, bool canResize, bool canFlip, double minSize, ComponentProperty[] properties, ConnectionGroup[] connections, RenderDescription[] renderDescriptions, Conditional<FlagOptions>[] flags, ComponentDescriptionMetadata metadata)
        {
            ID = id;
            ComponentName = componentName;
            CanResize = canResize;
            CanFlip = canFlip;
            MinSize = minSize;
            Properties = properties;
            Connections = connections;
            RenderDescriptions = renderDescriptions;
            Flags = flags;
            Metadata = metadata;
        }
    }
}