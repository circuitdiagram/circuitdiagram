using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    [AttributeUsage(AttributeTargets.Assembly)]
    class BuildChannelAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public UpdateChannelType UpdateChannel { get; set; }

        public BuildChannelAttribute(string displayName, UpdateChannelType channelType)
        {
            DisplayName = displayName;
            UpdateChannel = channelType;
        }
    }
}
