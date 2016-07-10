using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Conditions
{
    class PositioningReader : TextReader
    {
        private StringReader internalReader;
        private int pos = 0;

        /// <summary>
        /// Gets a value indicating the character position of the reader.
        /// </summary>
        public int CharPos { get { return pos; } }

        public PositioningReader(StringReader inner)
        {
            internalReader = inner;
        }

        public override int Peek()
        {
            return internalReader.Peek();
        }

        public override int Read()
        {
            var c = internalReader.Read();

            if (c >= 0)
                AdvancePosition((Char)c);

            return c;
        }

        private void AdvancePosition(Char c)
        {
            pos++;
        }
    }
}
