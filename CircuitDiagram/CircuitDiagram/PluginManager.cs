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

        [ImportMany(typeof(IDocumentWriter))]
        IList<IDocumentWriter> m_ExportWriters;

        [ImportMany(typeof(IDocumentReader))]
        IList<IDocumentReader> m_ImportReaders;

        Dictionary<IDocumentWriter, bool> m_exportWritersEnabled;

        Dictionary<IDocumentReader, bool> m_importReadersEnabled;

        public static IList<IDocumentWriter> ExportWriters { get { return m_manager.m_ExportWriters; } }

        public static IList<IDocumentReader> ImportReaders { get { return m_manager.m_ImportReaders; } }

        public static IDictionary<IDocumentWriter, bool> ExportWritersEnabled { get { return m_manager.m_exportWritersEnabled; } }

        public static IDictionary<IDocumentReader, bool> ImportReadersEnabled { get { return m_manager.m_importReadersEnabled; } }

        public static IEnumerable<IDocumentWriter> EnabledExportWriters { get { return m_manager.m_ExportWriters.Where(writer => m_manager.m_exportWritersEnabled[writer] == true); } }

        public static IEnumerable<IDocumentReader> EnabledImportReaders { get { return m_manager.m_ImportReaders.Where(reader => m_manager.m_importReadersEnabled[reader] == true); } }

        public static void Initialize()
        {
            m_manager = new PluginManager();

            try
            {
                m_manager.m_ExportWriters = new List<IDocumentWriter>();
                m_manager.m_ImportReaders = new List<IDocumentReader>();
                AggregateCatalog catalogue = new AggregateCatalog();
                catalogue.Catalogs.Add(new DirectoryCatalog(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\plugins"));
                CompositionContainer container = new CompositionContainer(catalogue);
                container.ComposeParts(m_manager);

                m_manager.m_exportWritersEnabled = new Dictionary<IDocumentWriter, bool>();
                foreach (IDocumentWriter writer in m_manager.m_ExportWriters)
                    if (Settings.Settings.HasSetting("PluginManager." + writer.GUID.ToString()) && Settings.Settings.ReadBool("PluginManager." + writer.GUID.ToString()))
                        m_manager.m_exportWritersEnabled.Add(writer, true);
                    else
                        m_manager.m_exportWritersEnabled.Add(writer, false);

                m_manager.m_importReadersEnabled = new Dictionary<IDocumentReader, bool>();
                foreach (IDocumentReader reader in m_manager.m_ExportWriters)
                    if (Settings.Settings.HasSetting("PluginManager." + reader.GUID.ToString()) && Settings.Settings.ReadBool("PluginManager." + reader.GUID.ToString()))
                        m_manager.m_importReadersEnabled.Add(reader, true);
                    else
                        m_manager.m_importReadersEnabled.Add(reader, false);
            }
            catch (Exception)
            {
            }
        }

        public static void SaveSettings()
        {
            foreach (IDocumentWriter writer in m_manager.m_ExportWriters)
                Settings.Settings.Write("PluginManager." + writer.GUID.ToString(), m_manager.m_exportWritersEnabled[writer]);
            foreach (IDocumentReader reader in m_manager.m_ImportReaders)
                Settings.Settings.Write("PluginManager." + reader.GUID.ToString(), m_manager.m_importReadersEnabled[reader]);
        }
    }
}
