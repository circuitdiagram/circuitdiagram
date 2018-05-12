// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Antlr4.Runtime;
using Autofac.Features.AttributeFilters;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Features;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions.ComponentPoints
{
    public class ComponentPointTemplateParser : IComponentPointTemplateParser, IComponentPointParser
    {
        private readonly IComponentPointParser baseParser;
        private readonly IFeatureSwitcher featureSwitcher;
        private readonly IXmlLoadLogger logger;

        public ComponentPointTemplateParser([KeyFilter("default")] IComponentPointParser baseParser, IFeatureSwitcher featureSwitcher, IXmlLoadLogger logger)
        {
            this.baseParser = baseParser;
            this.featureSwitcher = featureSwitcher;
            this.logger = logger;
        }

        public bool TryParse(string x, string y, IXmlLineInfo xLine, IXmlLineInfo yLine, out ComponentPointTemplate componentPointTemplate)
        {
            var errors = new StringWriter();

            try
            {
                var lexer = new ComponentPointLexer(new AntlrInputStream(new StringReader(x)), errors, errors);
                var parser = new ComponentPointParser(new CommonTokenStream(lexer), errors, errors);
                var ctx = parser.r();
                var xPosition = (ComponentPosition)Enum.Parse(typeof(ComponentPosition), ctx.position().GetText(), true);
                var xModifiers = new List<IModifierToken>();
                var currentModifier = ctx.modifiers();
                while (currentModifier.modifier() != null)
                {
                    var modifier = currentModifier.modifier();
                    bool positive = modifier.plusminus().GetText() == "+";

                    if (modifier.CONSTANT() != null)
                    {
                        xModifiers.Add(new ConstantModifierToken
                        {
                            Offset = (positive ? 1 : -1) * double.Parse(modifier.CONSTANT().GetText())
                        });
                    }
                    else
                    {
                        xModifiers.Add(new DefinitionModifierToken
                        {
                            Negated = !positive,
                            Name = modifier.variable().VARIABLENAME().GetText()
                        });
                    }

                    currentModifier = currentModifier.modifiers();
                }

                lexer = new ComponentPointLexer(new AntlrInputStream(new StringReader(y)), errors, errors);
                parser = new ComponentPointParser(new CommonTokenStream(lexer), errors, errors);
                ctx = parser.r();
                var yPosition = (ComponentPosition)Enum.Parse(typeof(ComponentPosition), ctx.position().GetText(), true);
                var yModifiers = new List<IModifierToken>();
                currentModifier = ctx.modifiers();
                while (currentModifier.modifier() != null)
                {
                    var modifier = currentModifier.modifier();
                    bool positive = modifier.plusminus().GetText() == "+";

                    if (modifier.CONSTANT() != null)
                    {
                        yModifiers.Add(new ConstantModifierToken
                        {
                            Offset = (positive ? 1 : -1) * double.Parse(modifier.CONSTANT().GetText())
                        });
                    }
                    else
                    {
                        yModifiers.Add(new DefinitionModifierToken
                        {
                            Negated = !positive,
                            Name = modifier.variable().VARIABLENAME().GetText()
                        });
                    }

                    currentModifier = currentModifier.modifiers();
                }
                
                componentPointTemplate = new ComponentPointTemplate(xPosition, yPosition, xModifiers, yModifiers);
                return true;
            }
            catch
            {
                componentPointTemplate = null;
                return false;
            }
            finally
            {
                foreach (var message in errors.ToString().Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var position = new FileRange(xLine.LineNumber, xLine.LinePosition, xLine.LineNumber, 10);
                    logger.Log(LogLevel.Error, position, message, null);
                }
            }
        }

        public bool TryParse(string location, IXmlLineInfo line, out ComponentPointTemplate componentPointTemplate)
        {
            if (!baseParser.TryParse(location, line, out var componentPoint))
            {
                componentPointTemplate = null;
                return false;
            }

            componentPointTemplate = new ComponentPointTemplate(componentPoint.RelativeToX, componentPoint.RelativeToY, new IModifierToken[]
            {
                new ConstantModifierToken
                {
                    Offset = componentPoint.Offset.X
                },
            }, new IModifierToken[]
            {
                new ConstantModifierToken
                {
                    Offset = componentPoint.Offset.Y
                }
            });

            return true;
        }
        
        bool IComponentPointParser.TryParse(string x, string y, IXmlLineInfo xLine, IXmlLineInfo yLine, out ComponentPoint componentPoint)
        {
            if (!featureSwitcher.IsFeatureEnabled(DefinitionsXmlLoaderExtensions.FeatureName))
                return baseParser.TryParse(x, y, xLine, yLine, out componentPoint);

            if (!TryParse(x, y, xLine, yLine, out ComponentPointTemplate templated) || templated.Variables.Any())
            {
                componentPoint = null;
                return false;
            }

            componentPoint = templated.Construct(new Dictionary<string, double>());
            return true;
        }

        bool IComponentPointParser.TryParse(string location, IXmlLineInfo line, out ComponentPoint componentPoint)
        {
            if (featureSwitcher.IsFeatureEnabled(DefinitionsXmlLoaderExtensions.FeatureName))
            {
                location = location.Replace("start", "_Start")
                                   .Replace("middle", "_Middle")
                                   .Replace("end", "_End");
            }

            return baseParser.TryParse(location, line, out componentPoint);
        }
    }
}
