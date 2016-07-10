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
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Components.Description
{
    public class ConnectionDescription
    {
        public ConnectionName Name { get; set; }
        public ComponentPoint Start { get; set; }
        public ComponentPoint End { get; set; }
        public ConnectionEdge Edge { get; set; }

        public ConnectionDescription(ComponentPoint start, ComponentPoint end, ConnectionEdge edge, ConnectionName name)
        {
            Start = start;
            End = end;
            Edge = edge;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ConnectionDescription return false.
            ConnectionDescription o = obj as ConnectionDescription;
            if ((System.Object)o == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Name.Equals(o.Name)
                && Start.Equals(o.Start)
                && End.Equals(o.End)
                && Edge.Equals(o.Edge));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                ^ Start.GetHashCode()
                ^ End.GetHashCode()
                ^ Edge.GetHashCode();
        }
    }

    public enum ConnectionEdge
    {
        None = 0,
        Start = 1,
        End = 2,
        Both = 3
    }
}
