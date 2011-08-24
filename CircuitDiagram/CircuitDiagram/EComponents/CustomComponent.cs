using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace CircuitDiagram.EComponents
{
    public class CustomComponent : EComponent
    {
        public BitmapSource Image { get; set; }

        private string m_path = "";

        [ComponentSerializable(ComponentSerializeOptions.StoreLowercase)]
        public string Path
        {
            get
            {
                return m_path.Replace('\\', '/').Replace(':',';');
            }
            set
            {
                if (!File.Exists(value.Replace(';', ':')))
                {
                    m_path = null;
                    Image = null;
                    return;
                }
                m_path = value.Replace(';', ':');
                Image = new BitmapImage(new Uri(m_path.Replace(';', ':')));
            }
        }

        public override System.Windows.Rect BoundingBox
        {
            get
            {
                if (Image == null)
                    return new Rect(StartLocation, new Size(100, 100));
                else
                    return new System.Windows.Rect(StartLocation, new Size(Image.Width, Image.Height));
            }
        }

        public CustomComponent()
        {
            CanResize = false;
            base.Editor = new CustomComponentEditor(this);
        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            if (Image == null)
                dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(StartLocation, new Size(100,100)));
            else
                dc.DrawImage(StartLocation, Image);
        }
    }
}
