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
using CircuitDiagram.DPIWindow;
using CircuitDiagram.IO;
using CircuitDiagram.Render;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;
using TaskDialogInterop;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroDPIWindow
    {
        #region Variables
        string m_documentTitle;
        string m_docPath = "";
        IO.CDDX.CDDXSaveOptions m_lastSaveOptions;
        DispatcherTimer m_statusTimer;
        UndoManager m_undoManager;
        Dictionary<Key, IdentifierWithShortcut> m_toolboxShortcuts = new Dictionary<Key, IdentifierWithShortcut>();
        UndoManager UndoManager { get { return m_undoManager; } }
        public System.Collections.ObjectModel.ObservableCollection<string> RecentFiles = new System.Collections.ObjectModel.ObservableCollection<string>();
        List<ImplementationConversionCollection> m_componentRepresentations = new List<ImplementationConversionCollection>();
        string m_docToLoad = null;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            m_statusTimer = new DispatcherTimer(new TimeSpan(0, 0, 5), DispatcherPriority.Normal, new EventHandler((sender, e) =>
                {
                    m_statusTimer.Stop();
                    lblStatus.Text = "Ready";
                }), lblStatus.Dispatcher);
            m_statusTimer.IsEnabled = false;

            // Initialize cdlibrary
            ConfigureCdLibrary();

            // Initialize settings
            ApplySettings();

            ComponentHelper.ComponentUpdatedDelegate = new ComponentUpdatedDelegate(Editor_ComponentUpdated);
            ComponentHelper.CreateEditor = new CreateComponentEditorDelegate(ComponentEditorHelper.CreateEditor);

            circuitDisplay.Document = new CircuitDocument();
            InitializeMetadata(circuitDisplay.Document);

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
            this.ContentRendered += new EventHandler(MainWindow_ContentRendered);

            // check if should open file
            if (App.AppArgs.Length > 0)
            {
                if (System.IO.File.Exists(App.AppArgs[0]))
                    m_docToLoad = App.AppArgs[0];
            }

            DPIChanged += MainWindow_DPIChanged;
        }

        void MainWindow_DPIChanged(object sender, EventArgs e)
        {
            // Set the new DPI for the MultiResolutionImageToImageSourceConverter
            var multiResImgConverter = this.Resources["MultiResolutionImageToImageSourceConverter"] as MultiResolutionImageToImageSourceConverter;
            multiResImgConverter.DPI = this.CurrentDPI;

            // Load new toolbox icons
            LoadToolbox();
        }

        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (m_docToLoad != null)
            {
                OpenDocument(m_docToLoad);
                m_docToLoad = null;
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var multiResImgConverter = this.Resources["MultiResolutionImageToImageSourceConverter"] as MultiResolutionImageToImageSourceConverter;
            try
            {
                multiResImgConverter.DPI = this.CurrentDPI;
            }
            catch
            {
                multiResImgConverter.DPI = 96d;
            }

            LoadToolbox();

            // Check for updates
            if (!SettingsManager.Settings.HasSetting("CheckForUpdatesOnStartup"))
                SettingsManager.Settings.Write("CheckForUpdatesOnStartup", true);

            var lastCheckForUpdates = SettingsManager.Settings.HasSetting("LastCheckForUpdates") ?
                (DateTime)SettingsManager.Settings.Read("LastCheckForUpdates") : new DateTime(2000, 1, 1);
            var timeSinceLastUpdateCheck = DateTime.Now.Subtract(lastCheckForUpdates);

            if (SettingsManager.Settings.ReadBool("CheckForUpdatesOnStartup") &&
                timeSinceLastUpdateCheck.TotalDays >= 1.0)
            {
                // Checking for updates is enabled, and last check was at least one day ago
                CheckForUpdates(false);
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            SettingsManager.Settings.Save();
        }

        /// <summary>
        /// Apply settings including toolbox and connection points.
        /// </summary>
        private void ApplySettings()
        {
            toolboxScroll.VerticalScrollBarVisibility = (CircuitDiagram.SettingsManager.Settings.ReadBool("showToolboxScrollBar") ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden);
            circuitDisplay.ShowConnectionPoints = SettingsManager.Settings.ReadBool("showConnectionPoints");
            circuitDisplay.ShowGrid = SettingsManager.Settings.ReadBool("showEditorGrid");
            if (SettingsManager.Settings.HasSetting("EmbedComponents"))
                ComponentHelper.EmbedOptions = (ComponentEmbedOptions)SettingsManager.Settings.Read("EmbedComponents");

            // Load user name
            if (!SettingsManager.Settings.HasSetting("ComputerUserName"))
                SettingsManager.Settings.Write("ComputerUserName", System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName);
        }

        /// <summary>
        /// Configures cdlibrary.dll, setting application constants.
        /// </summary>
        private void ConfigureCdLibrary()
        {
            // Set application version
            CircuitDiagram.IO.ApplicationInfo.FullName = "Circuit Diagram " + UpdateManager.AppDisplayVersion;
            CircuitDiagram.IO.ApplicationInfo.Name = "Circuit Diagram";
            CircuitDiagram.IO.ApplicationInfo.Version = UpdateManager.BuildVersion.ToString();
        }

        /// <summary>
        /// Load component descriptions from disk and populate the toolbox.
        /// </summary>
        private void Load()
        {
            // Load component descriptions
            try
            {
                ComponentsManager.LoadComponents();
            }
            catch (ComponentDescriptionLoadException ex)
            {
                SetStatusText(ex.Message);
            }

            // Load implementation conversions
            m_componentRepresentations = ImplementationConversionManager.LoadImplementationConversions();

            // Load plugins
            PluginManager.Initialize();
            try
            {
                LoadImportMenu();
            }
            catch (Exception)
            {
                SetStatusText("An error occurred loading plugins.");
            }
        }

        private void LoadImportMenu()
        {
            mnuFileImport.Items.Clear();
            if (PluginManager.EnabledImportReaders.Count() > 0)
            {
                mnuFileImport.Visibility = System.Windows.Visibility.Visible;

                foreach (var item in PluginManager.EnabledImportReaders)
                {
                    MenuItem importMenuItem = new MenuItem();
                    importMenuItem.Header = item.PluginPartName;
                    importMenuItem.Tag = item;
                    importMenuItem.Click += new RoutedEventHandler(importMenuItem_Click);
                    mnuFileImport.Items.Add(importMenuItem);
                }
            }
            else
                mnuFileImport.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Populates the toolbox from the xml file.
        /// </summary>
        private void LoadToolbox()
        {
            try
            {
                var toolboxData = ToolboxManager.LoadToolbox();

                // Add shortcuts
                m_toolboxShortcuts.Clear();
                foreach(var cat in toolboxData)
                {
                    foreach(IdentifierWithShortcut component in cat)
                    {
                        if (component.ShortcutKey != Key.None)
                            m_toolboxShortcuts.Add(component.ShortcutKey, component);
                    }
                }

                // Add search tool
                var searchIcon = new MultiResolutionImage();
                searchIcon.LoadedIcons.Add(new BitmapImage(new Uri("pack://application:,,,/Images/Search32.png")));
                searchIcon.LoadedIcons.Add(new BitmapImage(new Uri("pack://application:,,,/Images/Search64.png")));
                var searchItem = new CustomToolboxItem()
                {
                    DisplayName = "Search",
                    Icon = searchIcon
                };
                var searchCategory = new List<IToolboxItem>();
                searchCategory.Add(searchItem);
                toolboxData.Insert(0, searchCategory);

                // Add "select" tool
                var selectIcon = new MultiResolutionImage();
                selectIcon.LoadedIcons.Add(new BitmapImage(new Uri("pack://application:,,,/Images/Select32.png")));
                selectIcon.LoadedIcons.Add(new BitmapImage(new Uri("pack://application:,,,/Images/Select64.png")));
                var selectItem = new CustomToolboxItem()
                {
                    DisplayName = "Select",
                    Icon = selectIcon
                };
                var selectCategory = new List<IToolboxItem>();
                selectCategory.Add(selectItem);
                toolboxData.Insert(0, selectCategory);

                mainToolbox.ItemsSource = toolboxData;

                // Set "select" as the current tool in the toolbox
                ToolboxSelectSelectTool();
            }
            catch
            {
                TaskDialogOptions tdOptions = new TaskDialogOptions();
                tdOptions.Title = "Circuit Diagram";
                tdOptions.MainInstruction = "The toolbox is corrupt.";
                tdOptions.Content = "New items can be added to the toolbox under Tools->Toolbox."; // Toolbox missing
                tdOptions.CommonButtons = TaskDialogCommonButtons.Close;
                tdOptions.Owner = this;
                this.Dispatcher.BeginInvoke(new Action(() => TaskDialog.Show(tdOptions)));

                // Create new toolbox file
                ToolboxManager.OverwriteToolbox();
            }
        }

        private void ToolboxSelectSelectTool()
        {
            mainToolbox.SetSelectedCategoryItem((mainToolbox.Items[0] as IEnumerable<IToolboxItem>).First());
            circuitDisplay.NewComponentData = null;
        }

        private void ToolboxSelectSearchTool()
        {
            mainToolbox.SetSelectedCategoryItem((mainToolbox.Items[1] as IEnumerable<IToolboxItem>).First());
        }

        private void ToolboxSelectWire()
        {
            foreach(List<IToolboxItem> item in mainToolbox.Items)
            {
                var component = item.First() as IdentifierWithShortcut;
                if (component != null && component.Identifier.Description == ComponentHelper.WireDescription)
                {
                    mainToolbox.SetSelectedCategoryItem(component);
                    break;
                }
            }
            circuitDisplay.NewComponentData = String.Format("@rid:{0}", ComponentHelper.WireDescription.RuntimeID);
        }

        private void mainToolbox_SelectionChanged(object sender, EventArgs e)
        {
            var selectedItem = mainToolbox.SelectedCategoryItem as IdentifierWithShortcut;

            if (selectedItem == null)
            {
                var cat = mainToolbox.SelectedCategoryItem as CustomToolboxItem;
                if (cat.DisplayName == "Select")
                {
                    // "Select" tool
                    circuitDisplay.NewComponentData = null;
                    return;
                }
                else if (cat.DisplayName == "Search")
                {
                    // Open quick command
                    quickCommand.IsOpen = true;
                    return;
                }
            }

            circuitDisplay.NewComponentData = selectedItem.ToString();
        }

        void toolboxButton_Click(object sender, RoutedEventArgs e)
        {
            circuitDisplay.NewComponentData = (sender as Toolbox.ToolboxItem).Tag as string;
        }

        private void circuitDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gridEditor.Content = null;
            if (e.AddedItems.Count > 0 && (e.AddedItems[0] as Component).Editor != null)
                gridEditor.Content = (e.AddedItems[0] as Component).Editor as ComponentEditor;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized)
                return;
            circuitDisplay.NewComponentData = null;
        }

        /// <summary>
        /// Set the status bar text.
        /// </summary>
        /// <param name="text">The message.</param>
        void SetStatusText(string text)
        {
            lblStatus.Text = text;
            m_statusTimer.Start();
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        /// <param name="notifyIfNoUpdate">Show a dialog even if no updates are available.</param>
        private void CheckForUpdates(bool notifyIfNoUpdate)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var newVersion = UpdateManager.CheckForUpdates();

                    if (newVersion != null)
                    {
                        // New version available
                        this.Dispatcher.Invoke(new Action(() =>
                            winNewVersion.Show(this, NewVersionWindowType.NewVersionAvailable, newVersion.Version, newVersion.DownloadUrl)));
                    }
                    else if (notifyIfNoUpdate)
                    {
                        // Notify that no new versions are available
                        this.Dispatcher.Invoke(new Action(() => winNewVersion.Show(this, NewVersionWindowType.NoNewVersionAvailable, null, null)));
                    }
                }
                catch
                {
                    if (notifyIfNoUpdate)
                        this.Dispatcher.Invoke(new Action(() => winNewVersion.Show(this, NewVersionWindowType.Error, null, "http://www.circuit-diagram.org/")));
                }

                SettingsManager.Settings.Write("LastCheckForUpdates", DateTime.Now);
                SettingsManager.Settings.Save();
            });
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

        private void lblSliderZoom_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sliderZoom.Value = 50;
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (circuitDisplay != null)
                circuitDisplay.LayoutTransform = new ScaleTransform(e.NewValue / 50, e.NewValue / 50);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UndoManager == null)
                return;

            if (!UndoManager.IsSavedState())
            {
                TaskDialogOptions tdOptions = new TaskDialogOptions();
                tdOptions.Title = "Circuit Diagram";
                tdOptions.MainInstruction = "Do you want to save changes to " + m_documentTitle + "?";
                tdOptions.CustomButtons = new string[] { "&Save", "Do&n't Save", "Cancel" };
                tdOptions.Owner = this;
                TaskDialogResult result = TaskDialog.Show(tdOptions);

                bool saved = false;
                if (result.CustomButtonResult == 0)
                    SaveDocument(out saved);

                if (result.CustomButtonResult == 2 || result.Result == TaskDialogSimpleResult.Cancel || (result.CustomButtonResult == 0 && !saved))
                    e.Cancel = true;
            }

            SaveRecentFiles();
        }

        private void circuitDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom with the mouse wheel
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                sliderZoom.Value += (e.Delta / 60);
                e.Handled = true;
            }
        }

        #region IO
        /// <summary>
        /// Open a document and add it to the recent files menu.
        /// </summary>
        /// <param name="path">The path of the document to open.</param>
        private void OpenDocument(string path)
        {
            if (System.IO.Path.GetExtension(path).ToLowerInvariant() == ".cddx" || System.IO.Path.GetExtension(path).ToLowerInvariant() == ".zip")
            {
                using (IO.CDDX.CDDXReader reader = new IO.CDDX.CDDXReader())
                {
                    bool succeeded = reader.Load(File.OpenRead(path));

                    List<IOComponentType> unavailableComponents = null;
                    CircuitDocument loadedDocument = null;
                    if (succeeded)
                    {
                        loadedDocument = reader.Document.ToCircuitDocument(reader, out unavailableComponents);
                        loadedDocument.Metadata.Format = reader.LoadResult.Format; // Set format
                    }
                    if (unavailableComponents == null)
                        unavailableComponents = new List<IOComponentType>();

                    // Show load result dialog
                    if (reader.LoadResult.Type != DocumentLoadResultType.Success || reader.LoadResult.Errors.Count > 0 || unavailableComponents.Count > 0)
                    {
                        winDocumentLoadResult loadResultWindow = new winDocumentLoadResult();
                        loadResultWindow.Owner = this;
                        loadResultWindow.SetMessage(reader.LoadResult.Type);
                        loadResultWindow.SetErrors(reader.LoadResult.Errors);
                        loadResultWindow.SetUnavailableComponents(unavailableComponents);
                        loadResultWindow.ShowDialog();
                    }

                    if (succeeded)
                    {
                        circuitDisplay.Document = loadedDocument;
                        circuitDisplay.DrawConnections();
                        m_docPath = path;
                        m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(path);
                        this.Title = m_documentTitle + " - Circuit Diagram";
                        m_undoManager = new UndoManager();
                        circuitDisplay.UndoManager = m_undoManager;
                        m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                        m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
                        AddRecentFile(path);
                    }
                }
            }
            else
            {
                using (IO.Xml.XmlReader reader = new IO.Xml.XmlReader())
                {
                    bool succeeded = reader.Load(File.OpenRead(path));

                    List<IOComponentType> unavailableComponents = null;
                    CircuitDocument loadedDocument = null;
                    if (succeeded)
                    {
                        loadedDocument = reader.Document.ToCircuitDocument(reader, out unavailableComponents);
                        loadedDocument.Metadata.Format = reader.LoadResult.Format; // Set format
                    }
                    if (unavailableComponents == null)
                        unavailableComponents = new List<IOComponentType>();

                    // Show load result dialog
                    if (reader.LoadResult.Type != DocumentLoadResultType.Success || reader.LoadResult.Errors.Count > 0 || unavailableComponents.Count > 0)
                    {
                        winDocumentLoadResult loadResultWindow = new winDocumentLoadResult();
                        loadResultWindow.Owner = this;
                        loadResultWindow.SetMessage(reader.LoadResult.Type);
                        loadResultWindow.SetErrors(reader.LoadResult.Errors);
                        loadResultWindow.SetUnavailableComponents(unavailableComponents);
                        loadResultWindow.ShowDialog();
                    }

                    if (succeeded)
                    {
                        circuitDisplay.Document = loadedDocument;
                        circuitDisplay.DrawConnections();
                        m_docPath = path;
                        m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(path);
                        this.Title = m_documentTitle + " - Circuit Diagram";
                        m_undoManager = new UndoManager();
                        circuitDisplay.UndoManager = m_undoManager;
                        m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                        m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
                        AddRecentFile(path);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to save the document, prompting for a file name if it has not previously been saved.
        /// </summary>
        /// <param name="saved">Whether the document was saved.</param>
        private void SaveDocument(out bool saved)
        {
            if (!String.IsNullOrEmpty(m_docPath))
            {
                IDictionary<IOComponentType, EmbedComponentData> embedComponents;
                IODocument ioDocument = circuitDisplay.Document.ToIODocument(out embedComponents);

                string extension = Path.GetExtension(m_docPath);
                if (extension == ".cddx")
                {
                    // Load default save options if no previous options (e.g. if file opened)
                    SettingsManager.SettingsSerializer serializer = new SettingsManager.SettingsSerializer();
                    serializer.Category = "CDDX";
                    m_lastSaveOptions = new IO.CDDX.CDDXSaveOptions();
                    m_lastSaveOptions.Deserialize(serializer);

                    IO.CDDX.CDDXWriter writer = new IO.CDDX.CDDXWriter();
                    writer.Document = ioDocument;
                    writer.Options = m_lastSaveOptions;
                    writer.EmbedComponents = embedComponents;

                    writer.Begin();
                    if (writer.RenderContext != null)
                    {
                        writer.RenderContext.Begin();
                        circuitDisplay.Document.Render(writer.RenderContext);
                        writer.RenderContext.End();
                    }

                    using (FileStream fs = new FileStream(m_docPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        writer.Write(fs);
                    }

                    writer.End();
                }
                else
                {
                    IO.Xml.XmlWriter writer = new IO.Xml.XmlWriter();
                    writer.Document = ioDocument;

                    writer.Begin();
                    using (FileStream fs = new FileStream(m_docPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        writer.Write(fs);
                    }
                    writer.End();
                }
                this.Title = System.IO.Path.GetFileNameWithoutExtension(m_docPath) + " - Circuit Diagram";
                UndoManager.SetSaveIndex();

                saved = true;
            }
            else
                SaveDocumentAs(out saved);
        }

        /// <summary>
        /// Shows the save file dialog and then saves the document.
        /// </summary>
        /// <param name="saved">Whether the document was saved.</param>
        private void SaveDocumentAs(out bool saved)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "Circuit Diagram Document (*.cddx)|*.cddx|XML Files (*.xml)|*.xml";
            if (sfd.ShowDialog() == true)
            {
                string extension = Path.GetExtension(sfd.FileName);

                IDictionary<IOComponentType, EmbedComponentData> embedComponents;
                IODocument ioDocument = circuitDisplay.Document.ToIODocument(out embedComponents);

                if (extension == ".cddx")
                {
                    bool doSave = false;

                    // Load default save options
                    SettingsManager.SettingsSerializer serializer = new SettingsManager.SettingsSerializer();
                    serializer.Category = "CDDX";
                    CircuitDiagram.IO.CDDX.CDDXSaveOptions saveOptions = new CircuitDiagram.IO.CDDX.CDDXSaveOptions();
                    saveOptions.Deserialize(serializer);

                    if (!SettingsManager.Settings.ReadBool("CDDX.AlwaysUseSettings"))
                    {
                        winSaveOptions saveOptionsWindow = new winSaveOptions(new CDDXSaveOptionsEditor(), saveOptions);
                        saveOptionsWindow.Owner = this;
                        if (saveOptionsWindow.ShowDialog() == true)
                        {
                            saveOptions = saveOptionsWindow.Options as CircuitDiagram.IO.CDDX.CDDXSaveOptions;

                            if (saveOptionsWindow.AlwaysUseSettings)
                            {
                                saveOptions.Serialize(serializer);
                                SettingsManager.Settings.Write("CDDX.AlwaysUseSettings", true);
                            }

                            doSave = true;
                        }
                    }
                    else
                        doSave = true;

                    if (doSave)
                    {
                        IO.CDDX.CDDXWriter writer = new IO.CDDX.CDDXWriter();
                        writer.Document = ioDocument;
                        writer.Options = saveOptions;
                        writer.EmbedComponents = embedComponents;

                        writer.Begin();
                        if (writer.RenderContext != null)
                        {
                            writer.RenderContext.Begin();
                            circuitDisplay.Document.Render(writer.RenderContext);
                            writer.RenderContext.End();
                        }

                        using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            writer.Write(fs);
                        }

                        writer.End();

                        m_docPath = sfd.FileName;
                        m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                        this.Title = m_documentTitle + " - Circuit Diagram";
                        m_undoManager.SetSaveIndex();
                        AddRecentFile(m_docPath);
                        m_lastSaveOptions = saveOptions;

                        saved = true;
                    }
                    else
                        saved = false;
                }
                else
                {
                    IO.Xml.XmlWriter writer = new IO.Xml.XmlWriter();
                    writer.Document = ioDocument;

                    writer.Begin();
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        writer.Write(fs);
                    }
                    writer.End();

                    m_docPath = sfd.FileName;
                    m_documentTitle = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                    this.Title = m_documentTitle + " - Circuit Diagram";
                    m_undoManager.SetSaveIndex();
                    AddRecentFile(m_docPath);

                    saved = true;
                }
            }
            else
                saved = false;
        }
        #endregion

        #region Menu Bar
        void importMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import";
            IDocumentReader reader = (sender as MenuItem).Tag as IDocumentReader;
            ofd.Filter = String.Format("{0} (*{1})|*{1}", reader.FileTypeName, reader.FileTypeExtension);
            if (ofd.ShowDialog() == true)
            {
                bool succeeded = reader.Load(File.OpenRead(ofd.FileName));

                List<IOComponentType> unavailableComponents = null;
                CircuitDocument loadedDocument = null;
                if (succeeded)
                {
                    loadedDocument = reader.Document.ToCircuitDocument(reader, out unavailableComponents);
                    loadedDocument.Metadata.Format = reader.LoadResult.Format; // Set format
                }
                if (unavailableComponents == null)
                    unavailableComponents = new List<IOComponentType>();

                // Show load result dialog
                if (reader.LoadResult.Type != DocumentLoadResultType.Success || reader.LoadResult.Errors.Count > 0 || unavailableComponents.Count > 0)
                {
                    winDocumentLoadResult loadResultWindow = new winDocumentLoadResult();
                    loadResultWindow.Owner = this;
                    loadResultWindow.SetMessage(reader.LoadResult.Type);
                    loadResultWindow.SetErrors(reader.LoadResult.Errors);
                    loadResultWindow.SetUnavailableComponents(unavailableComponents);
                    loadResultWindow.ShowDialog();
                }

                if (succeeded)
                {
                    circuitDisplay.Document = loadedDocument;
                    circuitDisplay.DrawConnections();
                    m_docPath = null; // Cannot be saved to imported format
                    m_documentTitle = System.IO.Path.GetFileName(ofd.FileName);
                    this.Title = m_documentTitle + " - Circuit Diagram";
                    m_undoManager = new UndoManager();
                    circuitDisplay.UndoManager = m_undoManager;
                    m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                    m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
                }
            }
        }

        private void mnuFileExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Title = "Export";

            string filter = "PNG (*.png)|*.png|Scalable Vector Graphics (*.svg)|*.svg|XPS (*.xps)|*.xps";
            // Add plugin exporters
            foreach (IDocumentWriter pluginWriter in PluginManager.EnabledExportWriters)
                filter += String.Format("|{0} (*{1})|*{1}", pluginWriter.FileTypeName, pluginWriter.FileTypeExtension);

            sfd.Filter = filter;
            sfd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (sfd.ShowDialog() == true)
            {
                string extension = System.IO.Path.GetExtension(sfd.FileName);
                if (extension == ".svg")
                {
                    SVGRenderer renderer = new SVGRenderer(circuitDisplay.Document.Size.Width, circuitDisplay.Document.Size.Height);
                    renderer.Begin();
                    circuitDisplay.Document.Render(renderer);
                    renderer.End();
                    System.IO.File.WriteAllBytes(sfd.FileName, renderer.SVGDocument.ToArray());
                }
                else if (extension == ".png")
                {
                    winExportPNG exportPNGWindow = new winExportPNG();
                    exportPNGWindow.Owner = this;
                    exportPNGWindow.OriginalWidth = circuitDisplay.Width;
                    exportPNGWindow.OriginalHeight = circuitDisplay.Height;
                    exportPNGWindow.Update();
                    if (exportPNGWindow.ShowDialog() == true)
                    {
                        WPFRenderer renderer = new WPFRenderer();
                        renderer.Begin();
                        circuitDisplay.Document.Render(renderer);
                        renderer.End();
                        using (var memoryStream = renderer.GetPNGImage2(exportPNGWindow.OutputWidth, exportPNGWindow.OutputHeight, circuitDisplay.Document.Size.Width, circuitDisplay.Document.Size.Height, exportPNGWindow.OutputBackgroundColour == "White"))
                        {
                            FileStream fileStream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                            memoryStream.WriteTo(fileStream);
                            fileStream.Close();
                        }
                    }
                }
                else if (extension == ".xps")
                {
                    WPFRenderer renderer = new WPFRenderer();
                    renderer.Begin();
                    circuitDisplay.Document.Render(renderer);
                    renderer.End();
                    FixedDocument doc = renderer.GetDocument(new Size(circuitDisplay.Document.Size.Width, circuitDisplay.Document.Size.Height));

                    using (XpsDocument xdoc = new XpsDocument(sfd.FileName, FileAccess.Write))
                    {
                        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xdoc);
                        writer.Write(doc.DocumentPaginator);
                    }
                }
                else if (extension == ".emf") // Disabled
                {
                    EMFRenderer renderer = new EMFRenderer((int)circuitDisplay.Document.Size.Width, (int)circuitDisplay.Document.Size.Height);
                    renderer.Begin();
                    circuitDisplay.Document.Render(renderer);
                    renderer.End();
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        renderer.WriteEnhMetafile(stream);
                    }
                }
                else
                {
                    // Create the document writer
                    IDocumentWriter writer = PluginManager.EnabledExportWriters[sfd.FilterIndex - 3];

                    IDictionary<IOComponentType, EmbedComponentData> embedComponents = new Dictionary<IOComponentType, EmbedComponentData>();
                    if (writer is IElementDocumentWriter)
                        (writer as IElementDocumentWriter).Document = circuitDisplay.Document.ToIODocument(out embedComponents);
                    writer.Begin();
                    if (writer is IVisualDocumentWriter)
                    {
                        (writer as IVisualDocumentWriter).RenderContext.Begin();
                        circuitDisplay.Document.Render((writer as IVisualDocumentWriter).RenderContext);
                        (writer as IVisualDocumentWriter).RenderContext.End();
                    }
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        writer.Write(stream);
                    }
                    writer.End();
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
            bool saved;
            SaveDocumentAs(out saved);
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
            if (!UndoManager.IsSavedState())
            {
                TaskDialogOptions tdOptions = new TaskDialogOptions();
                tdOptions.Title = "Circuit Diagram";
                tdOptions.MainInstruction = "Do you want to save changes to " + m_documentTitle + "?";
                tdOptions.CustomButtons = new string[] { "&Save", "Do&n't Save", "Cancel" };
                tdOptions.Owner = this;
                TaskDialogResult result = TaskDialog.Show(tdOptions);

                bool saved = false;
                if (result.CustomButtonResult == 0)
                    SaveDocument(out saved);

                if (result.CustomButtonResult == 2 || result.Result == TaskDialogSimpleResult.Cancel || (result.CustomButtonResult == 0 && !saved))
                    return; // Don't create new document
            }

            // Load the clicked item
            if ((e.OriginalSource as MenuItem).Header is string && System.IO.File.Exists((e.OriginalSource as MenuItem).Header as string))
                OpenDocument((e.OriginalSource as MenuItem).Header as string);
        }

        private void mnuFileOptions_Click(object sender, RoutedEventArgs e)
        {
            winOptions options = new winOptions();
            options.Owner = this;
            options.ComponentRepresentations = m_componentRepresentations;
            if (options.ShowDialog() == true)
            {
                CircuitDiagram.SettingsManager.Settings.Save();

                ApplySettings();

                circuitDisplay.DrawConnections();

                // Save implementation representations
                ImplementationConversionManager.SaveImplementationConversions(m_componentRepresentations);

                // Some importers may have been enabled/disabled
                LoadImportMenu();

                // Grid may have been enabled/disabled
                circuitDisplay.RenderBackground();
            }
        }

        private void mnuFileDocument_Click(object sender, RoutedEventArgs e)
        {
            string previousTitle = circuitDisplay.Document.Metadata.Title;
            string previousDescription = circuitDisplay.Document.Metadata.Description;

            winDocumentProperties documentInfoWindow = new winDocumentProperties();
            documentInfoWindow.Owner = this;
            documentInfoWindow.SetDocument(circuitDisplay.Document);
            documentInfoWindow.ShowDialog();

            if (circuitDisplay.Document.Metadata.Title != previousTitle || circuitDisplay.Document.Metadata.Description != previousDescription)
            {
                UndoAction editMetadataAction = new UndoAction(UndoCommand.ModifyMetadata, "Modify metadata");
                editMetadataAction.AddData("before", new string[2] { previousTitle, previousDescription});
                editMetadataAction.AddData("after", new string[2] { circuitDisplay.Document.Metadata.Title, circuitDisplay.Document.Metadata.Description });
                UndoManager.AddAction(editMetadataAction);
            }
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

        private void mnuEditResizeDocument(object sender, RoutedEventArgs e)
        {
            winDocumentSize docSizeWindow = new winDocumentSize();
            docSizeWindow.Owner = this;
            docSizeWindow.DocumentWidth = circuitDisplay.Document.Size.Width;
            docSizeWindow.DocumentHeight = circuitDisplay.Document.Size.Height;
            if (docSizeWindow.ShowDialog() == true)
            {
                Size newSize = new Size(docSizeWindow.DocumentWidth, docSizeWindow.DocumentHeight);
                if (newSize != circuitDisplay.Document.Size)
                {
                    UndoAction resizeAction = new UndoAction(UndoCommand.ResizeDocument, "Resize document");
                    resizeAction.AddData("before", circuitDisplay.Document.Size);
                    circuitDisplay.Document.Size = newSize;
                    circuitDisplay.DocumentSizeChanged();
                    resizeAction.AddData("after", newSize);
                    UndoManager.AddAction(resizeAction);
                }
            }
        }

        private void mnuFilePrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pDlg = new PrintDialog();
            if (pDlg.ShowDialog() == true)
            {
                WPFRenderer renderer = new WPFRenderer();
                renderer.Begin();
                circuitDisplay.Document.Render(renderer);
                renderer.End();
                FixedDocument document = renderer.GetDocument(new Size(pDlg.PrintableAreaWidth, pDlg.PrintableAreaHeight));
                pDlg.PrintDocument(document.DocumentPaginator, m_documentTitle + " - Circuit Diagram");
            }
        }
        #endregion

        #region RecentFiles
        /// <summary>
        /// Adds a file to the recent files list.
        /// </summary>
        /// <param name="path">Path of the file to add.</param>
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

        /// <summary>
        /// Populates the recent files menu.
        /// </summary>
        private void LoadRecentFiles()
        {
            string[] files = CircuitDiagram.SettingsManager.Settings.Read("recentfiles") as string[];
            if (files == null || (files.Length == 1 && String.IsNullOrEmpty(files[0])))
            {
                RecentFiles.Add("(empty)");
                return;
            }
            foreach (string file in files)
                RecentFiles.Add(file);
        }

        /// <summary>
        /// Saves the recent files list.
        /// </summary>
        private void SaveRecentFiles()
        {
            if (!(RecentFiles.Count == 1 && RecentFiles[0] == "(empty)"))
                CircuitDiagram.SettingsManager.Settings.Write("recentfiles", RecentFiles.ToArray());
            CircuitDiagram.SettingsManager.Settings.Save();
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
                if (!UndoManager.IsSavedState())
                {
                    TaskDialogOptions tdOptions = new TaskDialogOptions();
                    tdOptions.Title = "Circuit Diagram";
                    tdOptions.MainInstruction = "Do you want to save changes to " + m_documentTitle + "?";
                    tdOptions.CustomButtons = new string[] { "&Save", "Do&n't Save", "Cancel" };
                    tdOptions.Owner = this;
                    TaskDialogResult result = TaskDialog.Show(tdOptions);

                    bool saved = false;
                    if (result.CustomButtonResult == 0)
                        SaveDocument(out saved);

                    if (result.CustomButtonResult == 2 || result.Result == TaskDialogSimpleResult.Cancel || (result.CustomButtonResult == 0 && !saved))
                        return; // Don't create new document
                }

                CircuitDocument newDocument = new CircuitDocument();
                newDocument.Size = new Size(newDocumentWindow.DocumentWidth, newDocumentWindow.DocumentHeight);
                InitializeMetadata(newDocument);
                circuitDisplay.Document = newDocument;
                circuitDisplay.DrawConnections();
                this.Title = "Untitled - Circuit Diagram";
                m_documentTitle = "Untitled";
                m_docPath = "";
                m_undoManager = new UndoManager();
                circuitDisplay.UndoManager = m_undoManager;
                m_undoManager.ActionDelegate = new CircuitDiagram.UndoManager.UndoActionDelegate(UndoActionProcessor);
                m_undoManager.ActionOccurred += new EventHandler(m_undoManager_ActionOccurred);
            }
        }

        private static void InitializeMetadata(CircuitDocument newDocument)
        {
            newDocument.Metadata.Creator = SettingsManager.Settings.Read("ComputerUserName") as string;
            if (!SettingsManager.Settings.ReadBool("CreatorUseComputerUserName") && SettingsManager.Settings.HasSetting("CreatorName"))
                newDocument.Metadata.Creator = SettingsManager.Settings.Read("CreatorName") as string;
            newDocument.Metadata.Created = DateTime.Now;
            newDocument.Metadata.Application = ApplicationInfo.Name;
            newDocument.Metadata.AppVersion = ApplicationInfo.Version;
        }

        private void CommandOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open";
            ofd.Filter = "Supported Circuits (*.cddx;*.xml)|*.cddx;*.xml|Circuit Diagram Document (*.cddx)|*.cddx|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (ofd.ShowDialog() == true)
            {
                if (!UndoManager.IsSavedState())
                {
                    TaskDialogOptions tdOptions = new TaskDialogOptions();
                    tdOptions.Title = "Circuit Diagram";
                    tdOptions.MainInstruction = "Do you want to save changes to " + m_documentTitle + "?";
                    tdOptions.CustomButtons = new string[] { "&Save", "Do&n't Save", "Cancel" };
                    tdOptions.Owner = this;
                    TaskDialogResult result = TaskDialog.Show(tdOptions);

                    bool saved = false;
                    if (result.CustomButtonResult == 0)
                        SaveDocument(out saved);

                    if (result.CustomButtonResult == 2 || result.Result == TaskDialogSimpleResult.Cancel || (result.CustomButtonResult == 0 && !saved))
                        return; // Don't create new document
                }

                OpenDocument(ofd.FileName);
            }
        }

        private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool saved;
            SaveDocument(out saved);
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

        private void FlipComponentCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            circuitDisplay.FlipComponentCommand_CanExecute(sender, e);
        }

        private void FlipComponentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            circuitDisplay.FlipComponentCommand(sender, e);
        }

        private void RotateComponentCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            circuitDisplay.RotateComponentCommand_CanExecute(sender, e);
        }

        private void RotateComponentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            circuitDisplay.RotateComponentCommand_Executed(sender, e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((circuitDisplay.IsMouseOver || mainToolbox.IsMouseOver) && !e.IsRepeat && e.IsDown)
            {
                // Check component shortcuts
                if (e.Key == Key.V) // Move/select
                {
                    ToolboxSelectSelectTool();
                    SetStatusText("Select tool");

                    e.Handled = true;
                }
                else if (e.Key == Key.W) // Wire
                {
                    ToolboxSelectWire();
                    SetStatusText("Placing wire");

                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    circuitDisplay.DeleteComponentCommand(this, null);

                    e.Handled = true;
                }
                else if (e.Key == Key.Q)
                {
                    ToolboxSelectSearchTool();
                    quickCommand.IsOpen = true;
                    e.Handled = true;
                }
                else
                {
                    // Check custom toolbox entries
                    if (m_toolboxShortcuts.ContainsKey(e.Key))
                    {
                        var identifier = m_toolboxShortcuts[e.Key];

                        // Set selected component
                        mainToolbox.SetSelectedCategoryItem(identifier);
                        circuitDisplay.NewComponentData = identifier.ToString();

                        // Set status text
                        SetStatusText(String.Format("Placing {0}", identifier.DisplayName));

                        e.Handled = true;
                    }
                }
            }
        }

        private void CommandCut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = circuitDisplay.SelectedComponents.Count > 0;
        }

        private void CommandCut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Copy components
            CommandCopy_Executed(sender, e);

            // Delete components
            circuitDisplay.DeleteComponentCommand(sender, e);
        }

        private void CommandCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = circuitDisplay.SelectedComponents.Count > 0;
        }

        private void CommandCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ComponentSerializer serializer = new ComponentSerializer();
            foreach (Component component in circuitDisplay.SelectedComponents)
                serializer.AddComponent(component);
            Clipboard.SetData("CircuitDiagram.ComponentData", serializer.ToString());
        }

        private void CommandPaste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsData("CircuitDiagram.ComponentData");
        }

        private void CommandPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ComponentDeserializer deserializer = new ComponentDeserializer(Clipboard.GetData("CircuitDiagram.ComponentData") as string);
            deserializer.Components.ForEach(c => circuitDisplay.Document.Elements.Add(c));
            circuitDisplay.SetSelectedComponents(deserializer.Components);

            UndoAction action = new UndoAction(UndoCommand.AddComponents, "Add components", deserializer.Components.ToArray());
            UndoManager.AddAction(action);
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

        /// <summary>
        /// Performs an undo or redo.
        /// </summary>
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
                                component.InvalidateVisual();
                                circuitDisplay.RedrawComponent(component);
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
                    case UndoCommand.AddComponents:
                        {
                            Component[] components = (e.Action.GetDefaultData() as Component[]);
                            foreach (Component component in components)
                            {
                                component.DisconnectConnections();
                                circuitDisplay.Document.Elements.Remove(component);
                            }
                        }
                        break;
                    case UndoCommand.ResizeDocument:
                        {
                            circuitDisplay.Document.Size = e.Action.GetData<Size>("before");
                            circuitDisplay.DocumentSizeChanged();
                        }
                        break;
                    case UndoCommand.ModifyMetadata:
                        {
                            string[] metadata = e.Action.GetData<string[]>("before");
                            circuitDisplay.Document.Metadata.Title = metadata[0];
                            circuitDisplay.Document.Metadata.Description = metadata[1];
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
                                component.InvalidateVisual();
                                circuitDisplay.RedrawComponent(component);
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
                    case UndoCommand.AddComponents:
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
                    case UndoCommand.ResizeDocument:
                        {
                            circuitDisplay.Document.Size = e.Action.GetData<Size>("after");
                            circuitDisplay.DocumentSizeChanged();
                        }
                        break;
                    case UndoCommand.ModifyMetadata:
                        {
                            string[] metadata = e.Action.GetData<string[]>("after");
                            circuitDisplay.Document.Metadata.Title = metadata[0];
                            circuitDisplay.Document.Metadata.Description = metadata[1];
                        }
                        break;
                }
            }

            circuitDisplay.DrawConnections();
        }
        #endregion

        private void quickCommand_ComponentSelected(object sender, EventArgs e, string componentData)
        {
            circuitDisplay.NewComponentData = componentData;
        }
    }
}
