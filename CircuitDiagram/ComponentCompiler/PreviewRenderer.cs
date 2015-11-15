using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram;
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Render;

namespace ComponentCompiler
{
    static class PreviewRenderer
    {
        public static byte[] GetSvgPreview(ComponentDescription desc, ComponentConfiguration configuration, bool horizontal)
        {
            var creationDataDictionary = new Dictionary<string, object>
            {
                {"@x", 320 - (horizontal ? 30 : 0)},
                {"@y", 240 - (!horizontal ? 30 : 0)},
                {"@size", 60},
                {"@orientation", horizontal ? "horizontal" : "vertical"}
            };

            if (configuration != null)
            {
                foreach (var setter in configuration.Setters)
                {
                    creationDataDictionary.Add(setter.Key, setter.Value.Value);
                }
            }

            var component = Component.Create(desc, creationDataDictionary);

            // Minimum size
            component.ImplementMinimumSize(component.Description.MinSize);

            // Orientation
            FlagOptions flagOptions = ComponentHelper.ApplyFlags(component);
            if ((flagOptions & FlagOptions.HorizontalOnly) == FlagOptions.HorizontalOnly && component.Orientation == Orientation.Vertical)
            {
                component.Orientation = Orientation.Horizontal;
                component.Size = component.Description.MinSize;
            }
            else if ((flagOptions & FlagOptions.VerticalOnly) == FlagOptions.VerticalOnly && component.Orientation == Orientation.Horizontal)
            {
                component.Orientation = Orientation.Vertical;
                component.Size = component.Description.MinSize;
            }

            var document = new CircuitDocument();
            document.Elements.Add(component);

            var renderer = new SVGRenderer(640, 480);
            renderer.Begin();
            document.Render(renderer);
            renderer.End();

            return renderer.SVGDocument.ToArray();
        }
    }
}
