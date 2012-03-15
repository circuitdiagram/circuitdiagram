// winComponents.xaml.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CircuitDiagram.Components;
using System.Collections;
using System.Globalization;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winComponents.xaml
    /// </summary>
    public partial class winComponents : Window
    {
        public winComponents()
        {
            InitializeComponent();
            lbxComponents.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ComponentName", System.ComponentModel.ListSortDirection.Ascending));
        }

        public object Components
        {
            get { return this.DataContext; }
            set { this.DataContext = value; }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void hyperlinkGetMoreComponents_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.circuit-diagram.org/components");
        }
    }

    class EmbedByDefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            ComponentDescription description = value as ComponentDescription;
            if (description != null)
            {
                return ComponentHelper.IsStandardComponent(description) ? "Yes" : "No";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
