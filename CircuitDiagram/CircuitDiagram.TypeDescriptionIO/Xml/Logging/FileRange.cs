using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    public struct FileRange
    {
        public FileRange(int startLine, int startCol, int endLine, int endCol)
        {
            StartLine = startLine;
            StartCol = startCol;
            EndLine = endLine;
            EndCol = endCol;
        }

        public int StartLine { get; }
        public int StartCol { get; }
        public int EndLine { get; }
        public int EndCol { get; }
    }
}
