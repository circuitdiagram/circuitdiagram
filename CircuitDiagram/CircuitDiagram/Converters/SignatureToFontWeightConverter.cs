using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace CircuitDiagram
{
    public class SignatureToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            SignatureStatus status = value as SignatureStatus;
            if (status == null)
                return FontWeights.Normal;

            if (!status.IsSigned)
                return FontWeights.Normal;
            else if (status.IsSignatureValid)
                return FontWeights.Bold;
            else
                return FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
