using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace CircuitDiagram
{
    [ValueConversion(typeof(Key), typeof(string))]
    class KeyTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((Key)value) == Key.None)
                return "";
            else
                return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (IsValidLetterKey(value.ToString().ToUpper()))
                    return Enum.Parse(typeof(Key), value.ToString().ToUpper());
                else
                    return Key.None;
            }
            catch (Exception)
            {
                return Key.None;
            }
        }

        public static bool IsValidLetterKey(string value)
        {
            if (value == null)
                return false;
            System.Text.RegularExpressions.Regex regexMatch = new System.Text.RegularExpressions.Regex("[A-Z]");
            return regexMatch.IsMatch(value);
        }
    }
}
