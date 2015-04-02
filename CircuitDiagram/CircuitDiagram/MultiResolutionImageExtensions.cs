using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            // Load icons if not already loaded
            if (icon.LoadedIcons.Count == 0)
                LoadIcons(icon);

            BitmapImage chosenImage = null;
            foreach (var res in icon.LoadedIcons)
            {
                var image = res as BitmapImage;
                if (chosenImage == null || // Nothing selected yet, or
                    // Current is lower DPI than screen and this one is higher than current, or
                    (chosenImage.DpiX <= dpi && image.DpiX > chosenImage.DpiX) ||
                    // Current is higher DPI than screen and this one is lower but still higher DPI than screen
                    (chosenImage.DpiX > dpi && image.DpiX < chosenImage.DpiX && image.DpiX >= dpi))
                    chosenImage = image;
            }

            return chosenImage;
        }

        public static void LoadIcons(this MultiResolutionImage icon)
        {
            icon.LoadedIcons.Clear();

            foreach (var res in icon)
            {
                MemoryStream tempStream = new MemoryStream(res.Data);
                var tempIcon = new System.Windows.Media.Imaging.BitmapImage();
                tempIcon.BeginInit();
                tempIcon.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                tempIcon.StreamSource = tempStream;
                tempIcon.EndInit();

                icon.LoadedIcons.Add(tempIcon);
            }
        }
    }
}
