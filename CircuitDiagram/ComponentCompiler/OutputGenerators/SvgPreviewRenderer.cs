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
using System.IO;
using System.Text;
using CircuitDiagram.Circuit;
using CircuitDiagram.Compiler;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using ComponentConfiguration = CircuitDiagram.TypeDescription.ComponentConfiguration;

namespace ComponentCompiler.OutputGenerators
{
    class SvgPreviewRenderer : IOutputGenerator
    {
        public string FileExtension => ".svg";

        public void Generate(ComponentDescription description, ComponentConfiguration configuration, IResourceProvider resourceProvider, bool horizontal, Stream input, Stream output)
        {
            var drawingContext = new SvgDrawingContext(640, 480);
            PreviewRenderer.RenderPreview(drawingContext, description, new ComponentConfiguration("", "", new Dictionary<PropertyName, PropertyValue>()), true);
            drawingContext.SvgDocument.WriteTo(output);
        }
    }
}
