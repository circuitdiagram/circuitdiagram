using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlRenderGroup : Conditional<List<IXmlRenderCommand>>, IRootFlattenable<RenderDescription>
    {
        public XmlRenderGroup(IConditionTreeItem conditions)
            :base(new List<IXmlRenderCommand>(), conditions)
        {
        }

        public AutoRotateType AutoRotate { get; set; }

        public FlipState AutoRotateFlip { get; set; }

        public IEnumerable<RenderDescription> Flatten(FlattenContext context)
        {
            // TODO: Group by/simplify conditions

            var flatConditions = new ConditionTree(ConditionTree.ConditionOperator.AND, context.AncestorConditions, Conditions);

            foreach (var command in Value.SelectMany(x => x.Flatten(context)))
            {
                var conditions = new ConditionTree(ConditionTree.ConditionOperator.AND, flatConditions, command.Conditions);
                yield return new RenderDescription(conditions, new[] { command.Value });
            }
        }
    }
}
