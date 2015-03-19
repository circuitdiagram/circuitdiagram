using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CircuitDiagram
{
    static class MultiResolutionImageExtensions
    {
        public static BitmapImage GetBestIcon(this MultiResolutionImage icon, double dpi)
        {
            BitmapImage chosenImage = null;
            foreach (var res in icon)
            {
                MemoryStream tempStream = new MemoryStream(res.Data);
                var tempIcon = new System.Windows.Media.Imaging.BitmapImage();
                tempIcon.BeginInit();
                tempIcon.StreamSource = tempStream;
                tempIcon.EndInit();

                if (chosenImage == null
                    || (chosenImage.DpiX < dpi && tempIcon.DpiX > chosenImage.DpiX))
                    chosenImage = tempIcon;
            }

            return chosenImage;
        }
    }
}
