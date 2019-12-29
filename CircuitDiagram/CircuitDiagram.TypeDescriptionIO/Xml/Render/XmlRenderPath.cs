using CircuitDiagram.Drawing;
using CircuitDiagram.Render.Path;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlRenderPath : IXmlRenderCommand
    {
        public XmlComponentPoint Start { get; set; }

        public double Thickness { get; set; } = 2.0;

        public bool Fill { get; set; }

        public IList<IPathCommand> Commands { get; set; }

        public IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            var commands = Commands.Select(x =>
            {
                if (context.AutoRotate.Mirror)
                    x = x.Reflect();
                if ((context.AutoRotate.FlipType & Circuit.FlipType.Horizontal) == Circuit.FlipType.Horizontal)
                    x = x.Flip(true);
                if ((context.AutoRotate.FlipType & Circuit.FlipType.Vertical) == Circuit.FlipType.Vertical)
                    x = x.Flip(false);
                return x;
            }).ToList();

            foreach (var start in Start.Flatten(context))
            {
                var command = new RenderPath(
                    start.Value,
                    Thickness,
                    Fill,
                    commands);

                yield return new Conditional<IRenderCommand>(command, start.Conditions);
            }
        }
    }
}
