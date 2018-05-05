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

using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions
{
    public interface IConditionParser
    {
        IConditionTreeItem Parse(ComponentDescription description, string input);
    }

    public static class ConditionParserExtensions
    {
        public static bool Parse(this IConditionParser parser, XAttribute conditionsAttribute, ComponentDescription description, IXmlLoadLogger logger, out IConditionTreeItem value)
        {
            try
            {
                value = parser.Parse(description, conditionsAttribute.Value);
                return true;
            }
            catch (ConditionFormatException ex)
            {
                IXmlLineInfo line = conditionsAttribute;
                int startCol = line.LinePosition + conditionsAttribute.Name.LocalName.Length + 2 + ex.PositionStart;
                var position = new FileRange(line.LineNumber, startCol, line.LineNumber, startCol + ex.Length);
                logger.Log(LogLevel.Error, position, ex.Message, ex);
                value = null;
                return false;
            }
        }
    }
}
