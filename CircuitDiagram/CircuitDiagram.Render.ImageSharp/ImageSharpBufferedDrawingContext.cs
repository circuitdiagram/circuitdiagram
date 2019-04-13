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
using System.Text;
using CircuitDiagram.Drawing.Text;
using SixLabors.Fonts;
using Size = CircuitDiagram.Primitives.Size;

namespace CircuitDiagram.Render.ImageSharp
{
    public class ImageSharpBufferedDrawingContext : BufferedDrawingContext
    {
        protected override Size MeasureText(TextRun text)
        {
            if (string.IsNullOrWhiteSpace(text.Text))
                return new Size(0, 0);

            // TODO: Support text.Formatting.FormattingType
            var family = SystemFonts.Find("Arial");
            var font = new Font(family, (float)text.Formatting.Size, FontStyle.Regular);
            var size = TextMeasurer.Measure(text.Text, new RendererOptions(font, 72));
            return new Size(size.Width, size.Height);
        }
    }
}
