using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Compiler;

namespace ComponentCompiler
{
    class ExtendedCompileResult : ComponentCompileResult
    {
        public ExtendedCompileResult(ComponentCompileResult result)
        {
            Author = result.Author;
            ComponentName = result.ComponentName;
            Guid = result.Guid;
            Success = result.Success;
        }

        public string Input { get; set; }
        public string Output { get; set; }
    }
}
