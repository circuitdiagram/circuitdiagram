using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Common.Features;
using CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions
{
    public class RenderSectionWithDefinitionsReader : RenderSectionReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IFeatureSwitcher featureSwitcher;
        private readonly IConditionParser conditionParser;
        private readonly IComponentPointParser componentPointParser;
        private readonly IComponentPointTemplateParser componentPointTemplateParser;
        private readonly IXmlSection<DefinitionsSection> definitionsSection;

        private readonly HashSet<string> availableDefinitions;

        public RenderSectionWithDefinitionsReader(IXmlLoadLogger logger,
                                                  IFeatureSwitcher featureSwitcher,
                                                  IConditionParser conditionParser,
                                                  IComponentPointParser componentPointParser,
                                                  IComponentPointTemplateParser componentPointTemplateParser,
                                                  IXmlSection<DefinitionsSection> definitionsSection)
            : base(logger, conditionParser, componentPointParser)
        {
            this.logger = logger;
            this.featureSwitcher = featureSwitcher;
            this.conditionParser = conditionParser;
            this.componentPointParser = componentPointParser;
            this.componentPointTemplateParser = componentPointTemplateParser;
            this.definitionsSection = definitionsSection;
            availableDefinitions = definitionsSection.Value.Definitions.Select(x => x.Key).ToHashSet();
        }

        public override void ReadSection(XElement element, ComponentDescription description)
        {
            var results = new List<RenderDescription>();

            var groupElements = element.Elements(element.GetDefaultNamespace() + "group");
            foreach (var groupElement in groupElements)
            {
                var renderDescriptions = ReadRenderDescriptions(description, groupElement);
                foreach(var renderDescription in renderDescriptions)
                    results.Add(renderDescription);
            }

            description.RenderDescriptions = results.ToArray();
        }

        protected IEnumerable<RenderDescription> ReadRenderDescriptions(ComponentDescription description, XElement renderNode)
        {
            bool definitionsEnabled = featureSwitcher.IsFeatureEnabled(DefinitionsXmlLoaderExtensions.FeatureName);

            IConditionTreeItem conditionCollection = ConditionTree.Empty;
            var conditionsAttribute = renderNode.Attribute("conditions");
            if (conditionsAttribute != null)
            {
                if (!conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection))
                    yield break;
            }

            var declaredDefinitions = new List<string>();
            var whenDefinedAttribute = renderNode.Attribute("whenDefined");
            if (whenDefinedAttribute != null)
            {
                var usedVariables = whenDefinedAttribute.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                declaredDefinitions.AddRange(usedVariables.Select(x => x.Substring(1)));
            }
            var allUndefined = declaredDefinitions.Except(availableDefinitions).ToHashSet();
            if (allUndefined.Any())
            {
                foreach(var undefined in allUndefined)
                    logger.LogError(whenDefinedAttribute, $"Usage of undefined variable: ${undefined}");
                yield break;
            }

            var commands = new List<IRenderCommand>();

            foreach (var renderCommandNode in renderNode.Descendants())
            {
                string commandType = renderCommandNode.Name.LocalName;
                if (commandType == "line")
                {
                    if (ReadLineCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "rect")
                {
                    if (ReadRectCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "ellipse")
                {
                    if (ReadEllipseCommand(description.Metadata.Version, renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "text")
                {
                    if (definitionsEnabled)
                    {
                        foreach (var renderTextDescription in ReadTextCommand(renderCommandNode, description, conditionCollection, declaredDefinitions))
                            yield return renderTextDescription;
                    }
                    else
                    {
                        if (ReadTextCommand(renderCommandNode, description, out var command))
                        {
                            base.ReadTextLocation(renderCommandNode, command);
                            commands.Add(command);
                        }
                    }
                }
                else if (commandType == "path")
                {
                    if (ReadPathCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
            }

            yield return new RenderDescription(conditionCollection, commands.ToArray());
        }

        private IEnumerable<RenderDescription> ReadTextCommand(XElement element, ComponentDescription description, IConditionTreeItem baseConditions, IReadOnlyList<string> usedDefinitions)
        {
            if (!element.GetAttribute("x", logger, out var x) ||
                !element.GetAttribute("y", logger, out var y))
                yield break;

            var locationPoints = EnumerateComponentPoint(x, y, baseConditions, usedDefinitions).ToList();

            foreach (var locationPoint in locationPoints)
            {
                if (!ReadTextCommand(element, description, out var command))
                    yield break;
                
                command.Location = locationPoint.Value;

                foreach(var resolvedCommand in EnumerateRenderText(element, command, locationPoint.Conditions, usedDefinitions))
                    yield return new RenderDescription(resolvedCommand.Conditions, new IRenderCommand[] { resolvedCommand.Value });
            }
        }
        
        protected override bool ValidateText(ComponentDescription description, string text, out string errorMessage)
        {
            if (!featureSwitcher.IsFeatureEnabled(DefinitionsXmlLoaderExtensions.FeatureName))
                return base.ValidateText(description, text, out errorMessage);

            if (!text.StartsWith("$"))
            {
                errorMessage = null;
                return true;
            }

            var name = text.Substring(1);
            var validNames = description.Properties.Select(x => x.Name).Union(definitionsSection.Value.Definitions.Select(x => x.Key)).ToHashSet();
            if (validNames.Contains(name))
            {
                errorMessage = null;
                return true;
            }

            errorMessage = $"Property or definition ${name} used for text value does not exist";
            return false;
        }

        protected override bool ReadTextLocation(XElement element, RenderText command)
        {
            // Do nothing - handled by EnumerateComponentPoint
            return true;
        }

        private IEnumerable<Conditional<ComponentPoint>> EnumerateComponentPoint(XAttribute x, XAttribute y, IConditionTreeItem baseConditions, IReadOnlyList<string> usedDefinitions)
        {
            if (!componentPointTemplateParser.TryParse(x, y, out var templatePoint))
                yield break;

            if (!templatePoint.Variables.Any())
            {
                if (componentPointParser.TryParse(x, y, out var componentPoint))
                    yield return new Conditional<ComponentPoint>(componentPoint, baseConditions);

                yield break;
            }

            var undeclaredXUsages = templatePoint.XVariables.Except(usedDefinitions).ToHashSet();
            if (!ReportUndeclared(x.GetFileRange(), undeclaredXUsages))
                yield break;

            var undeclaredYUsages = templatePoint.YVariables.Except(usedDefinitions).ToHashSet();
            if (!ReportUndeclared(y.GetFileRange(), undeclaredYUsages))
                yield break;

            var variable = templatePoint.Variables.First();
            foreach (var condition in definitionsSection.Value.Definitions[variable])
            {
                var cp = templatePoint.Construct(new Dictionary<string, double>
                {
                    [variable] = double.Parse(condition.Value),
                });

                var conditions = new ConditionTree(ConditionTree.ConditionOperator.AND, baseConditions, condition.Conditions);
                yield return new Conditional<ComponentPoint>(cp, conditions);
            }
        }

        private IEnumerable<Conditional<RenderText>> EnumerateRenderText(XElement commandElement, RenderText command, IConditionTreeItem baseConditions, IReadOnlyList<string> usedDefinitions)
        {
            var definitions = definitionsSection.Value.Definitions.Select(x => x.Key).ToHashSet();
            
            // Only one text run can use a definition variable
            if (command.TextRuns.Count(r => r.Text.StartsWith("$") && definitions.Contains(r.Text.Substring(1))) > 1)
            {
                logger.LogError(commandElement, "Only one definition per text element is permitted");
                yield break;
            }

            var replaceRun = command.TextRuns.FirstOrDefault(r => r.Text.StartsWith("$") && definitions.Contains(r.Text.Substring(1)));
            var replaceRunIndex = command.TextRuns.IndexOf(replaceRun);
            if (replaceRun == null)
            {
                // No definitions used
                yield return new Conditional<RenderText>(command, baseConditions);
                yield break;
            }

            if (!ReportUndeclared(commandElement.GetFileRange(), new HashSet<string>(new [] { replaceRun.Text.Substring(1) }).Except(usedDefinitions).ToHashSet()))
                yield break;

            foreach (var definitionValue in definitionsSection.Value.Definitions[replaceRun.Text.Substring(1)])
            {
                var conditions = new ConditionTree(ConditionTree.ConditionOperator.AND, baseConditions, definitionValue.Conditions);
                var replacementRun = new TextRun(definitionValue.Value, replaceRun.Formatting);
                var replacementCommand = (RenderText)command.Clone();
                replacementCommand.TextRuns.RemoveAt(replaceRunIndex);
                replacementCommand.TextRuns.Insert(replaceRunIndex, replacementRun);
                yield return new Conditional<RenderText>(replacementCommand, conditions);
            }
        }

        private bool ReportUndeclared(FileRange position, ISet<string> allUndeclared)
        {
            if (allUndeclared.Any())
            {
                foreach (var undeclared in allUndeclared)
                    logger.Log(LogLevel.Error, position, $"Usage of variable not present in the enclosing 'whenDefined' attribute: ${undeclared}", null);
                return false;
            }

            return true;
        }
    }
}
