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
using CircuitDiagram.View.Services;
using CircuitDiagram.View.ToolboxView;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace CircuitDiagram.View
{
    public class MainViewModel : BindableBase
    {
        private CircuitDocument document;
        private ToolboxEntry[][] availableComponents;
        private ToolboxEntry selectedToolboxItem;

        private readonly ToolboxEntry select = new ToolboxEntry
        {
            Name = "Select"
        };

        public MainViewModel(IComponentDescriptionService descriptionService,
                             IToolboxReader toolboxReader,
                             IConfigurationValues configurationValues)
        {
            DescriptionLookup = descriptionService;
            descriptionService.LoadDescriptions();

            using (var toolboxStream = File.OpenRead(configurationValues.ToolboxConfigurationFile))
                AvailableComponents = new[] {new[] {select}}.Concat(toolboxReader.GetToolbox(toolboxStream, descriptionService.AvailableTypes)).ToArray();

            NewDocument();
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

        public IComponentDescriptionLookup DescriptionLookup { get; }

        public ICommand NewDocumentCommand => new DelegateCommand(NewDocument);

        public ICommand OpenDocumentCommand => new DelegateCommand(OpenDocument);

        public ICommand SaveDocumentAsCommand => new DelegateCommand(SaveDocumentAs);

        private void NewDocument()
        {
            Document = new CircuitDocument
            {
                Size = new Size(640, 480)
            };
        }

        private void OpenDocument()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Supported Circuits (*.cddx;*.xml)|*.cddx;*.xml|Circuit Diagram Document (*.cddx)|*.cddx|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (ofd.ShowDialog() == true)
            {
                var reader = new CircuitDiagramDocumentReader();
                using (var fs = new FileStream(ofd.FileName, FileMode.Open))
                    Document = reader.ReadCircuit(fs);
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
    }
}
