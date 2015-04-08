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
using CircuitDiagram.IO.Descriptions.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CircuitDiagram
{
    /// <summary>
    /// Provides methods for loading component descriptions.
    /// </summary>
    static class ComponentsManager
    {
        private static readonly IEnumerable<string> componentDirectories;

        static ComponentsManager()
        {
            List<string> directories = new List<string>();

            // Components shipped with Circuit Diagram are placed in the program directory
            string permanentComponentsDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext";
            if (Directory.Exists(permanentComponentsDirectory))
                directories.Add(permanentComponentsDirectory);

#if !PORTABLE
            // User components are placed in the App Data folder
            string userComponentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\components";
            if (Directory.Exists(userComponentsDirectory))
                directories.Add(userComponentsDirectory);
#else
            // User components for the portable version are placed in the program directory
            string portableComponentsDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\components";
            if (Directory.Exists(portableComponentsDirectory))
                componentLocations.Add(portableComponentsDirectory);
#endif

#if DEBUG
            // Components compiled for debug builds are placed in the project directory
            string debugComponentsDirectory = Path.Combine(App.ProjectDirectory, "Components\\Output");
            if (Directory.Exists(debugComponentsDirectory))
                directories.Add(debugComponentsDirectory);
#endif

            componentDirectories = directories;
        }

        public static void LoadComponents()
        {
            bool conflictingGuid = false;

            conflictingGuid |= LoadXmlComponents();
            conflictingGuid |= LoadBinaryComponents();

            if (conflictingGuid)
                throw new ComponentDescriptionLoadException("Two or more components have the same GUID.");
        }

        private static bool LoadXmlComponents()
        {
            bool conflictingGuid = false;

            var xmlLoader = new XmlLoader();
            foreach (string location in componentDirectories)
            {
                foreach (string file in System.IO.Directory.GetFiles(location, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        xmlLoader.Load(fs);
                        if (xmlLoader.LoadErrors.Count() == 0)
                        {
                            ComponentDescription description = xmlLoader.GetDescriptions()[0];
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = new ComponentDescriptionSource(file, new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(new ComponentDescription[] { description }));

                            // Check if duplicate GUID
                            if (!conflictingGuid && description.Metadata.GUID != Guid.Empty)
                            {
                                foreach (ComponentDescription compareDescription in ComponentHelper.ComponentDescriptions)
                                    if (compareDescription.Metadata.GUID == description.Metadata.GUID)
                                        conflictingGuid = true;
                            }

                            ComponentHelper.AddDescription(description);
                            if (ComponentHelper.WireDescription == null && description.ComponentName.ToLowerInvariant() == "wire" && description.Metadata.GUID == new Guid("6353882b-5208-4f88-a83b-2271cc82b94f"))
                                ComponentHelper.WireDescription = description;
                        }
                    }
                }
            }

            return conflictingGuid;
        }

        private static bool LoadBinaryComponents()
        {
            bool conflictingGuid = false;

            X509Chain certChain = new X509Chain();
            foreach (string location in componentDirectories)
            {
                foreach (string file in System.IO.Directory.GetFiles(location, "*.cdcom", SearchOption.TopDirectoryOnly))
                {
                    var binLoader = new CircuitDiagram.IO.Descriptions.BinaryDescriptionReader();
                    binLoader.CertificateChain = certChain;

                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (binLoader.Read(fs))
                        {
                            var descriptions = binLoader.ComponentDescriptions;
                            ComponentDescriptionSource source = new ComponentDescriptionSource(file, new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(descriptions));
                            foreach (ComponentDescription description in descriptions)
                            {
                                description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                                description.Source = source;

                                // Check if duplicate GUID
                                if (!conflictingGuid && description.Metadata.GUID != Guid.Empty)
                                {
                                    foreach (ComponentDescription compareDescription in ComponentHelper.ComponentDescriptions)
                                        if (compareDescription.Metadata.GUID == description.Metadata.GUID)
                                            conflictingGuid = true;
                                }

                                ComponentHelper.AddDescription(description);
                                if (ComponentHelper.WireDescription == null && description.ComponentName.ToLowerInvariant() == "wire" && description.Metadata.GUID == new Guid("6353882b-5208-4f88-a83b-2271cc82b94f"))
                                    ComponentHelper.WireDescription = description;
                            }
                        }
                    }
                }
            }

            return conflictingGuid;
        }
    }
}
