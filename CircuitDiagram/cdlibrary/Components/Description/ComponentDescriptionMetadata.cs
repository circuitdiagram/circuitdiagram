// ComponentDescriptionMetadata.cs
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
using System.Windows.Media;

namespace CircuitDiagram.Components.Description
{
    public class ComponentDescriptionMetadata
    {
        public Guid GUID { get; set; }
        public Version Version { get; set; }
        public string Author { get; set; }
        public string AdditionalInformation { get; set; }
        public string Type { get; set; }

        //public object Icon { get; set; }
        //public string IconMimeType { get; set; }
        //public byte[] IconData { get; set; }

        public LocationType Location { get; set; }
        public List<ComponentConfiguration> Configurations { get; private set; }
        public string ImplementSet { get; set; }
        public string ImplementItem { get; set; }
        public SignatureStatus Signature { get; set; }
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the icon for this component.
        /// </summary>
        public MultiResolutionImage Icon { get; set; }

        public ComponentDescriptionMetadata()
        {
            GUID = Guid.Empty;
            Version = new System.Version(1, 0);
            Author = "Unknown";
            AdditionalInformation = "";
            Type = "Binary (*.cdcom)";
            Location = LocationType.None;
            Configurations = new List<ComponentConfiguration>();
            Signature = new SignatureStatus();
            Created = DateTime.Now;
        }

        public enum LocationType
        {
            None,
            Installed,
            Embedded
        }
    }
}
