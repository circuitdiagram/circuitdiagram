using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using CircuitDiagram.Components.Description;

namespace CircuitDiagram
{
    public class SignatureToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            SignatureStatus status = value as SignatureStatus;
            if (status == null)
                return Brushes.Black;

            if (!status.IsSigned)
                return Brushes.Black;
            else if (status.IsSignatureValid)
                return Brushes.LimeGreen;
            else
                return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
