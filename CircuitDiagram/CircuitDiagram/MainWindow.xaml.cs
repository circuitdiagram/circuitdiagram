// MainWindow.xaml.cs
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using CircuitDiagram.Components;
using CircuitDiagram.Render;
using Microsoft.Win32;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables
        string m_documentTitle;
        string m_docPath = "";
        UndoManager m_undoManager;
        Dictionary<Key, string> m_toolboxShortcuts = new Dictionary<Key, string>();

        UndoManager UndoManager { get { return m_undoManager; } }
        public System.Collections.ObjectModel.ObservableCollection<string> RecentFiles = new System.Collections.ObjectModel.ObservableCollection<string>();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize settings
            CircuitDiagram.Settings.Settings.Initialize(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\settings.xml");
            ApplySettings();

            circuitDisplay.Document = new CircuitDocument();
            Load();

            // load recent items
            LoadRecentFiles();
            mnuRecentItems.ItemsSource = RecentFiles;

            // Initialize undo manager
            m_undoManager = new UndoManager();
            circuitDisplay.UndoManager = m_undoManager;
            m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
            m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);

            this.Title = "Untitled - Circuit Diagram";
            m_documentTitle = "Untitled";

            this.Closed += new EventHandler(MainWindow_Closed);
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);

            ComponentHelper.ComponentUpdatedDelegate = new ComponentEditor.ComponentUpdatedDelegate(Editor_ComponentUpdated);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // check for updates
            if (!Settings.Settings.HasSetting("CheckForUpdatesOnStartup"))
                Settings.Settings.Write("CheckForUpdatesOnStartup", true);
            if (!Settings.Settings.ReadBool("CheckForUpdatesOnStartup"))
                return;
            if (DateTime.Now.Subtract(Settings.Settings.HasSetting("LastCheckForUpdates") ? (DateTime)Settings.Settings.Read("LastCheckForUpdates") : new DateTime(2000, 1, 1)).TotalDays < 1.0)
                return;
            System.Threading.Thread T = new System.Threading.Thread((O => CheckForUpdates(false)));
            T.Start();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Settings.Settings.Save();
        }

        /// <summary>
        /// Apply settings including toolbox and connection points.
        /// </summary>
        private void ApplySettings()
        {
            toolboxScroll.VerticalScrollBarVisibility = (CircuitDiagram.Settings.Settings.ReadBool("showToolboxScrollBar") ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden);
            circuitDisplay.ShowConnectionPoints = Settings.Settings.ReadBool("showConnectionPoints");
        }

        /// <summary>
        /// Open a document and add it to the recent files menu.
        /// </summary>
        /// <param name="path">The path of the document to open.</param>
        private void OpenDocument(string path)
        {
            if (System.IO.Path.GetExtension(path) == ".cddx" || System.IO.Path.GetExtension(path) == ".zip")
            {
                CircuitDocument document;
                CircuitDiagram.IO.DocumentLoadResult result = CircuitDiagram.IO.CDDXIO.Read(File.OpenRead(path), out document);
                if (result == IO.DocumentLoadResult.Success)
                    circuitDisplay.Document = document;
                else if (result == IO.DocumentLoadResult.FailNewerVersion)
                {
                    MessageBox.Show("The document was created in a newer version of Circuit Diagram and could not be loaded correctly.", "Unable to Load Document");
                }
                else if (result == IO.DocumentLoadResult.FailIncorrectFormat)
                {
                    MessageBox.Show("The document was not in the correct format.", "Unable to Load Document");
                }
                else
                {
                    // Unknown
                    MessageBox.Show("An unknown error occurred while loading the document.", "Unable to Load Document");
                }

                circuitDisplay.DrawConnections();

                m_docPath = path;
                m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(path);
                this.Title = m_documentTitle + " - Circuit Diagram";
                m_undoManager = new UndoManager();
                circuitDisplay.UndoManager = m_undoManager;
                m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
            }
            else
            {
                CircuitDiagram.IO.CircuitDocumentLoader loader = new IO.CircuitDocumentLoader();

                CircuitDiagram.IO.DocumentLoadResult result = loader.Load(new FileStream(path, FileMode.Open));

                if (result == IO.DocumentLoadResult.Success)
                {
                    circuitDisplay.Document = loader.Document;

                    m_docPath = path;
                    m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(path);
                    this.Title = m_documentTitle + " - Circuit Diagram";
                    m_undoManager = new UndoManager();
                    circuitDisplay.UndoManager = m_undoManager;
                    m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                    m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
                }
                else if (result == IO.DocumentLoadResult.FailNewerVersion)
                {
                    MessageBox.Show("The document was created in a newer version of Circuit Diagram and could not be loaded correctly.", "Unable to Load Document");
                }
                else if (result == IO.DocumentLoadResult.FailIncorrectFormat)
                {
                    MessageBox.Show("The document was not in the correct format.", "Unable to Load Document");
                }
                else
                {
                    // Unknown
                    MessageBox.Show("An unknown error occurred while loading the document.", "Unable to Load Document");
                }

                circuitDisplay.DrawConnections();
            }
        }

        /// <summary>
        /// Load component descriptions from disk and populate the toolbox.
        /// </summary>
        private void Load()
        {
            #region Load component descriptions
            List<string> componentLocations = new List<string>();

            string permanentComponentsDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ext";
            if (Directory.Exists(permanentComponentsDirectory))
                componentLocations.Add(permanentComponentsDirectory);
            string userComponentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\components";
            if (Directory.Exists(userComponentsDirectory))
                componentLocations.Add(userComponentsDirectory);

#if DEBUG
            string debugComponentsDirectory = Path.GetFullPath("../../Components");
            if (Directory.Exists(debugComponentsDirectory))
                componentLocations.Add(debugComponentsDirectory);
#endif

            CircuitDiagram.IO.XmlLoader xmlLoader = new CircuitDiagram.IO.XmlLoader();
            CircuitDiagram.IO.BinaryLoader binLoader = new CircuitDiagram.IO.BinaryLoader();

            foreach (string location in componentLocations)
            {
                foreach (string file in System.IO.Directory.GetFiles(location, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (xmlLoader.Load(fs))
                        {
                            ComponentDescription component = xmlLoader.GetDescriptions()[0];
                            component.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            component.Source = new ComponentDescriptionSource(file, new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(new ComponentDescription[] { component }));
                            ComponentHelper.AddDescription(component);
                            if (StandardComponents.Wire == null && component.ComponentName.ToLowerInvariant() == "wire" && component.Metadata.GUID == new Guid("6353882b-5208-4f88-a83b-2271cc82b94f"))
                                StandardComponents.Wire = component;
                        }
                    }
                }
            }
            Stream keyStream = System.Reflection.Assembly.GetAssembly(typeof(CircuitDocument)).GetManifestResourceStream("CircuitDiagram.key.txt");
            System.Security.Cryptography.RSACryptoServiceProvider tempRSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            byte[] data = new byte[keyStream.Length];
            keyStream.Read(data, 0, (int)keyStream.Length);
            string aaa = Encoding.UTF8.GetString(data);
            tempRSA.FromXmlString(aaa.Trim());
            foreach (string location in componentLocations)
            {
                foreach (string file in System.IO.Directory.GetFiles(location, "*.cdcom", SearchOption.TopDirectoryOnly))
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        binLoader.Load(fs, tempRSA.ExportParameters(false));
                        ComponentDescription[] descriptions = binLoader.GetDescriptions();
                        ComponentDescriptionSource source = new ComponentDescriptionSource(file, new System.Collections.ObjectModel.ReadOnlyCollection<ComponentDescription>(descriptions));
                        foreach (ComponentDescription description in descriptions)
                        {
                            description.Metadata.Location = ComponentDescriptionMetadata.LocationType.Installed;
                            description.Source = source;
                            ComponentHelper.AddDescription(description);
                            if (StandardComponents.Wire == null && description.ComponentName.ToLowerInvariant() == "wire" && description.Metadata.GUID == new Guid("6353882b-5208-4f88-a83b-2271cc82b94f"))
                                StandardComponents.Wire = description;
                        }
                    }
                }
            }
            #endregion

            LoadToolbox();
        }

        /// <summary>
        /// Populates the toolbox from the xml file.
        /// </summary>
        private void LoadToolbox()
        {
            object selectCategory = mainToolbox.Items[0];
            mainToolbox.Items.Clear();
            mainToolbox.Items.Add(selectCategory);

            m_toolboxShortcuts.Clear();

            try
            {
                XmlDocument toolboxSettings = new XmlDocument();
                string toolboxSettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml";
                toolboxSettings.Load(toolboxSettingsPath);

                XmlNodeList categoryNodes = toolboxSettings.SelectNodes("/display/category");
                foreach (XmlNode categoryNode in categoryNodes)
                {
                    var newCategory = new Toolbox.ToolboxCategory();
                    foreach (XmlNode node in categoryNode.ChildNodes)
                    {
                        if (node.Name == "component")
                        {
                            XmlElement element = node as XmlElement;

                            if (element.HasAttribute("guid") && element.HasAttribute("configuration"))
                            {
                                ComponentDescription description = ComponentHelper.FindDescription(new Guid(element.Attributes["guid"].InnerText));
                                if (description != null)
                                {
                                    ComponentConfiguration configuration = description.Metadata.Configurations.FirstOrDefault(configItem => configItem.Name == element.Attributes["configuration"].InnerText);
                                    if (configuration != null)
                                    {
                                        Toolbox.ToolboxItem newItem = new Toolbox.ToolboxItem();
                                        newItem.Tag = "@rid:" + description.RuntimeID + ", @config: " + configuration.Name;
                                        newItem.ToolTip = configuration.Name;
                                        TextBlock contentBlock = new TextBlock();
                                        contentBlock.Text = configuration.Name;
                                        contentBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                        contentBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                        newItem.Content = contentBlock;
                                        if (configuration.Icon != null)
                                        {
                                            Canvas contentCanvas = new Canvas();
                                            contentCanvas.Width = 45;
                                            contentCanvas.Height = 45;
                                            var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = configuration.Icon };
                                            newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                            newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                            contentCanvas.Children.Add(newImage);
                                            newItem.Content = contentCanvas;
                                        }
                                        else if (description.Metadata.Icon != null)
                                        {
                                            Canvas contentCanvas = new Canvas();
                                            contentCanvas.Width = 45;
                                            contentCanvas.Height = 45;
                                            var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = description.Metadata.Icon };
                                            newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                            newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                            contentCanvas.Children.Add(newImage);
                                            newItem.Content = contentCanvas;
                                        }
                                        newItem.Click += new RoutedEventHandler(toolboxButton_Click);
                                        newCategory.Items.Add(newItem);

                                        // Shortcut
                                        if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                                        {
                                            Key key = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);

                                            if (!m_toolboxShortcuts.ContainsKey(key))
                                                m_toolboxShortcuts.Add(key, "@rid:" + description.RuntimeID + ", @config: " + configuration.Name);
                                        }
                                    }
                                }
                            }
                            else if (element.HasAttribute("guid"))
                            {
                                ComponentDescription description = ComponentHelper.FindDescription(new Guid(element.Attributes["guid"].InnerText));
                                if (description != null)
                                {
                                    Toolbox.ToolboxItem newItem = new Toolbox.ToolboxItem();
                                    newItem.Tag = "@rid:" + description.RuntimeID;
                                    newItem.ToolTip = description.ComponentName;
                                    TextBlock contentBlock = new TextBlock();
                                    contentBlock.Text = description.ComponentName;
                                    contentBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                    contentBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                    newItem.Content = contentBlock;
                                    if (description.Metadata.Icon != null)
                                    {
                                        Canvas contentCanvas = new Canvas();
                                        contentCanvas.Width = 45;
                                        contentCanvas.Height = 45;
                                        var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = description.Metadata.Icon };
                                        newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                        newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                        contentCanvas.Children.Add(newImage);
                                        newItem.Content = contentCanvas;
                                    }
                                    newItem.Click += new RoutedEventHandler(toolboxButton_Click);
                                    newCategory.Items.Add(newItem);

                                    // Shortcut
                                    if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                                    {
                                        Key key = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);

                                        if (!m_toolboxShortcuts.ContainsKey(key))
                                            m_toolboxShortcuts.Add(key, "@rid:" + description.RuntimeID);
                                    }
                                }
                            }
                            else if (element.HasAttribute("type") && element.HasAttribute("configuration"))
                            {
                                ComponentDescription description = ComponentHelper.FindDescription(element.Attributes["type"].InnerText);
                                if (description != null)
                                {
                                    ComponentConfiguration configuration = description.Metadata.Configurations.FirstOrDefault(configItem => configItem.Name == element.Attributes["configuration"].InnerText);
                                    if (configuration != null)
                                    {
                                        Toolbox.ToolboxItem newItem = new Toolbox.ToolboxItem();
                                        newItem.Tag = "@rid:" + description.RuntimeID + ", @config: " + configuration.Name;
                                        newItem.ToolTip = configuration.Name;
                                        TextBlock contentBlock = new TextBlock();
                                        contentBlock.Text = configuration.Name;
                                        contentBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                        contentBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                        newItem.Content = contentBlock;
                                        if (configuration.Icon != null)
                                        {
                                            Canvas contentCanvas = new Canvas();
                                            contentCanvas.Width = 45;
                                            contentCanvas.Height = 45;
                                            var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = configuration.Icon };
                                            newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                            newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                            contentCanvas.Children.Add(newImage);
                                            newItem.Content = contentCanvas;
                                        }
                                        else if (description.Metadata.Icon != null)
                                        {
                                            Canvas contentCanvas = new Canvas();
                                            contentCanvas.Width = 45;
                                            contentCanvas.Height = 45;
                                            var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = description.Metadata.Icon };
                                            newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                            newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                            contentCanvas.Children.Add(newImage);
                                            newItem.Content = contentCanvas;
                                        }
                                        newItem.Click += new RoutedEventHandler(toolboxButton_Click);
                                        newCategory.Items.Add(newItem);

                                        // Shortcut
                                        if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                                        {
                                            Key key = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);

                                            if (!m_toolboxShortcuts.ContainsKey(key))
                                                m_toolboxShortcuts.Add(key, "@rid:" + description.RuntimeID + ", @config: " + configuration.Name);
                                        }
                                    }
                                }
                            }
                            else if (element.HasAttribute("type"))
                            {
                                ComponentDescription description = ComponentHelper.FindDescription(element.Attributes["type"].InnerText);
                                if (description != null)
                                {
                                    Toolbox.ToolboxItem newItem = new Toolbox.ToolboxItem();
                                    newItem.Tag = "@rid:" + description.RuntimeID;
                                    newItem.ToolTip = description.ComponentName;
                                    TextBlock contentBlock = new TextBlock();
                                    contentBlock.Text = description.ComponentName;
                                    contentBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                    contentBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                    newItem.Content = contentBlock;
                                    if (description.Metadata.Icon != null)
                                    {
                                        Canvas contentCanvas = new Canvas();
                                        contentCanvas.Width = 45;
                                        contentCanvas.Height = 45;
                                        var newImage = new Image() { Width = 45, Height = 45, Stretch = System.Windows.Media.Stretch.Uniform, VerticalAlignment = System.Windows.VerticalAlignment.Center, Source = description.Metadata.Icon };
                                        newImage.Effect = new System.Windows.Media.Effects.DropShadowEffect();
                                        newImage.SetValue(System.Windows.Media.RenderOptions.BitmapScalingModeProperty, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
                                        contentCanvas.Children.Add(newImage);
                                        newItem.Content = contentCanvas;
                                    }
                                    newItem.Click += new RoutedEventHandler(toolboxButton_Click);
                                    newCategory.Items.Add(newItem);

                                    // Shortcut
                                    if (element.HasAttribute("key") && KeyTextConverter.IsValidLetterKey(element.Attributes["key"].InnerText))
                                    {
                                        Key key = (Key)Enum.Parse(typeof(Key), element.Attributes["key"].InnerText);

                                        if (!m_toolboxShortcuts.ContainsKey(key))
                                            m_toolboxShortcuts.Add(key, "@rid:" + description.RuntimeID);
                                    }
                                }
                            }
                        }
                    }
                    if (newCategory.Items.Count > 0)
                        mainToolbox.Items.Add(newCategory);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The toolbox is corrupt. Please go to Tools->Toolbox to add items.", "Toolbox Corrupt", MessageBoxButton.OK, MessageBoxImage.Error);
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Circuit Diagram\\toolbox.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?><display></display>");
            }
        }

        void toolboxButton_Click(object sender, RoutedEventArgs e)
        {
            circuitDisplay.NewComponentData = (sender as Toolbox.ToolboxItem).Tag as string;
        }

        private void circuitDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gridEditor.Children.Clear();
            if (e.AddedItems.Count > 0)
                gridEditor.Children.Add((e.AddedItems[0] as Component).Editor);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized)
                return;
            circuitDisplay.NewComponentData = null;
        }

        CircuitDiagram.IO.CDDXSaveOptions m_defaultSaveOptions;
        CircuitDiagram.IO.CDDXSaveOptions m_lastSaveOptions;
        private void LoadDefaultCDDXSaveOptions()
        {
            string settingsData = Settings.Settings.Read("DefaultCDDXSaveSettings") as string;
            if (settingsData != null)
            {
                using (MemoryStream stream = new MemoryStream(System.Convert.FromBase64String(settingsData)))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    m_defaultSaveOptions = (IO.CDDXSaveOptions)binaryFormatter.Deserialize(stream);
                }
            }
            else
                m_defaultSaveOptions = new IO.CDDXSaveOptions();
        }

        void SetStatusText(string text)
        {
            lblStatus.Text = text;
        }

        private void CheckForUpdates(bool notifyIfNoUpdate)
        {
            // Check if using UI thread
            if (System.Threading.Thread.CurrentThread != this.Dispatcher.Thread)
            {
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { CheckForUpdates(notifyIfNoUpdate); }));
                return;
            }

            // Check for new version
            System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader("http://www.circuit-diagram.org/app/version.xml");
            System.Reflection.Assembly _assemblyInfo = System.Reflection.Assembly.GetExecutingAssembly();
            Version thisVersion = _assemblyInfo.GetName().Version;
            Version serverVersion = null;
            Version serverPreVersion = null;
            string downloadUrl = null;
            string elementName = null;
            try
            {
                reader.MoveToContent();
                if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "application")
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == System.Xml.XmlNodeType.Element)
                            elementName = reader.Name;
                        else if (reader.NodeType == System.Xml.XmlNodeType.Text)
                        {
                            if (elementName == "version" && reader.HasValue)
                            {
                                serverVersion = new Version(reader.Value);
                            }
                            else if (elementName == "preversion" && reader.HasValue)
                            {
                                serverPreVersion = new Version(reader.Value);
                            }
                            else if (elementName == "url" && reader.HasValue)
                            {
                                downloadUrl = reader.Value;
                            }
                        }
                    }
                }

#if PREVIEWVERSION
                if (serverVersion != null && (thisVersion.CompareTo(serverVersion) <= 0 || (thisVersion.Major <= serverVersion.Major && thisVersion.Minor <= serverVersion.Minor)) && downloadUrl != null)
                {
                    winNewVersion.Show(this, NewVersionWindowType.NewVersionAvailable, serverVersion.ToString(), downloadUrl);
                }
                else if (serverPreVersion != null && thisVersion.CompareTo(serverPreVersion) < 0 && downloadUrl != null)
                {
                    winNewVersion.Show(this, NewVersionWindowType.NewVersionAvailable, serverPreVersion.ToString() + " -pre", downloadUrl);
                }
                else if (thisVersion.CompareTo(serverVersion) >= 0)
                {
                    if (notifyIfNoUpdate)
                        winNewVersion.Show(this, NewVersionWindowType.NoNewVersionAvailable, null);
                }
                else
                {
                    if (notifyIfNoUpdate)
                        winNewVersion.Show(this, NewVersionWindowType.Error, null, "http://www.circuit-diagram.org/");
                }
#else
                if (serverVersion != null && thisVersion.CompareTo(serverVersion) < 0 && downloadUrl != null)
                {
                    winNewVersion.Show(this, NewVersionWindowType.NewVersionAvailable, serverVersion.ToString(), downloadUrl);
                }
                else if (thisVersion.CompareTo(serverVersion) >= 0)
                {
                    if (notifyIfNoUpdate)
                        winNewVersion.Show(this, NewVersionWindowType.NoNewVersionAvailable, null);
                }
                else
                {
                    if (notifyIfNoUpdate)
                        winNewVersion.Show(this, NewVersionWindowType.Error, null, "http://www.circuit-diagram.org/");
                }
#endif

                Settings.Settings.Write("LastCheckForUpdates", DateTime.Now);
                Settings.Settings.Save();
            }
            catch (Exception)
            {
                if (notifyIfNoUpdate)
                    winNewVersion.Show(this, NewVersionWindowType.Error, null, "http://www.circuit-diagram.org/");
            }
        }

        void Editor_ComponentUpdated(object sender, ComponentUpdatedEventArgs e)
        {
            UndoAction undoAction = new UndoAction(UndoCommand.ModifyComponents, "edit", new Component[] { e.Component });
            Dictionary<Component, string> previousData = new Dictionary<Component, string>(1);
            previousData.Add(e.Component, e.PreviousData);
            undoAction.AddData("before", previousData);
            Dictionary<Component, string> newData = new Dictionary<Component, string>(1);
            newData.Add(e.Component, e.Component.SerializeToString());
            undoAction.AddData("after", newData);
            UndoManager.AddAction(undoAction);

            // Update connections
            e.Component.ResetConnections();
            e.Component.ApplyConnections(circuitDisplay.Document);
            circuitDisplay.DrawConnections();
        }

        #region Menu Bar
        private void mnuFileExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Title = "Export";
            sfd.Filter = "PNG (*.png)|*.png|Scalable Vector Graphics (*.svg)|*.svg"; //"PNG (*.png)|*.png|Scalable Vector Graphics (*.svg)|*.svg|Enhanced Metafile (*.emf)|*.emf";
            sfd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (sfd.ShowDialog() == true)
            {
                string extension = System.IO.Path.GetExtension(sfd.FileName);
                if (extension == ".svg")
                {
                    SVGRenderer renderer = new SVGRenderer();
                    renderer.Begin();
                    circuitDisplay.Document.Render(renderer);
                    renderer.End();
                    System.IO.File.WriteAllBytes(sfd.FileName, renderer.SVGDocument.ToArray());
                }
                else if (extension == ".png")
                {
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)circuitDisplay.Document.Size.Width, (int)circuitDisplay.Document.Size.Height, 96d, 96d, PixelFormats.Default);
                    bmp.Render(circuitDisplay);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    System.IO.FileStream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create);
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }

        private void mnuFileComponents_Click(object sender, RoutedEventArgs e)
        {
            winComponents componentsWindow = new winComponents();
            componentsWindow.Components = ComponentHelper.ComponentDescriptions;
            componentsWindow.Owner = this;
            componentsWindow.ShowDialog();
        }

        private void mnuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "Circuit Diagram Document (*.cddx)|*.cddx|XML Files (*.xml)|*.xml";
            if (sfd.ShowDialog() == true)
            {
                string extension = System.IO.Path.GetExtension(sfd.FileName);
                if (extension == ".cddx")
                {
                    if (m_defaultSaveOptions == null)
                        LoadDefaultCDDXSaveOptions();

                    IO.CDDXSaveOptions saveOptions = m_lastSaveOptions;

                    bool doSave = true;
                    bool alwaysUseSettings = Settings.Settings.ReadBool("AlwaysUseCDDXSaveSettings");
                    if (!alwaysUseSettings)
                    {
                        winCDDXSave saveOptionsDialog = new winCDDXSave();
                        saveOptionsDialog.Owner = this;
                        saveOptionsDialog.SaveOptions = m_defaultSaveOptions;

                        List<ComponentDescription> usedDescriptions = new List<ComponentDescription>();
                        foreach (Component component in circuitDisplay.Document.Components)
                            if (!usedDescriptions.Contains(component.Description))
                                usedDescriptions.Add(component.Description);
                        saveOptionsDialog.AvailableComponents = usedDescriptions;

                        if (saveOptionsDialog.ShowDialog() == true)
                        {
                            doSave = true;
                            saveOptions = saveOptionsDialog.SaveOptions;
                            if (saveOptionsDialog.AlwaysUseSettings)
                            {
                                m_defaultSaveOptions = saveOptionsDialog.SaveOptions;
                                Settings.Settings.Write("AlwaysUseCDDXSaveSettings", true);

                                using (MemoryStream stream = new MemoryStream())
                                {
                                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                                    binaryFormatter.Serialize(stream, m_defaultSaveOptions);

                                    string encodedData = System.Convert.ToBase64String(stream.ToArray());
                                    Settings.Settings.Write("DefaultCDDXSaveSettings", encodedData);
                                }
                            }
                        }
                        else
                            doSave = false;
                    }

                    if (doSave)
                    {
                        m_lastSaveOptions = saveOptions;
                        CircuitDiagram.IO.CDDXIO.Write(new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), circuitDisplay.Document, saveOptions);
                        m_docPath = sfd.FileName;
                        m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                        this.Title = m_documentTitle + " - Circuit Diagram";
                        m_undoManager.SetSaveIndex();
                    }
                }
                else if (extension == ".xml")
                {
                    circuitDisplay.Document.Save(new FileStream(sfd.FileName, FileMode.Create));
                    m_docPath = sfd.FileName;
                    m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                    this.Title = m_documentTitle + " - Circuit Diagram";
                    m_undoManager.SetSaveIndex();
                }
            }
        }

        private void mnuToolsToolbox_Click(object sender, RoutedEventArgs e)
        {
            winToolbox toolboxWindow = new winToolbox();
            toolboxWindow.Owner = this;
            if (toolboxWindow.ShowDialog() == true)
            {
                LoadToolbox();
            }
        }

        private void mnuRecentItem_Click(object sender, RoutedEventArgs e)
        {
            // Load the clicked item
            if ((e.OriginalSource as MenuItem).Header is string && System.IO.File.Exists((e.OriginalSource as MenuItem).Header as string))
                OpenDocument((e.OriginalSource as MenuItem).Header as string);
        }

        private void mnuFileOptions_Click(object sender, RoutedEventArgs e)
        {
            winOptions options = new winOptions();
            options.Owner = this;
            if (options.ShowDialog() == true)
            {
                CircuitDiagram.Settings.Settings.Save();

                ApplySettings();

                circuitDisplay.DrawConnections();
            }
        }

        private void mnuFileDocument_Click(object sender, RoutedEventArgs e)
        {
            winDocument documentInfoWindow = new winDocument();
            documentInfoWindow.Owner = this;
            documentInfoWindow.ShowDialog();
        }

        private void mnuHelpDocumentation_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.circuit-diagram.org/help");
        }

        private void mnuCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates(true);
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            winAbout aboutWindow = new winAbout();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region RecentFiles
        private void AddRecentFile(string path)
        {
            if (RecentFiles.Count == 1 && RecentFiles[0] == "(empty)")
                RecentFiles.Clear();
            if (RecentFiles.Contains(path))
                RecentFiles.Remove(path);
            RecentFiles.Insert(0, path);
            if (RecentFiles.Count > 10)
                RecentFiles.RemoveAt(10);
        }

        private void LoadRecentFiles()
        {
            string[] files = CircuitDiagram.Settings.Settings.Read("recentfiles") as string[];
            if (files == null)
            {
                RecentFiles.Add("(empty)");
                return;
            }
            foreach (string file in files)
                RecentFiles.Add(file);
        }

        private void SaveRecentFiles()
        {
            if (RecentFiles.Count == 1 && RecentFiles[0] == "(empty)")
                return;
            CircuitDiagram.Settings.Settings.Write("recentfiles", RecentFiles.ToArray());
            CircuitDiagram.Settings.Settings.Save();
        }
        #endregion

        #region Commands
        private void CommandNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            winNewDocument newDocumentWindow = new winNewDocument();
            newDocumentWindow.Owner = this;
            if (newDocumentWindow.ShowDialog() == true)
            {
                CircuitDocument newDocument = new CircuitDocument();
                newDocument.Size = new Size(newDocumentWindow.DocumentWidth, newDocumentWindow.DocumentHeight);
                circuitDisplay.Document = newDocument;
                circuitDisplay.DrawConnections();
                this.Title = "Untitled - Circuit Diagram";
                m_documentTitle = "Untitled";
                m_undoManager = new UndoManager();
                circuitDisplay.UndoManager = m_undoManager;
                m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
            }
        }

        private void CommandOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open";
            ofd.Filter = "Supported Formats (*.cddx;*.xml)|*.cddx;*.xml|Circuit Diagram Document (*.cddx)|*.cddx|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (ofd.ShowDialog() == true)
            {
                OpenDocument(ofd.FileName);
            }
        }

        private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_docPath != "")
            {
                if (System.IO.Path.GetExtension(m_docPath) == ".cddx")
                {
                    // Save in CDDX format
                    CircuitDiagram.IO.CDDXIO.Write(new FileStream(m_docPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), circuitDisplay.Document, m_lastSaveOptions);
                }
                else
                {
                    // Save in XML format
                    circuitDisplay.Document.Save(new FileStream(m_docPath, FileMode.Create));
                }
                this.Title = System.IO.Path.GetFileNameWithoutExtension(m_docPath) + " - Circuit Diagram";
                UndoManager.SetSaveIndex();
            }
            else
                mnuFileSaveAs_Click(sender, e);
        }

        private void CommandUndo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoManager.CanStepBackwards();
        }

        private void CommandUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UndoManager.StepBackwards();
        }

        private void CommandRedo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoManager.CanStepForwards();
        }

        private void CommandRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UndoManager.StepForwards();
        }

        private void DeleteComponentCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            circuitDisplay.DeleteComponentCommand_CanExecute(sender, e);
        }

        private void DeleteComponentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            circuitDisplay.DeleteComponentCommand(sender, e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat && e.IsDown)
            {
                // Check component shortcuts
                if (e.Key == Key.V) // Move/select
                {
                    circuitDisplay.NewComponentData = null;

                    SetStatusText("Select tool.");

                    e.Handled = true;
                }
                else if (e.Key == Key.W) // Wire
                {
                    circuitDisplay.NewComponentData = "@rid: " + StandardComponents.Wire.RuntimeID;

                    SetStatusText("Placing wire.");

                    e.Handled = true;
                }
                else
                {
                    // Check custom toolbox entries
                    if (m_toolboxShortcuts.ContainsKey(e.Key))
                    {
                        circuitDisplay.NewComponentData = m_toolboxShortcuts[e.Key];
                        
                        string rid = circuitDisplay.NewComponentData.Substring(circuitDisplay.NewComponentData.IndexOf("@rid:") + 5);
                        string configuration = null;
                        if (rid.Contains(" "))
                        {
                            configuration = rid.Substring(rid.IndexOf("@config:") + 8);
                            rid = rid.Substring(0, rid.IndexOf(" "));
                        }

                        ComponentDescription description = ComponentHelper.FindDescriptionByRuntimeID(int.Parse(rid));

                        // TODO: add configuration to status text

                        if (description != null)
                            SetStatusText(String.Format("Placing component: {0}", description.ComponentName));

                        e.Handled = true;
                    }
                    else
                    {
                        SetStatusText(String.Format("Unknown shortcut: {0}.", e.Key));
                    }
                }
            }
        }
        #endregion

        #region Undo Manager
        void m_undoManager_ActionOccurred(object sender, EventArgs e)
        {
            if (UndoManager.IsSavedState())
            {
                this.Title = m_documentTitle + " - Circuit Diagram";
            }
            else
            {
                this.Title = m_documentTitle + "* - Circuit Diagram";
            }
        }

        void UndoActionProcessor(object sender, UndoActionEventArgs e)
        {
            if (e.Event == UndoActionEvent.Remove)
            {
                switch (e.Action.Command)
                {
                    case UndoCommand.ModifyComponents:
                        {
                            Component[] components = (e.Action.GetDefaultData() as Component[]);
                            Dictionary<Component, string> beforeData = e.Action.GetData<Dictionary<Component, string>>("before");
                            foreach (Component component in components)
                            {
                                component.Deserialize(ComponentDataString.ConvertToDictionary(beforeData[component]));
                                component.UpdateVisual();
                                component.ResetConnections();
                                component.ApplyConnections(circuitDisplay.Document);
                            }
                        }
                        break;
                    case UndoCommand.DeleteComponents:
                        {
                            Component[] components = (e.Action.GetDefaultData() as Component[]);
                            foreach (Component component in components)
                            {
                                circuitDisplay.Document.Elements.Add(component);
                                component.ResetConnections();
                                component.ApplyConnections(circuitDisplay.Document);
                            }
                        }
                        break;
                    case UndoCommand.AddComponent:
                        {
                            Component component = (e.Action.GetDefaultData() as Component);
                            component.DisconnectConnections();
                            circuitDisplay.Document.Elements.Remove(component);
                        }
                        break;
                }
            }
            else
            {
                switch (e.Action.Command)
                {
                    case UndoCommand.ModifyComponents:
                        {
                            Component[] components = (e.Action.GetDefaultData() as Component[]);
                            Dictionary<Component, string> beforeData = e.Action.GetData<Dictionary<Component, string>>("after");
                            foreach (Component component in components)
                            {
                                component.Deserialize(ComponentDataString.ConvertToDictionary(beforeData[component]));
                                component.UpdateVisual();
                                component.ResetConnections();
                                component.ApplyConnections(circuitDisplay.Document);
                            }
                        }
                        break;
                    case UndoCommand.DeleteComponents:
                        {
                            Component[] components = (e.Action.GetDefaultData() as Component[]);
                            foreach (Component component in components)
                            {
                                component.DisconnectConnections();
                                circuitDisplay.Document.Elements.Remove(component);
                            }
                        }
                        break;
                    case UndoCommand.AddComponent:
                        {
                            Component component = (e.Action.GetDefaultData() as Component);
                            circuitDisplay.Document.Elements.Add(component);
                            component.ResetConnections();
                            component.ApplyConnections(circuitDisplay.Document);
                        }
                        break;
                }
            }

            circuitDisplay.DrawConnections();
        }
        #endregion
    }
}