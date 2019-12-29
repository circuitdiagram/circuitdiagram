using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlRenderText : IXmlRenderCommand
    {
        public XmlComponentPoint Location { get; set; }

        public TextAlignment Alignment { get; set; }

        public TextRotation Rotation { get; set; }

        public List<TextRun> TextRuns { get; } = new List<TextRun>();

        public virtual IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            foreach (var location in Location.Flatten(context))
            {
                var command = new RenderText(
                    location.Value,
                    Alignment,
                    TextRuns)
                {
                    Rotation = Rotation,
                };

                yield return new Conditional<IRenderCommand>(command, location.Conditions);
            }
        }
    }
}
