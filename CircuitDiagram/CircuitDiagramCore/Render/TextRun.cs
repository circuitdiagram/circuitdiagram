// TextRun.cs
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

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Represents a block of text with a single style of formatting.
    /// </summary>
    public class TextRun
    {
        /// <summary>
        /// How the text should be formatted.
        /// </summary>
        public TextRunFormatting Formatting { get; set; }

        /// <summary>
        /// The text to render.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Creates a new text run with the specified parameters.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="formatting">How the text should be formatted.</param>
        public TextRun(string text, TextRunFormatting formatting)
        {
            Text = text;
            Formatting = formatting;
        }
    }
}
