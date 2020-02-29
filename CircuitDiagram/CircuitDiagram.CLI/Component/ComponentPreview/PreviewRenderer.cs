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
using CircuitDiagram.Render.Connections;
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
            }
            else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Layout.Orientation == Orientation.Horizontal)
            {
                component.Layout.Orientation = Orientation.Vertical;
            }

            // Flip
            if ((flagOptions & FlagOptions.FlipPrimary) == FlagOptions.FlipPrimary && (options.Flip & FlipState.Primary) == FlipState.Primary)
            {
                component.Layout.Flip |= FlipState.Primary;
            }
            if ((flagOptions & FlagOptions.FlipSecondary) == FlagOptions.FlipSecondary && (options.Flip & FlipState.Secondary) == FlipState.Secondary)
            {
                component.Layout.Flip |= FlipState.Secondary;
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

            Vector translationOffset = new Vector(0, 0);
            if (options.Crop)
            {
                resultContext = drawingContext(options.Crop ? bb.Size : new Size(options.Width * options.Scale, options.Height * options.Scale));
                dc = new TranslationDrawingContext(new Vector(Math.Round(-bb.X), Math.Round(-bb.Y)), resultContext);
            }
            else if (options.Center)
            {
                resultContext = drawingContext(new Size(options.Width, options.Height));

                var x = bb.X - options.Width / 2 + bb.Width / 2;
                var y = bb.Y - options.Height / 2 + bb.Height / 2;
                translationOffset = new Vector(Math.Round(-x), Math.Round(-y));
                dc = new TranslationDrawingContext(translationOffset, resultContext);
            }
            else
            {
                resultContext = drawingContext(new Size(options.Width, options.Height));
                dc = resultContext;
            }

            if (options.Grid && resultContext is SkiaDrawingContext gridSkiaContext)
            {
                RenderGrid(gridSkiaContext, options.Width, options.Height, translationOffset);
            }

            if (options.DebugLayout && resultContext is SkiaDrawingContext debugSkiaContext)
            {
                RenderDebugLayout(debugSkiaContext, component, desc, translationOffset);
            }

            docRenderer.RenderCircuit(document, dc);

            return resultContext;
        }

        private static void RenderGrid(SkiaDrawingContext skiaContext, double width, double height, Vector translationOffset)
        {
            skiaContext.Mutate(canvas =>
            {
                int offsetX = (int)translationOffset.X % 10;
                int offsetY = (int)translationOffset.Y % 10;

                canvas.Save();
                canvas.Translate(offsetX, offsetY);

                var gridPaint = new SKPaint
                {
                    Color = SKColors.LightGray.WithAlpha(75),
                    StrokeWidth = 1.0f,
                };

                for (int x = -10; x < width + 10; x += 10)
                {
                    canvas.DrawLine(
                        x,
                        -10,
                        x,
                        (int)height + 10,
                        gridPaint);
                }

                for (int y = -10; y < height + 10; y += 10)
                {
                    canvas.DrawLine(
                        -10,
                        y,
                        (int)width + 10,
                        y,
                        gridPaint);
                }

                canvas.Restore();
            });
        }

        private static void RenderDebugLayout(
            SkiaDrawingContext skiaContext,
            PositionalComponent component,
            ComponentDescription desc,
            Vector translationOffset)
        {
            var layoutOptions = new LayoutOptions
            {
                AlignMiddle = (desc.DetermineFlags(component) & FlagOptions.MiddleMustAlign) == FlagOptions.MiddleMustAlign,
                GridSize = 10.0
            };

            skiaContext.Mutate(canvas =>
            {
                canvas.Save();
                canvas.Translate((float)translationOffset.X, (float)translationOffset.Y);

                var connectionDebugPaint = new SKPaint
                {
                    Color = SKColors.CornflowerBlue.WithAlpha(150),
                    StrokeWidth = 1.0f,
                    FilterQuality = SKFilterQuality.High,
                };

                var connectionPositioner = new ConnectionPositioner();
                var connectionPoints = connectionPositioner.PositionConnections(component, desc, layoutOptions);
                foreach (var connection in connectionPoints)
                {
                    canvas.DrawLine(
                        (float)connection.Location.X - 4,
                        (float)connection.Location.Y - 4,
                        (float)connection.Location.X + 4,
                        (float)connection.Location.Y + 4,
                        connectionDebugPaint);
                    canvas.DrawLine(
                        (float)connection.Location.X + 4,
                        (float)connection.Location.Y - 4,
                        (float)connection.Location.X - 4,
                        (float)connection.Location.Y + 4,
                        connectionDebugPaint);

                    if (connection.Orientation == Orientation.Horizontal)
                    {
                        if (connection.IsEdge)
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X,
                                (float)connection.Location.Y - 20,
                                (float)connection.Location.X,
                                (float)connection.Location.Y + 20,
                                connectionDebugPaint);
                        }
                        else if (connection.Location.X % 20.0 == 0.0)
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X,
                                (float)connection.Location.Y - 10,
                                (float)connection.Location.X,
                                (float)connection.Location.Y,
                                connectionDebugPaint);
                        }
                        else
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X,
                                (float)connection.Location.Y,
                                (float)connection.Location.X,
                                (float)connection.Location.Y + 10,
                                connectionDebugPaint);
                        }
                    }
                    else
                    {
                        if (connection.IsEdge)
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X - 20,
                                (float)connection.Location.Y,
                                (float)connection.Location.X + 20,
                                (float)connection.Location.Y,
                                connectionDebugPaint);
                        }
                        else if (connection.Location.Y % 20.0 == 0.0)
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X - 10,
                                (float)connection.Location.Y,
                                (float)connection.Location.X,
                                (float)connection.Location.Y,
                                connectionDebugPaint);
                        }
                        else
                        {
                            canvas.DrawLine(
                                (float)connection.Location.X,
                                (float)connection.Location.Y,
                                (float)connection.Location.X + 10,
                                (float)connection.Location.Y,
                                connectionDebugPaint);
                        }
                    }
                }

                // Start and end points

                var paint = new SKPaint
                {
                    Color = SKColors.CornflowerBlue,
                    IsAntialias = true,
                    Style = SKPaintStyle.StrokeAndFill,
                    StrokeWidth = 2f,
                    StrokeCap = SKStrokeCap.Square,
                };

                canvas.DrawOval(component.Layout.Location.ToSkPoint(), new SKSize(2f, 2f), paint);

                var endOffset = new Vector(
                    component.Layout.Orientation == Orientation.Horizontal ? component.Layout.Size : 0.0,
                    component.Layout.Orientation == Orientation.Vertical ? component.Layout.Size : 0.0);

                canvas.DrawOval(component.Layout.Location.Add(endOffset).ToSkPoint(), new SKSize(2f, 2f), paint);

                canvas.Restore();
            });
        }
    }
}
