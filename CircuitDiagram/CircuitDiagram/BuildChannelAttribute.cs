using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    [AttributeUsage(AttributeTargets.Assembly)]
    class BuildChannelAttribute : Attribute
    {
        public int Increment { get; set; }
        public string DisplayName { get; set; }
        public ChannelType Type { get; set; }

        public BuildChannelAttribute(string displayName, ChannelType channelType, int increment)
        {
            DisplayName = displayName;
            Type = channelType;
            Increment = increment;
        }

        public enum ChannelType
        {
            Stable,
            Dev
        }
    }
}
