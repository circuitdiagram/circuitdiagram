// TextRunFormatting.cs
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

namespace CircuitDiagram.Drawing.Text
{
    /// <summary>
    /// Defines the style of a block of text.
    /// </summary>
    public class TextRunFormatting
    {
        /// <summary>
        /// The size of the text, in points.
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The formatting type.
        /// </summary>
        public TextRunFormattingType FormattingType { get; set; }

        /// <summary>
        /// Creates a new TextRunFormatting with the specified parameters.
        /// </summary>
        /// <param name="formattingType">Controls</param>
        /// <param name="size"></param>
        public TextRunFormatting(TextRunFormattingType formattingType, double size = 12d)
        {
            FormattingType = formattingType;
            Size = size;
        }

        /// <summary>
        /// Formatting for normal text.
        /// </summary>
        public static TextRunFormatting Normal
        {
            get { return new TextRunFormatting(TextRunFormattingType.Normal); }
        }

        /// <summary>
        /// Formatting for subscript text.
        /// </summary>
        public static TextRunFormatting Subscript
        {
            get { return new TextRunFormatting(TextRunFormattingType.Subscript); }
        }

        /// <summary>
        /// Formatting for superscript text.
        /// </summary>
        public static TextRunFormatting Superscript
        {
            get { return new TextRunFormatting(TextRunFormattingType.Superscript); }
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to TextRunFormatting return false.
            TextRunFormatting o = obj as TextRunFormatting;
            if ((System.Object)o == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Size.Equals(o.Size)
                && FormattingType.Equals(o.FormattingType));
        }

        public override int GetHashCode()
        {
            return Size.GetHashCode()
                ^ FormattingType.GetHashCode();
        }
    }
}
