using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CircuitDiagram.TypeDescriptionIO.Binary;

namespace CircuitDiagram
{
    public class BinaryResourceToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BinaryResource resource = value as BinaryResource;
            if (resource == null)
                return null;

            MemoryStream tempStream = new MemoryStream(resource.Buffer);
            var tempIcon = new System.Windows.Media.Imaging.BitmapImage();
            tempIcon.BeginInit();
            tempIcon.StreamSource = tempStream;
            tempIcon.EndInit();

            return tempIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
