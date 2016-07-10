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

using CircuitDiagram.Components.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.Components.Description;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescription
{
    public class ConnectionGroup : Conditional<ConnectionDescription[]>
    {
        public ConnectionGroup(IConditionTreeItem conditions, ConnectionDescription[] connections)
            : base(connections, conditions)
        {
        }
    }
}
