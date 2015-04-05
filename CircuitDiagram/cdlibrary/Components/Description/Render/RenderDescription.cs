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
using System.Windows.Media;
using CircuitDiagram.Render;
using CircuitDiagram.Components.Conditions;

namespace CircuitDiagram.Components.Description.Render
{
    public class RenderDescription : Conditional<IRenderCommand[]>
    {
        public RenderDescription(IConditionTreeItem conditions, IRenderCommand[] commands)
            :base(commands, conditions)
        {
        }

        public void Render(Component component, IRenderContext dc, bool absolute)
        {
            foreach (IRenderCommand command in Value)
                command.Render(component, dc, absolute);
        }
    }
}
