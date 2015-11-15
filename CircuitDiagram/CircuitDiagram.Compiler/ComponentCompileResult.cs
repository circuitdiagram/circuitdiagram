using System;

namespace CircuitDiagram.Compiler
{
    public class ComponentCompileResult
    {
        public string ComponentName { get; set; }
        public Guid Guid { get; set; }
        public string Author { get; set; }
        public bool Success { get; set; }
    }
}
