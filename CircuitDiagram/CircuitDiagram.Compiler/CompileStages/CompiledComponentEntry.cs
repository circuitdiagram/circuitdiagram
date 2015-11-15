using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cdcompile
{
    class CompiledComponentEntry
    {
        public string ComponentName { get; set; }
        public Guid Guid { get; set; }
        public string Author { get; set; }
        public string OutputPath { get; set; }
        public bool Success { get; set; }
    }
}
