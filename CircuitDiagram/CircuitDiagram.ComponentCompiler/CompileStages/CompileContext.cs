using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CircuitDiagram.Components.Description;
using CircuitDiagram.IO;
using log4net;

namespace CircuitDiagram.Compiler.CompileStages
{
    class CompileContext
    {
        public Stream Input { get; set; }
        public Stream Output { get; set; }
        public CompileOptions Options { get; set; }
        public ComponentDescription Description { get; set; }
        public IResourceProvider Resources { get; set; }
        public List<CompileError> Errors { get; } = new List<CompileError>();
    }
}
