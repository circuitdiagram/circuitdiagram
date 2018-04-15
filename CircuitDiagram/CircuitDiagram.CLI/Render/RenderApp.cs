// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
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
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CircuitDiagram.Document;
using CircuitDiagram.Render;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescriptionIO.Util;
using SkiaSharp;

namespace CircuitDiagram.CLI.Render
{
    class RenderApp
    {
        public void Run(string input, string output, string[] componentDirectories)
        {
            var reader = new CircuitDiagramDocumentReader();
            CircuitDiagramDocument circuit;
            using (var fs = File.Open(input, FileMode.Open, FileAccess.Read))
                circuit = reader.ReadCircuit(fs);

            var descriptionLookup = new DirectoryComponentDescriptionLookup(componentDirectories);
            var renderer = new CircuitRenderer(descriptionLookup);
            //var drawingContext = new BitmapDrawingContext((int)circuit.Size.Width, (int)circuit.Size.Height, NamedColors<Argb32>.White);
            var drawingContext = new SkiaDrawingContext((int)circuit.Size.Width, (int)circuit.Size.Height, SKColors.White);
            renderer.RenderCircuit(circuit, drawingContext);

            using (var outputFs = File.OpenWrite(output))
                drawingContext.WriteAsPng(outputFs);
        }
    }
}
