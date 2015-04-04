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
            static readonly string toolboxSettingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\settings\\toolbox.xml";
#elif DEBUG
            static readonly string toolboxSettingsPath = Path.Combine(App.ProjectDirectory, "Components\\toolbox.xml");
#else
            static readonly string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
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

        public static void WriteToolbox(IEnumerable<IEnumerable<IdentifierWithShortcut>> items)
        {
            using (System.IO.FileStream stream = new System.IO.FileStream(toolboxSettingsPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("display");

                foreach (var category in items)
                {
                    if (category.Count() == 0)
                        continue;

                    writer.WriteStartElement("category");

                    foreach (var item in category)
                    {
                        writer.WriteStartElement("component");

                        if (item.Identifier.Description.Metadata.GUID != Guid.Empty)
                            writer.WriteAttributeString("guid", item.Identifier.Description.Metadata.GUID.ToString());
                        else
                            writer.WriteAttributeString("type", item.Identifier.Description.ComponentName);

                        if (item.Identifier.Configuration != null)
                            writer.WriteAttributeString("configuration", item.Identifier.Configuration.Name);

                        if (item.ShortcutKey != Key.None)
                            writer.WriteAttributeString("key", item.ShortcutKey.ToString());

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();

                writer.Flush();
            }
        }
    }
}
