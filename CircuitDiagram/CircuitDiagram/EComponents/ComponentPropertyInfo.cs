using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CircuitDiagram.EComponents
{
    public class ComponentPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; set; }
        public string SerializeAs { get; set; }
        public string DisplayName { get; set; }
        public bool DisplayAlignLeft { get; set; }

        public ComponentPropertyInfo(string serializeAs, string displayName, bool alignLeft, PropertyInfo propertyInfo)
        {
            SerializeAs = serializeAs;
            DisplayName = displayName;
            PropertyInfo = propertyInfo;
            DisplayAlignLeft = alignLeft;
        }
    }
}
