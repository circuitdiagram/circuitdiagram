using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    class ExternalConnection : EComponent
    {
        public string ConnectionText { get; set; }
        public bool ConnectionTopLeft { get; set; }

        public ExternalConnection()
        {
            ConnectionText = "input";
            ConnectionTopLeft = true;
            this.Editor = new ExternalConnectionEditor();
        }

        public override double MinimumWidth
        {
            get
            {
                return 40f;
            }
        }

        public override Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return base.BoundingBox;
                else
                {
                    FormattedText txt = new FormattedText(ConnectionText, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12f, Brushes.Black);
                    return Rect.Inflate(base.BoundingBox, new Size(txt.Width / 2f, 0f));
                }
            }
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color color)
        {
            FormattedText txt = new FormattedText(ConnectionText, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12f, Brushes.Black);

            if (ConnectionTopLeft)
            {
                if (Horizontal)
                {
                    dc.DrawLine(color, 2f, Point.Add(StartLocation, new Vector(txt.Width + 13f, 0f)), EndLocation);
                    dc.DrawEllipse(Colors.Transparent, color, 2f, Point.Add(StartLocation, new Vector(txt.Width + 10f, 0f)), 3f, 3f);
                    dc.DrawText(ConnectionText, "Arial", 12f, color, Point.Add(StartLocation, new Vector(0d, -txt.Height / 2)));
                }
                else
                {
                    dc.DrawLine(color, 2f, Point.Add(StartLocation, new Vector(0f, txt.Height + 13f)), EndLocation);
                    dc.DrawEllipse(Colors.Transparent, color, 2f, Point.Add(StartLocation, new Vector(0f, txt.Height + 10f)), 3f, 3f);
                    dc.DrawText(ConnectionText, "Arial", 12f, color, Point.Add(StartLocation, new Vector(-txt.Width / 2, 0f)));
                }
            }
            else
            {
                if (Horizontal)
                {
                    dc.DrawLine(color, 2f, StartLocation, Point.Add(EndLocation, new Vector(-(txt.Width + 13f), 0f)));
                    dc.DrawEllipse(Colors.Transparent, color, 2f, Point.Add(EndLocation, new Vector(-(txt.Width + 10f), 0f)), 3f, 3f);
                    dc.DrawText(ConnectionText, "Arial", 12f, color, Point.Add(EndLocation, new Vector(-txt.Width, -txt.Height / 2)));
                }
                else
                {
                    dc.DrawLine(color, 2f, StartLocation, Point.Add(EndLocation, new Vector(0f, -(txt.Height + 13f))));
                    dc.DrawEllipse(Colors.Transparent, color, 2f, Point.Add(EndLocation, new Vector(0f, -(txt.Height + 10f))), 3f, 3f);
                    dc.DrawText(ConnectionText, "Arial", 12f, color, Point.Add(EndLocation, new Vector(-txt.Width / 2, -txt.Height)));
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                reader.MoveToAttribute("text");
                ConnectionText = reader.ReadContentAsString();
                reader.MoveToAttribute("topleft");
                string connectionTL = reader.ReadContentAsString();
                if (connectionTL.ToLower() == "true")
                    ConnectionTopLeft = true;
                else
                    ConnectionTopLeft = false;
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("text", ConnectionText);
            writer.WriteAttributeString("topleft", ConnectionTopLeft.ToString());
        }
    }
}
