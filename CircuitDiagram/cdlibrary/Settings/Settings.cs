// Settings.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace CircuitDiagram.Settings
{
    public static class Settings
    {
        private static string m_file;
        private static Dictionary<string, object> m_values;

        public static void Initialize(string file)
        {
            m_file = file;
            m_values = new Dictionary<string, object>();

            if (File.Exists(file))
            {
                // load settings
                XmlTextReader reader = new XmlTextReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                try
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "setting")
                        {
                            reader.MoveToAttribute("key");
                            string key = reader.Value;
                            reader.MoveToAttribute("type");
                            string type = reader.Value;
                            if (type == "string")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, value);
                            }
                            else if (type == "int")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, int.Parse(value));
                            }
                            else if (type == "bool")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, bool.Parse(value));
                            }
                            else if (type == "string-array")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                string[] values = value.Split(',');
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, values);
                            }
                            else if (type == "double")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, double.Parse(value));
                            }
                            else if (type == "int-array")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                string[] values = value.Split(',');
                                int[] parsedValues = new int[values.Length];
                                for (int i = 0; i < values.Length; i++)
                                    parsedValues[i] = int.Parse(values[i]);
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, parsedValues);
                            }
                            else if (type == "double-array")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                string[] values = value.Split(',');
                                double[] parsedValues = new double[values.Length];
                                for (int i = 0; i < values.Length; i++)
                                    parsedValues[i] = double.Parse(values[i]);
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, parsedValues);
                            }
                            else if (type == "DateTime")
                            {
                                reader.MoveToAttribute("value");
                                string value = reader.Value;
                                if (!m_values.ContainsKey(key))
                                    m_values.Add(key, DateTime.Parse(value));
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    // invalid xml
                }
            }
            else
            {
                // create new settings file
                
            }
        }

        public static void Save()
        {
            XmlTextWriter writer = new XmlTextWriter(m_file, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("settings");
            foreach(KeyValuePair<string, object> pair in m_values)
            {
                writer.WriteStartElement("setting");
                writer.WriteAttributeString("key", pair.Key);
                Type settingType = pair.Value.GetType();
                string typeString = "string";
                string valueString = pair.Value.ToString();
                if (settingType == typeof(int[]))
                {
                    typeString = "int-array";
                    valueString = "";
                    foreach (int value in ((int[])pair.Value))
                        valueString += (valueString.Length > 0 ? "," : "") + value.ToString();
                }
                else if (settingType == typeof(double[]))
                {
                    typeString = "double-array";
                    valueString = "";
                    foreach (double value in ((double[])pair.Value))
                        valueString += (valueString.Length > 0 ? "," : "") + value.ToString();
                }
                else if (settingType == typeof(string[]))
                {
                    typeString = "string-array";
                    valueString = "";
                    foreach (string value in ((string[])pair.Value))
                        valueString += (valueString.Length > 0 ? "," : "") + value;
                }
                else if (settingType == typeof(int))
                {
                    typeString = "int";
                }
                else if (settingType == typeof(double))
                {
                    typeString = "double";
                }
                else if (settingType == typeof(bool))
                {
                    typeString = "bool";
                }
                else if (settingType == typeof(DateTime))
                {
                    typeString = "DateTime";
                }
                writer.WriteAttributeString("type", typeString);
                writer.WriteAttributeString("value", valueString);
            }
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        public static object Read(string key)
        {
            object value;
            if (m_values.TryGetValue(key, out value))
                return value;
            return null;
        }

        public static void Write(string key, object value)
        {
            if (m_values.ContainsKey(key))
                m_values[key] = value;
            else
                m_values.Add(key, value);
        }

        public static bool ReadBool(string key)
        {
            object value = Read(key);
            if (value == null)
                return false;
            return (bool)value;
        }

        public static bool HasSetting(string key)
        {
            return m_values.ContainsKey(key);
        }
    }
}
