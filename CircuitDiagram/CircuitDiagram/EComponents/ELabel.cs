using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace CircuitDiagram.EComponents
{
    class ELabel : EComponent
    {
        [ComponentSerializable("text", "Text")]
        public string Text { get; set; }

        public override System.Windows.Rect BoundingBox
        {
            get
            {
                FormattedText text = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
                return new System.Windows.Rect(this.StartLocation.X - text.Width / 2, this.StartLocation.Y - text.Height / 2, text.Width, text.Height);
            }
        }

        public ELabel()
        {
            CanResize = false;
            Text = "Label";
            this.Editor = new AutomaticEditor(this);
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color color)
        {
            FormattedText text = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawText(Text, "Arial", 12d, color, new Point(StartLocation.X - text.Width / 2, StartLocation.Y - text.Height / 2));
        }
    }
}
