using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    public class LoadError
    {
        public string File { get; set; }
        public int StartLine { get; set; }
        public int StartCol { get; set; }
        public int? EndLine { get; set; }
        public int? EndCol { get; set; }
        public LoadErrorCategory Category { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }

        public LoadError(string file, int line, int col, LoadErrorCategory cat, string message)
        {
            File = file;
            StartLine = line;
            StartCol = col;
            Category = cat;
            Code = "0";
            Message = message;
        }

        public override string ToString()
        {
            string origin;
            if (!EndLine.HasValue)
                origin = String.Format("{0}({1},{2})", File, StartLine, StartCol);
            else
                origin = String.Format("{0}({1},{2},{3},{4})", File, StartLine, StartCol, EndLine, EndCol);

            return String.Format("{0}: {1} {2}: {3}", origin, Category.ToString().ToLowerInvariant(), Code, Message);
        }
    }

    public enum LoadErrorCategory
    {
        Warning,
        Error
    }
}
