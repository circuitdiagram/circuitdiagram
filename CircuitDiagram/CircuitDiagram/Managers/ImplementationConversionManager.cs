#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion

using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CircuitDiagram
{
    static class ImplementationConversionManager
    {
        private static readonly string implementationsFilePath;

        static ImplementationConversionManager()
        {
#if PORTABLE
            implementationsFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\settings\\implementations.xml";
#elif DEBUG
            implementationsFilePath = Path.Combine(App.ProjectDirectory, "Components\\implementations.xml");
#else
            implementationsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\implementations.xml";
#endif
        }

        public static List<ImplementationConversionCollection> LoadImplementationConversions()
        {
            var implementationConversions = new List<ImplementationConversionCollection>();

            if (File.Exists(implementationsFilePath))
            {
                try
                {
                    XmlDocument implDoc = new XmlDocument();
                    implDoc.Load(implementationsFilePath);

                    XmlNodeList sourceNodes = implDoc.SelectNodes("/implementations/source");
                    foreach (XmlNode sourceNode in sourceNodes)
                    {
                        string collection = sourceNode.Attributes["definitions"].InnerText;

                        ImplementationConversionCollection newCollection = new ImplementationConversionCollection();
                        newCollection.ImplementationSet = collection;

                        foreach (XmlNode childNode in sourceNode.ChildNodes)
                        {
                            if (childNode.Name != "add")
                                continue;

                            string item = childNode.Attributes["item"].InnerText;
                            Guid guid = Guid.Empty;
                            XmlNode guidNode = childNode.SelectSingleNode("guid");
                            if (guidNode != null)
                                guid = new Guid(guidNode.InnerText);
                            string configuration = null;
                            XmlNode configurationNode = childNode.SelectSingleNode("configuration");
                            if (configurationNode != null)
                                configuration = configurationNode.InnerText;

                            ComponentDescription description = ComponentHelper.FindDescription(guid);
                            if (description != null)
                            {
                                ImplementationConversion newConversion = new ImplementationConversion();
                                newConversion.ImplementationName = item;
                                newConversion.ToName = description.ComponentName;
                                newConversion.ToGUID = description.Metadata.GUID;

                                ComponentConfiguration theConfiguration = null;
                                if (configuration != null)
                                {
                                    theConfiguration = description.Metadata.Configurations.FirstOrDefault(check => check.Name == configuration);
                                    if (theConfiguration != null)
                                    {
                                        newConversion.ToConfiguration = theConfiguration.Name;
                                        if (theConfiguration.Icon != null)
                                            newConversion.ToIcon = theConfiguration.Icon;
                                        else
                                            newConversion.ToIcon = description.Metadata.Icon;
                                    }
                                    else if (description.Metadata.Icon != null)
                                        newConversion.ToIcon = description.Metadata.Icon;
                                }
                                else if (description.Metadata.Icon != null)
                                    newConversion.ToIcon = description.Metadata.Icon;

                                newCollection.Items.Add(newConversion);
                                ComponentHelper.SetStandardComponent(newCollection.ImplementationSet, newConversion.ImplementationName, description, theConfiguration);
                            }
                        }

                        implementationConversions.Add(newCollection);
                    }
                }
                catch (Exception)
                {
                    // Invalid XML file
                }
            }

            return implementationConversions;
        }
    
        public static void SaveImplementationConversions(List<ImplementationConversionCollection> implementationConversions)
        {
            XmlTextWriter writer = new XmlTextWriter(implementationsFilePath, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("implementations");
            foreach (var source in implementationConversions)
            {
                if (source.Items.Count == 0)
                    continue;

                writer.WriteStartElement("source");

                writer.WriteAttributeString("definitions", source.ImplementationSet);

                foreach (var item in source.Items)
                {
                    writer.WriteStartElement("add");

                    writer.WriteAttributeString("item", item.ImplementationName);
                    writer.WriteStartElement("guid");
                    writer.WriteValue(item.ToGUID.ToString());
                    writer.WriteEndElement();
                    if (!String.IsNullOrEmpty(item.ToConfiguration))
                    {
                        writer.WriteStartElement("configuration");
                        writer.WriteValue(item.ToConfiguration);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            writer.Flush();
            writer.Close();
        }
    }
}
