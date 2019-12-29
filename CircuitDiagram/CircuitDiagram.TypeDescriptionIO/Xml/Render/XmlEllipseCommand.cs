using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlEllipseCommand : IXmlRenderCommand
    {
        public XmlComponentPoint Centre { get; set; }

        public double RadiusX { get; set; }

        public double RadiusY { get; set; }

        public double StrokeThickness { get; set; } = 2.0;

        public bool Fill { get; set; }

        public IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            foreach(var centre in Centre.Flatten(context))
            {
                var command = new Ellipse(
                    centre.Value,
                    RadiusX,
                    RadiusY,
                    StrokeThickness,
                    Fill);

                yield return new Conditional<IRenderCommand>(command, centre.Conditions);
            }
        }
    }
}
