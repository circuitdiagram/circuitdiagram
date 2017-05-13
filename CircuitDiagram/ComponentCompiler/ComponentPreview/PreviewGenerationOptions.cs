using System;
using System.Collections.Generic;
using System.Text;
using CircuitDiagram.TypeDescription;

namespace ComponentCompiler.ComponentPreview
{
    sealed class PreviewGenerationOptions
    {
        public bool Crop { get; set; }
        public bool Center { get; set; }
        public double Width { get; set; } = 640.0;
        public double Height { get; set; } = 480.0;
        public ComponentConfiguration Configuration { get; set; }
        public bool Horizontal { get; set; } = true;
        public double Size { get; set; } = 60.0;
    }
}
