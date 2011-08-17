using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    public static class ComponentStringDescription
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

        public static Dictionary<string, object> ConvertToDictionary(string description)
        {
            string[] propertyPairs = description.Split(new string[] { "\r\n", "," }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (string property in propertyPairs)
            {
                string[] propertySplit = property.Split(':');
                properties.Add(propertySplit[0], propertySplit[1]);
            }
            return properties;
        }
    }
}
