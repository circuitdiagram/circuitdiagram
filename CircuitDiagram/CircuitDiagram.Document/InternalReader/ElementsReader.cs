// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document.ReaderErrors;
using CircuitDiagram.Primitives;
using Ns = CircuitDiagram.Document.Namespaces;

namespace CircuitDiagram.Document.InternalReader
{
    class ElementsReader
    {
        public void ReadElements(CircuitDiagramDocument document,
                                 XElement elements,
                                 ReaderContext context)
        {
            var components = from el in elements.Elements()
                             where el.Name == Ns.Document + "c"
                             select el;

            foreach (var componentElement in components)
            {
                var typeAttr = componentElement.Attribute("tp");
                if (typeAttr == null)
                {
                    context.Log(ReaderErrorCodes.MissingRequiredAttribute, componentElement, "tp");
                    continue;
                }

                var componentType = context.GetComponentType(ParseType(typeAttr.Value));
                if (componentType == null)
                {
                    context.Log(ReaderErrorCodes.UnknownComponentType, typeAttr, typeAttr.Value);
                    continue;
                }

                Component component;

                if (componentElement.Attribute("x") != null)
                {
                    component = new PositionalComponent(componentType, null, new Point(0, 0));

                    // Layout
                    ReadLayout((PositionalComponent)component, componentElement, context);
                }
                else
                    component = new Component(componentType, null);

                // Properties

                var propertiesElement = componentElement.Elements(Ns.Document + "prs").SingleOrDefault();
                var properties = propertiesElement != null
                    ? from el in propertiesElement.Elements()
                      where el.Name == Ns.Document + "p"
                      select el
                    : new XElement[0];

                foreach (var propertyElement in properties)
                {
                    var keyAttr = propertyElement.Attribute("k");
                    if (keyAttr == null)
                    {
                        context.Log(ReaderErrorCodes.MissingRequiredAttribute, propertyElement, "k");
                        continue;
                    }

                    var valueAttr = propertyElement.Attribute("v");
                    if (valueAttr == null)
                    {
                        context.Log(ReaderErrorCodes.MissingRequiredAttribute, propertyElement, "v");
                        continue;
                    }

                    if (!component.Type.PropertyNames.Contains(keyAttr.Value))
                        component.Type.PropertyNames.Add(keyAttr.Value);
                    component.Properties[keyAttr.Value] = PropertyValue.Dynamic(valueAttr.Value);
                }

                // Connections

                var connectionsElement = componentElement.Elements(Ns.Document + "cns").SingleOrDefault();
                var connections = connectionsElement != null
                    ? from el in connectionsElement.Elements()
                      where el.Name == Ns.Document + "cn"
                      select el
                    : new XElement[0];

                foreach (var connectionElement in connections)
                {
                    var idAttr = connectionElement.Attribute("id");
                    if (idAttr == null)
                    {
                        context.Log(ReaderErrorCodes.MissingRequiredAttribute, connectionElement, "id");
                        continue;
                    }

                    var pointAttr = connectionElement.Attribute("pt");
                    if (pointAttr == null)
                    {
                        context.Log(ReaderErrorCodes.MissingRequiredAttribute, connectionElement, "pt");
                        continue;
                    }

                    if (component.Connections.All(x => x.Value.Name.Value != pointAttr.Value))
                    {
                        // Add new connection definition to component type
                        component.Type.ConnectionNames.Add(new ConnectionName(pointAttr.Value));
                    }

                    context.ApplyConnection(idAttr.Value,
                        component.Connections.First(x => x.Value.Name.Value == pointAttr.Value).Value);
                }

                document.Elements.Add(component);
            }

            var wires = from el in elements.Elements()
                             where el.Name == Ns.Document + "w"
                             select el;

            foreach (var wireElement in wires)
            {
                var wire = new Wire(new Point());
                ReadLayout(wire, wireElement, context);

                document.Elements.Add(wire);
            }
        }

        private void ReadLayout(IPositionalElement positionalElement, XElement element, ReaderContext context)
        {
            int? x = element.GetIntAttribute("x", context);
            int? y = element.GetIntAttribute("y", context);
            int? size = element.GetIntAttribute("sz", context);
            bool? flipped = element.GetBoolAttribute("flp", context);
            Orientation? orientation = element.GetComponentOrientationAttribute("o", context);

            if (!x.HasValue || !y.HasValue)
                return;

            positionalElement.Layout.Location = positionalElement.Layout.Location.WithNewX(x.Value);
            positionalElement.Layout.Location = positionalElement.Layout.Location.WithNewY(y.Value);
            positionalElement.Layout.Size = size ?? 0;
            positionalElement.Layout.IsFlipped = flipped ?? false;
            positionalElement.Layout.Orientation = orientation ?? Orientation.Horizontal;
        }

        private static string ParseType(string value)
        {
            return value.Replace("{", "").Replace("}", "");
        }
    }
}
