using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Connections
{
    public class XmlConnectionGroup : Conditional<List<XmlConnectionDescription>>, IRootFlattenable<ConnectionGroup>
    {
        public XmlConnectionGroup(IConditionTreeItem conditions)
            : base(new List<XmlConnectionDescription>(), conditions)
        {
        }

        public AutoRotateType AutoRotate { get; set; }

        public FlipState AutoRotateFlip { get; set; }

        public IEnumerable<ConnectionGroup> Flatten(FlattenContext context)
        {
            // TODO: Group by/simplify conditions

            var flatConditions = new ConditionTree(ConditionTree.ConditionOperator.AND, context.AncestorConditions, Conditions);

            foreach (var connection in Value.SelectMany(x => x.Flatten(context)))
            {
                var conditions = new ConditionTree(ConditionTree.ConditionOperator.AND, flatConditions, connection.Conditions);
                yield return new ConnectionGroup(conditions, new[] { connection.Value });
            }
        }
    }
}
