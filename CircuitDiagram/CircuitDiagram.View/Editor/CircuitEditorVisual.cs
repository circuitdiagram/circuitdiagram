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
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;

namespace CircuitDiagram.View.Editor
{
    class CircuitEditorVisual : CircuitEditorBase
    {
        private const int BackgroundVisualIndex = 0;
        private const int HighlightBoxVisualsOffset = 1;
        private int ConnectionsVisualIndex => HighlightBoxVisualsOffset + HighlightBoxVisuals.Count;
        private int ElementVisualsIndexOffset => ConnectionsVisualIndex + 1;
        private int FirstResizeVisualIndex => ElementVisualsIndexOffset + ElementVisuals.Count;
        private int SecondResizeVisualIndex => FirstResizeVisualIndex + 1;

        protected readonly DrawingVisual BackgroundVisual;
        protected readonly Dictionary<IPositionalElement, ElementHighlightBoxVisual> HighlightBoxVisuals;
        protected readonly ConnectionsVisual ConnectionsVisual;
        protected readonly Dictionary<IPositionalElement, ElementDrawingVisual> ElementVisuals;
        protected readonly ResizeHandle FirstResizeVisual;
        protected readonly ResizeHandle SecondResizeVisual;

        public CircuitEditorVisual()
        {
            BackgroundVisual = new DrawingVisual();
            AddChild(BackgroundVisual);
            ConnectionsVisual = new ConnectionsVisual();
            AddChild(ConnectionsVisual);
            FirstResizeVisual = new ResizeHandle();
            AddChild(FirstResizeVisual);
            SecondResizeVisual = new ResizeHandle();
            AddChild(SecondResizeVisual);
            ElementVisuals = new Dictionary<IPositionalElement, ElementDrawingVisual>();
            HighlightBoxVisuals = new Dictionary<IPositionalElement, ElementHighlightBoxVisual>();
        }

        protected void AddChild(Visual visual)
        {
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        protected override int VisualChildrenCount => 4 + HighlightBoxVisuals.Count + ElementVisuals.Count;

        protected override Visual GetVisualChild(int requestedVisual)
        {
            if (requestedVisual == BackgroundVisualIndex)
                return BackgroundVisual;

            if (requestedVisual == ConnectionsVisualIndex)
                return ConnectionsVisual;

            if (IsRequestForHiglightBoxVisual(requestedVisual))
                return HighlightBoxVisuals.ElementAt(requestedVisual - HighlightBoxVisualsOffset).Value;

            if (IsRequestForElementVisual(requestedVisual))
                return ElementVisuals.ElementAt(requestedVisual - ElementVisualsIndexOffset).Value;

            if (requestedVisual == FirstResizeVisualIndex)
                return FirstResizeVisual;

            if (requestedVisual == SecondResizeVisualIndex)
                return SecondResizeVisual;

            throw new IndexOutOfRangeException("The requested visual does not exist.");
        }

        private bool IsRequestForHiglightBoxVisual(int requestedVisual)
        {
            return requestedVisual < HighlightBoxVisuals.Count + HighlightBoxVisualsOffset;
        }

        private bool IsRequestForElementVisual(int requestedVisual)
        {
            return requestedVisual < ElementVisuals.Count + ElementVisualsIndexOffset;
        }
    }
}
