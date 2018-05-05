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
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers
{
    class PositioningReader : TextReader
    {
        private StringReader internalReader;
        private int pos = 0;

        /// <summary>
        /// Gets a value indicating the character position of the reader.
        /// </summary>
        public int CharPos
        {
            get { return pos; }
        }

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
