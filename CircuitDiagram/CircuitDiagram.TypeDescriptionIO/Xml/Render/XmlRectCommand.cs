using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlRectCommand : IXmlRenderCommand
    {
        public XmlComponentPoint Location { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double StrokeThickness { get; set; } = 2.0;

        public bool Fill { get; set; }

        public IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            double width = context.AutoRotate.Mirror ? Height : Width;
            double height = context.AutoRotate.Mirror ? Width : Height;

            foreach (var location in Location.Flatten(context))
            {
                var command = new Rectangle(
                    location.Value,
                    width,
                    height,
                    StrokeThickness,
                    Fill);

                yield return new Conditional<IRenderCommand>(command, location.Conditions);
            }
        }
    }
}
