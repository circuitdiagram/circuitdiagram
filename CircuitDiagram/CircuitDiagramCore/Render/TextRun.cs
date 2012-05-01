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
