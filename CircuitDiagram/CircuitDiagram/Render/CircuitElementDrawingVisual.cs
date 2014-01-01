using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using CircuitDiagram.Elements;
using CircuitDiagram.Components.Description.Render;

namespace CircuitDiagram
{
    class CircuitElementDrawingVisual : DrawingVisual
    {
        public ICircuitElement CircuitElement { get; private set; }

        public CircuitElementDrawingVisual(ICircuitElement circuitElement)
        {
            CircuitElement = circuitElement;

            this.Offset = circuitElement.Location;
            
            circuitElement.Updated += (o, e) =>
                {
                    this.Offset = circuitElement.Location;
                };
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // Hit if within bounding box

            Point pt = hitTestParameters.HitPoint;

            return new PointHitTestResult(this, pt);
        }

        public void UpdateVisual()
        {
            using (DrawingContext dc = base.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(this.Offset.X);
                guidelines.GuidelinesY.Add(this.Offset.Y);
                dc.PushGuidelineSet(guidelines);

                Render.DrawingRenderer renderer = new Render.DrawingRenderer(dc);
                renderer.Begin();

                CircuitElement.Render(renderer, false);

                renderer.End();
            }
        }
    }
}
