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
using CircuitDiagram.Components;
using CircuitDiagram.Render;
using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescription
{
    public class RenderDescription : Conditional<IRenderCommand[]>
    {
        public RenderDescription(IConditionTreeItem conditions, IRenderCommand[] commands)
            :base(commands, conditions)
        {
        }

        public void Render(PositionalComponent component, LayoutContext layoutContext, IDrawingContext dc)
        {
            foreach (IRenderCommand command in Value)
                command.Render(component.Layout, layoutContext, dc);
        }
    }
}
