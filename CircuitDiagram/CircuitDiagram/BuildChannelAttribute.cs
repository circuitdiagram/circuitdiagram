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
        public UpdateChannelType UpdateChannel { get; set; }

        public BuildChannelAttribute(string displayName, UpdateChannelType channelType, int increment)
        {
            DisplayName = displayName;
            UpdateChannel = channelType;
            Increment = increment;
        }
    }
}
