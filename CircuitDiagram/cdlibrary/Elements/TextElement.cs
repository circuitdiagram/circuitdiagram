// TextElement.cs
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
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.Elements
{
    class TextElement : ICircuitElement
    {
        public event EventHandler Updated;

        private DrawingVisual m_drawingVisual;

        public System.Windows.Media.Visual Visual { get { return m_drawingVisual; } }

        public TextElement()
        {
            m_drawingVisual = new DrawingVisual();
            using (DrawingContext dc = m_drawingVisual.RenderOpen())
                dc.DrawLine(new Pen(Brushes.Black, 2d), new Point(0, 0), new Point(10, 10));
        }

        public void Render(Render.IRenderContext dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateVisual()
        {
            if (Updated != null)
                Updated(this, new EventArgs());
        }
    }
}
