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
using CircuitDiagram.Render.Drawing;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescription;
using SkiaSharp;

namespace CircuitDiagram.CLI.ComponentPreview
{
    static class PreviewRenderer
    {
        public static T RenderPreview<T>(Func<Size, T> drawingContext,
            ComponentDescription desc,
            PreviewGenerationOptions options)
            where T: IDrawingContext
        {
            var componentType = new TypeDescriptionComponentType(desc.Metadata.GUID, ComponentType.UnknownCollection, desc.ComponentName);

            var component = new PositionalComponent(componentType);
            component.Layout.Location = new Point(options.Width / 2 - (options.Horizontal ? options.Size : 0), options.Height / 2 - (!options.Horizontal ? options.Size : 0));
            component.Layout.Orientation = options.Horizontal ? Orientation.Horizontal : Orientation.Vertical;

            // Minimum size
            component.Layout.Size = Math.Max(desc.MinSize, options.Size);

            // Configuration
            var configurationDesc = desc.Metadata.Configurations.FirstOrDefault(x => x.Name == options.Configuration);
            if (configurationDesc != null)
            {
                foreach (var setter in configurationDesc.Setters)
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
            
            // Properties
            foreach (var property in options.Properties)
            {
                // Look up serialized name
                var propertyInfo = desc.Properties.FirstOrDefault(x => x.Name == property.Key);

                if (propertyInfo != null)
                    component.Properties[propertyInfo.SerializedName] = PropertyValue.Dynamic(property.Value);
            }

            foreach (var property in options.RawProperties)
            {
                component.Properties[property.Key] = property.Value;
            }

            CircuitDocument document = new CircuitDocument();
            document.Elements.Add(component);
            
            var lookup = new DictionaryComponentDescriptionLookup();
            lookup.AddDescription(componentType, desc);
            var docRenderer = new CircuitRenderer(lookup);

            var buffer = new SkiaBufferedDrawingContext();
            docRenderer.RenderCircuit(document, buffer);
            var bb = buffer.BoundingBox ?? new Rect();

            T resultContext;
            IDrawingContext dc;

            if (options.Crop)
            {
                resultContext = drawingContext(options.Crop ? bb.Size : new Size(options.Width, options.Height));
                dc = new TranslationDrawingContext(new Vector(Math.Round(-bb.X), Math.Round(-bb.Y)), resultContext);
            }
            else if (options.Center)
            {
                resultContext = drawingContext(new Size(options.Width, options.Height));

                var x = bb.X - options.Width / 2 + bb.Width / 2;
                var y = bb.Y - options.Height / 2 + bb.Height / 2;
                dc = new TranslationDrawingContext(new Vector(Math.Round(-x), Math.Round(-y)), resultContext);
            }
            else
            {
                resultContext = drawingContext(new Size(options.Width, options.Height));
                dc = resultContext;
            }

            if (options.DebugLayout)
            {
                var endOffset = new Vector(component.Layout.Orientation == Orientation.Horizontal ? component.Layout.Size : 0.0,
                                          component.Layout.Orientation == Orientation.Vertical ? component.Layout.Size : 0.0);

                var skiaContext = resultContext as SkiaDrawingContext;
                var originalColor = SKColors.Black;
                if (skiaContext != null)
                {
                    originalColor = skiaContext.Color;
                    skiaContext.Color = SKColors.CornflowerBlue;
                }

                dc.DrawEllipse(component.Layout.Location, 2d, 2d, 2d, true);
                dc.DrawEllipse(component.Layout.Location.Add(endOffset), 2d, 2d, 2d, true);

                if (skiaContext != null)
                {
                    skiaContext.Color = originalColor;
                }
            }

            docRenderer.RenderCircuit(document, dc);

            return resultContext;
        }
    }
}
