using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    class Fuse : EComponent
    {
        [ComponentSerializable(ComponentSerializeOptions.StoreLowercase)]
        public double Value { get; set; }

        [ComponentSerializable(options: ComponentSerializeOptions.StoreLowercase | ComponentSerializeOptions.DisplayAlignLeft, displayName:"Show value")]
        public bool DisplayValue { get; set; }

        public string ValueString
        {
            get { return String.Format("{0} A", Value); }
        }

        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (DisplayValue)
                {
                    FormattedText text = new FormattedText(ValueString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 17d - text.Height / 2), new Point(EndLocation.X, EndLocation.Y + 8d));
                    else
                        return new Rect(new Point(StartLocation.X - text.Width - 15d, StartLocation.Y), new Point(EndLocation.X + 8d, EndLocation.Y));
                }
                else
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y- 8d), new Point(EndLocation.X, EndLocation.Y + 8d));
                    else
                        return new Rect(new Point(StartLocation.X - 8d, StartLocation.Y), new Point(EndLocation.X + 8d, EndLocation.Y));
                }
            }
        }

        public Fuse()
        {
            DisplayValue = false;
            Value = 1;
            this.Editor = new AutomaticEditor(this);
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            if (Horizontal)
            {
                dc.DrawLine(colour, 2d, StartLocation, EndLocation);
                dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y - 8d, 40d, 16d));
                FormattedText text = new FormattedText(ValueString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                if (DisplayValue)
                    dc.DrawText(ValueString, "Arial", 10d, colour, new Point(StartLocation.X + Size.Width / 2 - text.Width / 2, StartLocation.Y - 17d - text.Height / 2));
            }
            else
            {
                dc.DrawLine(colour, 2d, StartLocation, EndLocation);
                dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(StartLocation.X  - 8d, StartLocation.Y + Size.Height / 2 - 20d, 16d, 40d));
                FormattedText text = new FormattedText(ValueString, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10d, new SolidColorBrush(Colors.Black));
                if (DisplayValue)
                    dc.DrawText(ValueString, "Arial", 10d, colour, new Point(StartLocation.X - text.Width - 15d, StartLocation.Y + Size.Height / 2 - text.Height / 2));
            }
        }
    }
}
