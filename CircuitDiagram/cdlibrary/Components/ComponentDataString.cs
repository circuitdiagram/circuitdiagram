// ComponentDataString.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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

namespace CircuitDiagram.Components
{
    public static class ComponentDataString
    {
        public static string ConvertToString(Dictionary<string, object> properties)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object> property in properties)
            {
                builder.AppendLine(property.Key + ":" + property.Value.ToString());
            }
            return builder.ToString();
        }

        public static string ConvertToString(Dictionary<string, object> properties, string separator)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, object> property in properties)
            {
                builder.Append(property.Key + ":" + property.Value.ToString() + separator);
            }
            // Remove last separator
            if (builder.Length > 0)
                builder.Remove(builder.Length - separator.Length, separator.Length);
            return builder.ToString();
        }

        public static Dictionary<string, object> ConvertToDictionary(string description)
        {
            string[] propertyPairs = description.Split(new string[] { "\r\n", "," }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (string property in propertyPairs)
            {
                string[] propertySplit = property.Split(':');
                properties.Add(propertySplit[0].Trim(), propertySplit[1].Trim());
            }
            return properties;
        }
    }
}
