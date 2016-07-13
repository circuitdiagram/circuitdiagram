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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace CircuitDiagram.View.Dialogs
{
    public class NewDocumentViewModel : BindableBase
    {
        private double documentWidth;
        private double documentHeight;

        public NewDocumentViewModel()
        {
            DocumentWidth = 640;
            DocumentHeight = 480;
        }

        public double DocumentWidth
        {
            get { return documentWidth; }
            set
            {
                documentWidth = value;
                OnPropertyChanged();
            }
        }

        public double DocumentHeight
        {
            get { return documentHeight; }
            set
            {
                documentHeight = value;
                OnPropertyChanged();
            }
        }
    }
}
