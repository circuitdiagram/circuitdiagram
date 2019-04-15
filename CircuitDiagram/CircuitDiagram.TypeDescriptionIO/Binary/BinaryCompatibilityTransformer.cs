using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Binary
{
    /// <summary>
    /// Transforms a <see cref="ComponentDescription"/> for maximum compatibility.
    /// </summary>
    public class BinaryCompatibilityTransformer
    {
        private readonly ILogger<BinaryCompatibilityTransformer> _logger;

        public BinaryCompatibilityTransformer(ILogger<BinaryCompatibilityTransformer> logger)
        {
            _logger = logger;
        }

        public ComponentDescription Transform(ComponentDescription description)
        {
            var flags = description.Flags.Select(x =>
            {
                return new Conditional<FlagOptions>(x.Value, TransformConditionTreeItem(description, x.Conditions));
            }).ToArray();

            var connections = description.Connections.Select(x =>
            {
                return new ConnectionGroup(TransformConditionTreeItem(description, x.Conditions), x.Value);
            }).ToArray();

            var renderDescriptions = description.RenderDescriptions.Select(x =>
            {
                return new RenderDescription(TransformConditionTreeItem(description, x.Conditions), x.Value);
            }).ToArray();

            var compatible = new ComponentDescription(description.ID, description.ComponentName, description.MinSize, description.Properties, connections, renderDescriptions, flags, description.Metadata);

            if (flags.Any(x => !x.Conditions.Equals(ConditionTree.Empty) && (x.Value & FlagOptions.FlipPrimary) != 0))
            {
                // Circuit Diagram <=3.1 does not support conditional FlipPrimary.
                // If it is permitted under any conditions, allow under all conditions.
                _logger.LogInformation("Setting default flags with FlipPrimary enabled");
                compatible.SetDefaultFlag(FlagOptions.FlipPrimary, true);
            }

            return compatible;
        }

        private IConditionTreeItem TransformConditionTreeItem(ComponentDescription description, IConditionTreeItem conditions)
        {
            switch (conditions)
            {
                case ConditionTreeLeaf leaf:
                    if (IsBooleanVariable(description, leaf))
                    {
                        switch (leaf.Comparison)
                        {
                            case ConditionComparison.Truthy:
                                _logger.LogInformation("Converting 'truthy' comparison on boolean variable to an 'equals true' comparison");
                                return new ConditionTreeLeaf(
                                    leaf.Type,
                                    leaf.VariableName,
                                    ConditionComparison.Equal,
                                    new Circuit.PropertyValue(true));
                            case ConditionComparison.Falsy:
                                _logger.LogInformation("Converting 'falsy' comparison on boolean variable to an 'equals false' comparison");
                                return new ConditionTreeLeaf(
                                    leaf.Type,
                                    leaf.VariableName,
                                    ConditionComparison.Equal,
                                    new Circuit.PropertyValue(false));
                        }
                    }
                    break;
                case ConditionTree tree:
                    return new ConditionTree(
                        tree.Operator,
                        TransformConditionTreeItem(description, tree.Left),
                        TransformConditionTreeItem(description, tree.Right)
                        );
            }

            // Unchanged
            return conditions;
        }

        private bool IsBooleanVariable(ComponentDescription description, ConditionTreeLeaf leaf)
        {
            if (leaf.Type == ConditionType.State && leaf.VariableName == "horizontal")
                return true;

            return leaf.Type == ConditionType.Property && description.Properties.FirstOrDefault(x => x.Name == leaf.VariableName).Type == PropertyType.Boolean;
        }
    }
}
