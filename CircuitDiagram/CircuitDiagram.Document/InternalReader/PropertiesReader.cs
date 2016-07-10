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
using CircuitDiagram.Document.ReaderErrors;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Document.InternalReader
{
    class PropertiesReader
    {
        public void ReadProperties(CircuitDiagramDocument document,
                                   XElement properties,
                                   ReaderContext context)
        {
            int widthValue = 0;
            int heightValue = 0;

            try
            {
                var widthEl = (from el in properties.Elements()
                               where el.Name == Namespaces.Document + "width"
                               select el).Single();


                if (!int.TryParse(widthEl.Value, out widthValue))
                    context.Log(ReaderErrorCodes.UnableToParseValueAsInt, widthEl, widthEl.Value);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("more than one"))
            {
                context.Log(ReaderErrorCodes.DuplicateElement, properties, "width");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("no matching"))
            {
                context.Log(ReaderErrorCodes.MissingRequiredElement, properties, "width");
            }

            try
            {
                var heightEl = (from el in properties.Elements()
                                where el.Name == Namespaces.Document + "height"
                                select el).Single();

                if (!int.TryParse(heightEl.Value, out heightValue))
                    context.Log(ReaderErrorCodes.UnableToParseValueAsInt, heightEl, heightEl.Value);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("more than one"))
            {
                context.Log(ReaderErrorCodes.DuplicateElement, properties, "height");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("no matching"))
            {
                context.Log(ReaderErrorCodes.MissingRequiredElement, properties, "height");
            }

            document.Size = new Size(widthValue, heightValue);
        }
    }
}
