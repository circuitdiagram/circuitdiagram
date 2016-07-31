// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.View.Dialogs;
using CircuitDiagram.View.Services;
using CircuitDiagram.View.ToolboxView;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace CircuitDiagram.View
{
    public class MainViewModel : BindableBase
    {
        private readonly IDialogService dialogService;
        private readonly IDocumentService documentService;
        private readonly AboutViewModel aboutViewModel;
        private readonly CheckForUpdatesViewModel checkForUpdatesViewModel;
        private readonly Func<NewDocumentViewModel> newDocumentViewModelProvider;
        private CircuitDocument document;
        private ToolboxEntry[][] availableComponents;
        private ToolboxEntry selectedToolboxItem;

        private readonly ToolboxEntry select = new ToolboxEntry
        {
            Name = "Select"
        };

        private ObservableCollection<PositionalComponent> selectedElements;

        public MainViewModel(IComponentDescriptionService descriptionService,
                             IDialogService dialogService,
                             IDocumentService documentService,
                             IToolboxReader toolboxReader,
                             IConfigurationValues configurationValues,
                             AboutViewModel aboutViewModel,
                             CheckForUpdatesViewModel checkForUpdatesViewModel,
                             Func<NewDocumentViewModel> newDocumentViewModelProvider)
        {
            this.dialogService = dialogService;
            this.documentService = documentService;
            this.aboutViewModel = aboutViewModel;
            this.checkForUpdatesViewModel = checkForUpdatesViewModel;
            this.newDocumentViewModelProvider = newDocumentViewModelProvider;
            SelectedElements = new ObservableCollection<PositionalComponent>();
            DescriptionLookup = descriptionService;
            descriptionService.LoadDescriptions();

            using (var toolboxStream = File.OpenRead(configurationValues.ToolboxConfigurationFile))
                AvailableComponents = new[] {new[] {select}}.Concat(toolboxReader.GetToolbox(toolboxStream, descriptionService.AvailableTypes)).ToArray();

            Document = new CircuitDocument
            {
                Size = new Size(640, 480)
            };
        }

        public ToolboxEntry[][] AvailableComponents
        {
            get { return availableComponents; }
            set
            {
                availableComponents = value;
                OnPropertyChanged();
            }
        }

        public ToolboxEntry SelectedToolboxItem
        {
            get { return selectedToolboxItem; }
            set
            {
                selectedToolboxItem = value;
                OnPropertyChanged();
            }
        }

        public CircuitDocument Document
        {
            get { return document; }
            set
            {
                document = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PositionalComponent> SelectedElements
        {
            get { return selectedElements; }
            set
            {
                selectedElements = value;
                OnPropertyChanged();
            }
        }

        public IComponentDescriptionLookup DescriptionLookup { get; }

        public ICommand NewDocumentCommand => new DelegateCommand(NewDocument);

        public ICommand OpenDocumentCommand => new DelegateCommand(OpenDocument);

        public ICommand SaveDocumentAsCommand => new DelegateCommand(SaveDocumentAs);

        public ICommand CheckForUpdatesCommand => new DelegateCommand(CheckForUpdates);

        public ICommand AboutCommand => new DelegateCommand(() => dialogService.ShowDialog("About Circuit Diagram", aboutViewModel));

        private void NewDocument()
        {
            var viewModel = newDocumentViewModelProvider();
            if (dialogService.ShowDialog("New Document", viewModel) != true)
                return;

            Document = new CircuitDocument
            {
                Size = new Size(viewModel.DocumentWidth, viewModel.DocumentHeight)
            };
        }

        private void OpenDocument()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Circuit Diagram Document (*.cddx)|*.cddx|All Files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (ofd.ShowDialog() == true)
            {
                var reader = new CircuitDiagramDocumentReader();
                using (var fs = new FileStream(ofd.FileName, FileMode.Open))
                    Document = documentService.OpenDocument(fs);
            }
        }

        private void SaveDocumentAs()
        {
            var sfd = new SaveFileDialog
            {
                Title = "Save As",
                Filter = "Circuit Diagram Document (*.cddx)|*.cddx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (sfd.ShowDialog() == true)
            {
                var writer = new CircuitDiagramDocumentWriter();
                using (var fs = new FileStream(sfd.FileName, FileMode.Create))
                    writer.WriteCircuit(Document, fs);
            }
        }

        private void CheckForUpdates()
        {
            checkForUpdatesViewModel.CheckForUpdates();
            dialogService.ShowDialog("Updates", checkForUpdatesViewModel);
        }
    }
}
