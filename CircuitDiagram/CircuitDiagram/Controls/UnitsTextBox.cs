using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CircuitDiagram.Controls
{
    public class UnitsTextBox : TextBox
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(UnitsTextBox));

        public double Value
        {
            get { return UnitsTextBoxConversions.ExpandToDouble(Text); }
            set { Text = UnitsTextBoxConversions.ContractToString(value); }
        }
    }

    public static class UnitsTextBoxConversions
    {
        private static readonly Dictionary<string, double> Multipliers = new Dictionary<string, double>
        {
            ["p"] = 1e-12,
            ["n"] = 1e-9,
            ["u"] = 1e-6,
            ["m"] = 1e-3,
            [""] = 1.0,
            ["k"] = 1e3,
            ["M"] = 1e6,
        };

        internal static double ExpandToDouble(string value)
        {
            if (value.Length < 1)
                return 0.0;

            char lastChar = value.Last();
            string text = value.Substring(0, value.Length - 1);

            double multiplier;
            if (!Multipliers.TryGetValue(lastChar.ToString(), out multiplier))
            {
                text = value;
                multiplier = 1.0;
            }

            double val;
            double.TryParse(text, out val);
            return val * multiplier;
        }

        internal static string ContractToString(double value)
        {
            foreach (var m in Multipliers.Reverse())
            {
                if (value >= m.Value)
                    return (value / m.Value).ToString() + m.Key;
            }

            return value.ToString();
        }
    }
}
