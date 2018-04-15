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
using Ns = CircuitDiagram.Document.Namespaces;

namespace CircuitDiagram.Document.InternalReader
{
    class DefinitionsReader
    {
        public void ReadDefinitions(CircuitDiagramDocument document,
                                    XElement definitions,
                                    ReaderContext context)
        {
            var sources = from el in definitions.Elements()
                          where el.Name == Ns.Document + "src"
                          select el;

            foreach (var source in sources)
            {
                var collection = ComponentType.UnknownCollection;
                var collectionAttr = source.Attribute("col");
                if (collectionAttr != null)
                {
                    if (!Uri.TryCreate(collectionAttr.Value, UriKind.Absolute, out collection))
                        context.Log(ReaderErrorCodes.UnableToParseValueAsUri, collectionAttr, collectionAttr.Value);
                }

                foreach (var componentType in source.Elements().Where(el => el.Name == Ns.Document + "add"))
                {
                    var idAttr = componentType.Attribute("id");
                    if (idAttr == null)
                    {
                        context.Log(ReaderErrorCodes.MissingRequiredAttribute, componentType, "id");
                        continue;
                    }
                    string id = idAttr.Value;

                    var guid = componentType.GetGuidAttribute(Ns.DocumentComponentDescriptions + "guid", context);
                    var collectionItem = componentType.GetCollectionItemAttribute("item", context);
                    var name = componentType.GetComponentNameAttribute("name", context);

                    var type = collectionItem != null ? new ComponentType(collection ?? ComponentType.UnknownCollection, collectionItem) : ComponentType.Unknown(name ?? guid.ToString());

                    if (guid.HasValue)
                    {
                        type = new TypeDescriptionComponentType(guid ?? Guid.Empty, type);
                    }

                    context.RegisterComponentType(id, type);
                }
            }
        }
    }
}
