using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO;

namespace CircuitDiagram.Compiler
{
    public class CompileError
    {
        private readonly LoadError loadError;
        private string message;
        private LoadErrorCategory? level;

        public CompileError(LoadErrorCategory level, string message)
        {
            this.level = level;
            this.message = message;
        }

        public CompileError(LoadError loadError)
        {
            this.loadError = loadError;
        }

        public string Message
        {
            get { return message ?? loadError?.Message; }
            set { message = value; }
        }

        public LoadErrorCategory? Level
        {
            get { return level ?? loadError?.Category; }
            set { level = value; }
        }

        public override string ToString()
        {
            if (loadError != null)
                return loadError.ToString();

            return $"[{Level}] {Message}";
        }
    }
}
