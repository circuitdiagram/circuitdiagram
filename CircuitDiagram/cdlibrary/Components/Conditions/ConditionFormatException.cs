using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    public class ConditionFormatException : IOException
    {
        public int PositionStart { get; set; }

        public int PositionEnd { get; set; }

        public ConditionFormatException(string message, int positionStart, int positionEnd)
            : base(message)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }
    }
}
