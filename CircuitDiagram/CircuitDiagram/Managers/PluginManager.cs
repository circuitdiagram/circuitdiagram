using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using CircuitDiagram.IO;

namespace CircuitDiagram
{
    class PluginManager
    {
        private static PluginManager m_manager;

        [ImportMany(typeof(IPlugin))]
        List<IPlugin> m_plugins;

        Dictionary<IPlugin, bool> m_pluginsEnabled;

        public static List<IPlugin> Plugins { get { return m_manager.m_plugins; } }

        public static List<IDocumentWriter> EnabledExportWriters { get; private set; }

        public static List<IDocumentReader> EnabledImportReaders { get; private set; }

        public static void Initialize()
        {
            m_manager = new PluginManager();
            m_manager.m_plugins = new List<IPlugin>();
            m_manager.m_pluginsEnabled = new Dictionary<IPlugin, bool>();
            EnabledExportWriters = new List<IDocumentWriter>();
            EnabledImportReaders = new List<IDocumentReader>();

            try
            {
                string pluginDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\plugins";

                // Check if plugin directory exists
                if (!System.IO.Directory.Exists(pluginDirectory))
                    return;

                AggregateCatalog catalogue = new AggregateCatalog();
                catalogue.Catalogs.Add(new DirectoryCatalog(pluginDirectory));
                CompositionContainer container = new CompositionContainer(catalogue);
                container.ComposeParts(m_manager);

                m_manager.m_pluginsEnabled = new Dictionary<IPlugin, bool>();
                foreach (IPlugin plugin in m_manager.m_plugins)
                {
                    m_manager.m_pluginsEnabled.Add(plugin, false);
                    if (Settings.Settings.HasSetting("PluginManager." + plugin.GUID.ToString()) && Settings.Settings.ReadBool("PluginManager." + plugin.GUID.ToString()))
                        m_manager.m_pluginsEnabled[plugin] = true;
                }

                Update();
            }
            catch (Exception)
            {
            }
        }

        public static void Update()
        {
            EnabledImportReaders.Clear();
            EnabledExportWriters.Clear();

            foreach (IPlugin plugin in Plugins.Where(plugin => m_manager.m_pluginsEnabled[plugin] == true))
            {
                foreach (IPluginPart pluginPart in plugin.PluginParts)
                {
                    if (pluginPart is IDocumentReader)
                        EnabledImportReaders.Add(pluginPart as IDocumentReader);
                    else if (pluginPart is IDocumentWriter)
                        EnabledExportWriters.Add(pluginPart as IDocumentWriter);
                }
            }
        }

        public static void SaveSettings()
        {
            foreach (var plugin in m_manager.m_pluginsEnabled)
                Settings.Settings.Write("PluginManager." + plugin.Key.GUID.ToString(), plugin.Value);
        }

        public static bool IsPluginEnabled(IPlugin item)
        {
            return m_manager.m_pluginsEnabled[item];
        }

        public static void SetPluginEnabled(IPlugin item, bool enabled)
        {
            m_manager.m_pluginsEnabled[item] = enabled;
        }
    }
}
