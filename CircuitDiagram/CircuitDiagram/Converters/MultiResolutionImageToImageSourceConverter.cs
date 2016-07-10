using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CircuitDiagram.TypeDescriptionIO;

namespace CircuitDiagram
{
    public class MultiResolutionImageToImageSourceConverter : IValueConverter
    {
        public double DPI { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MultiResolutionImage image = value as MultiResolutionImage;
            if (image == null)
                return null;

            return image.GetBestIcon(DPI);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
