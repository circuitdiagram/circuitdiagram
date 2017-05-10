// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using ComponentConfiguration = CircuitDiagram.TypeDescription.ComponentConfiguration;

namespace ComponentCompiler
{
    static class PreviewRenderer
    {
        public static void RenderPreview(IDrawingContext drawingContext, ComponentDescription desc, ComponentConfiguration configuration, bool horizontal)
        {
            var componentType = new ComponentType(desc.Metadata.GUID, desc.ComponentName);
            foreach (var property in desc.Properties)
                componentType.PropertyNames.Add(property.SerializedName);

            var component = new PositionalComponent(componentType);
            component.Layout.Location = new Point(320 - (horizontal ? 30 : 0), 240 - (!horizontal ? 30 : 0));
            component.Layout.Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical;

            // Minimum size
            component.Layout.Size = Math.Max(desc.MinSize, 60.0);

            // Configuration
            if (configuration != null)
            {
                foreach (var setter in configuration.Setters)
                    component.Properties[setter.Key] = setter.Value;
            }

            // Orientation
            FlagOptions flagOptions = desc.DetermineFlags(component);
            if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && component.Layout.Orientation == Orientation.Vertical)
            {
                component.Layout.Orientation = Orientation.Horizontal;
                component.Layout.Size = desc.MinSize;
            }
            else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Layout.Orientation == Orientation.Horizontal)
            {
                component.Layout.Orientation = Orientation.Vertical;
                component.Layout.Size = desc.MinSize;
            }

            CircuitDocument document = new CircuitDocument();
            document.Elements.Add(component);

            drawingContext.Begin();

            var lookup = new DictionaryComponentDescriptionLookup();
            lookup.AddDescription(componentType, desc);
            var docRenderer = new CircuitRenderer(lookup);

            docRenderer.RenderCircuit(document, drawingContext);

            drawingContext.End();
        }
    }
}
