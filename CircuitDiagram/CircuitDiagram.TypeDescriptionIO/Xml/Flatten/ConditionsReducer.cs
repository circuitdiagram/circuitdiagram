using CircuitDiagram.TypeDescription.Conditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    static class ConditionsReducer
    {
        public static IConditionTreeItem SimplifyConditions(IConditionTreeItem item)
        {
            switch (item)
            {
                case ConditionTreeLeaf leaf:
                    return leaf;
                case ConditionTree tree:
                    {
                        var left = SimplifyConditions(tree.Left);
                        var right = SimplifyConditions(tree.Right);

                        if (left == ConditionTree.Empty && right == ConditionTree.Empty)
                        {
                            return ConditionTree.Empty;
                        }

                        if (left == ConditionTree.Empty)
                        {
                            return right;
                        }

                        if (right == ConditionTree.Empty)
                        {
                            return left;
                        }

                        return tree;
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
