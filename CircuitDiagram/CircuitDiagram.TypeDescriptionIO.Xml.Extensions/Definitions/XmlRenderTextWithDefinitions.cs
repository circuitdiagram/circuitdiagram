using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render.TypeDescription.Conditions;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    class XmlRenderTextWithDefinitions : XmlRenderText
    {
        public new ConditionalCollection<TextAlignment> Alignment { get; set; }

        public new ConditionalCollection<TextRotation> Rotation { get; set; }

        public override IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            foreach (var location in Location.Flatten(context))
            {
                foreach(var alignment in Alignment)
                {
                    foreach(var rotation in Rotation)
                    {
                        var command = new RenderText(
                            location.Value,
                            alignment.Value,
                            TextRuns)
                        {
                            Rotation = rotation.Value,
                        };

                        var conditions = ConditionTreeBuilder.And(new[]
                        {
                            location.Conditions,
                            alignment.Conditions,
                            rotation.Conditions,
                        });

                        yield return new Conditional<IRenderCommand>(command, conditions);
                    }
                }
            }
        }
    }
}
