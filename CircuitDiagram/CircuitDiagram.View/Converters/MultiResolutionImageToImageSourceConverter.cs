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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using CircuitDiagram.TypeDescriptionIO;

namespace CircuitDiagram.View.Converters
{
    class MultiResolutionImageToImageSourceConverter : IValueConverter
    {
        public double DPI { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var image = value as MultiResolutionImage;
            if (image == null)
                return null;

            return GetBestIcon(image, DPI);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        BitmapImage GetBestIcon(MultiResolutionImage icon, double dpi)
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

        void LoadIcons(MultiResolutionImage icon)
        {
            icon.LoadedIcons.Clear();

            foreach (var res in icon)
            {
                var tempStream = new MemoryStream(res.Data);
                var tempIcon = new BitmapImage();
                tempIcon.BeginInit();
                tempIcon.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                tempIcon.StreamSource = tempStream;
                tempIcon.EndInit();

                icon.LoadedIcons.Add(tempIcon);
            }
        }
    }
}
