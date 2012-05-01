using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CircuitDiagram.Components;

namespace CircuitDiagram
{
    public class EmbedComponentOptions
    {
        public ComponentsToEmbed EmbedComponents { get; set; }

        public EmbedComponentOptions()
        {
            EmbedComponents = ComponentsToEmbed.Automatic;
        }

        public static EmbedComponentOptions LoadFromSettings()
        {
            EmbedComponentOptions returnOptions = new EmbedComponentOptions();
            if (Settings.Settings.HasSetting("EmbedComponents"))
                returnOptions.EmbedComponents = (ComponentsToEmbed)Settings.Settings.Read("EmbedComponents");

            return returnOptions;
        }

        public void SaveToSettings()
        {
            Settings.Settings.Write("EmbedComponents", (int)EmbedComponents);
        }
    }

    public enum ComponentsToEmbed
    {
        Automatic,
        All,
        None,
        Custom
    }
}
