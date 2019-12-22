using CircuitDiagram.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.CLI.ComponentPreview
{
    public sealed class PreviewGenerationOptions
    {
        public bool DebugLayout { get; set; }
        public bool Grid { get; set; }
        public bool Crop { get; set; }
        public bool Center { get; set; }
        public double Width { get; set; } = 640.0;
        public double Height { get; set; } = 480.0;
        public string Configuration { get; set; }
        public bool Horizontal { get; set; } = true;
        public double Size { get; set; } = 60.0;
        public double Scale { get; set; } = 2.0;
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public Dictionary<PropertyName, PropertyValue> RawProperties { get; set; } = new Dictionary<PropertyName, PropertyValue>();
        public PngRenderer Renderer { get; set; } = PngRenderer.Skia;
    }
}
