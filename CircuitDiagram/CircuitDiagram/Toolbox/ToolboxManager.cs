using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace CircuitDiagram
{
    static class ToolboxManager
    {
#if PORTABLE
            string toolboxSettingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\settings\\toolbox.xml";
#elif DEBUG
        static readonly string toolboxSettingsPath = Path.Combine(MainWindow.ProjectDirectory, "Components\\toolbox.xml");
#else
            string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
#endif

        /// <summary>
        /// Populates the toolbox from the xml file.
        /// </summary>
        public static List<List<IToolboxItem>> LoadToolbox()
        {
            var result = new List<List<IToolboxItem>>();

            XmlDocument toolboxSettings = new XmlDocument();
            toolboxSettings.Load(toolboxSettingsPath);

            XmlNodeList categoryNodes = toolboxSettings.SelectNodes("/display/category");
            foreach (XmlNode categoryNode in categoryNodes)
            {
                var newCategory = new List<IToolboxItem>();
                foreach (XmlNode node in categoryNode.ChildNodes)
                {
                    if (node.Name == "component")
                    {
                        XmlElement element = node as XmlElement;

                        ComponentDescription description;

                        if (element.HasAttribute("guid"))
                            description = ComponentHelper.FindDescription(new Guid(element.Attributes["guid"].InnerText));
                        else if (element.HasAttribute("type"))
                            description = ComponentHelper.FindDescription(element.Attributes["type"].InnerText);
                        else
                            continue;

                        ComponentConfiguration configuration = null;
                        if (element.HasAttribute("configuration"))
                            configuration = description.Metadata.Configurations.FirstOrDefault(configItem => configItem.Name == element.Attributes["configuration"].InnerText);

                        var newItem = new IdentifierWithShortcut();
                        newItem.Identifier = new ComponentIdentifier(description, configuration);

                        if (newItem.Icon.LoadedIcons.Count == 0)
                            newItem.Icon.LoadIcons();

                        // Shortcut
                        if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                        {
                            Key key = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);
                            newItem.ShortcutKey = key;
                        }

                        newCategory.Add(newItem);
                    }
                }

                if (newCategory.Count > 0)
                    result.Add(newCategory);
            }

            return result;
        }

        public static void OverwriteToolbox()
        {
            if (!Directory.Exists(Path.GetDirectoryName(toolboxSettingsPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(toolboxSettingsPath));
            File.WriteAllText(toolboxSettingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><display></display>");
        }
    }
}