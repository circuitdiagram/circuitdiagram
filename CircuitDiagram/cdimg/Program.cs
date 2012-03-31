// CircuitDocumentWriter.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Linq;
using System.Text;
using CircuitDiagram;
using CircuitDiagram.Components;
using CircuitDiagram.IO;
using CircuitDiagram.Render;
using NDesk.Options;

namespace cdimg
{
    /// <summary>
    /// Generates component previews.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string input = null;
            string output = null;
            int width = 300;
            int height = 150;
            var p = new OptionSet() {
                { "o|output=", v => output = v },
                { "i|input=", v => input = v },
                {"width=", v => width = int.Parse(v) },
                {"height=", v => height = int.Parse(v) }
            };
            List<string> extra = p.Parse(args);

            XmlLoader loader = new XmlLoader();
            loader.Load(System.IO.File.OpenRead(input));
            ComponentDescription[] descriptions = loader.GetDescriptions();

            ComponentHelper.AddDescription(descriptions[0]);
            Component component = Component.Create(String.Format("type:{0},x:0,y:0,size:60,orientation:horizontal", descriptions[0].ComponentName));

            // Implement minimum size
            component.ImplementMinimumSize(component.Description.MinSize);

            CircuitDocument document = new CircuitDocument();
            document.Elements.Add(component);

            WPFRenderer renderer = new WPFRenderer();
            renderer.Begin();
            document.Render(renderer);
            renderer.End();

            System.IO.File.WriteAllBytes(output, renderer.GetPNGImage(width, height, true).ToArray());
        }
    }
}
