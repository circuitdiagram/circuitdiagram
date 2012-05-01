using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Render
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
    }
}
