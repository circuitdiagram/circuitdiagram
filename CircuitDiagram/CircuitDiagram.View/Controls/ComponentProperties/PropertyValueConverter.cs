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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.View.Controls.ComponentProperties
{
    class PropertyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyValue = (PropertyValue)value;

            switch (propertyValue.PropertyType)
            {
                case PropertyValue.Type.Boolean:
                    return propertyValue.BooleanValue;
                case PropertyValue.Type.String:
                    return propertyValue.StringValue;
                case PropertyValue.Type.Numeric:
                    return propertyValue.NumericValue;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyType = (PropertyType)parameter;

            switch (propertyType)
            {
                case PropertyType.Boolean:
                    return new PropertyValue((bool)value);
                case PropertyType.String:
                case PropertyType.Enum:
                    return new PropertyValue((string)value);
                case PropertyType.Decimal:
                case PropertyType.Integer:
                    return new PropertyValue(double.Parse((string)value));
            }

            return null;
        }
    }
}
