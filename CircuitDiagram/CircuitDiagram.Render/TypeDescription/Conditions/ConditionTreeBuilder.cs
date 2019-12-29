using CircuitDiagram.TypeDescription.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Render.TypeDescription.Conditions
{
    public static class ConditionTreeBuilder
    {
        public static IConditionTreeItem And(IEnumerable<IConditionTreeItem> input)
        {
            var result = ConditionTree.Empty;
            foreach(var item in input)
            {
                result = new ConditionTree(ConditionTree.ConditionOperator.AND, result, item);
            }
            return result;
        }

        public static IConditionTreeItem And(params IConditionTreeItem[] input)
        {
            return And((IEnumerable<IConditionTreeItem>)input);
        }
    }
}
