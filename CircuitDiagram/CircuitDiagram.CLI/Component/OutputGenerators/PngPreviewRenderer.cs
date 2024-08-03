﻿// This file is part of Circuit Diagram.
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
using System.IO;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.Compiler;
using CircuitDiagram.Render;
using CircuitDiagram.Render.ImageSharp;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescription;
using SkiaSharp;

namespace CircuitDiagram.CLI.Component.OutputGenerators
{
    class PngPreviewRenderer : IOutputGenerator
    {
        public string Format => "png";

        public string FileExtension => ".png";

        public bool AcceptsSourceFileType(SourceFileType sourceType) => true;

        public PngRenderer Renderer { get; set; }

        public void Generate(ComponentDescription description, ComponentConfiguration configuration, IResourceProvider resourceProvider, PreviewGenerationOptions options, Stream input, Stream output, SourceFileType sourceType)
        {
            switch (Renderer)
            {
                case PngRenderer.Skia:
                    {
                        var drawingContext = PreviewRenderer.RenderPreview(size => new SkiaDrawingContext((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), SKColors.White, (float)options.Scale, SKTypeface.FromFamilyName(options.FontFamily), options.FontSizeScale),
                                                                           description,
                                                                           options);
                        drawingContext.WriteAsPng(output);
                        break;
                    }
                case PngRenderer.ImageSharp:
                    {
                        var drawingContext = PreviewRenderer.RenderPreview(size => new ImageSharpDrawingContext((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), SixLabors.ImageSharp.Color.White, options.FontFamily),
                                                                           description,
                                                                           options);
                        drawingContext.WriteAsPng(output);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unknown renderer: {Renderer}");
            }
        }
    }
}
