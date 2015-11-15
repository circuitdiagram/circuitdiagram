using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComponentCompiler
{
    class CliCompileOptions
    {
        public string Output { get; set; }
        public string Input { get; set; }
        public List<string> IconPaths { get; private set; }
        public string DefaultIconPath { get; set; }
        public bool Sign { get; set; }
        public string CertificateThumbprint { get; set; }
        public bool Recursive { get; set; }
        public bool Strict { get; set; }
        public bool Verbose { get; set; }
        public bool Colour { get; set; }
        public string WriteManifest { get; set; }
        public string Preview { get; set; }

        public CliCompileOptions()
        {
            IconPaths = new List<string>();
        }
    }
}
