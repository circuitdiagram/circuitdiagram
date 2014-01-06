using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CircuitDiagram
{
    class SignatureStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            SignatureStatus status = value as SignatureStatus;
            if (status == null)
                return "No";

            if (!status.IsSigned)
                return "No";
            else if (status.IsSignatureValid)
                return "Yes";
            else
                return "Signature invalid";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
