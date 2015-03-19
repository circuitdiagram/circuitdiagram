using CircuitDiagram.Components.Description;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winComponentDetails.xaml
    /// </summary>
    public partial class winComponentDetails : MetroWindow
    {
        public winComponentDetails(ComponentDescription component)
        {
            InitializeComponent();

            var reader = new IO.Descriptions.BinaryDescriptionReader();
            using (var file = System.IO.File.OpenRead(component.Source.Path))
            {
                if (reader.Read(file))
                {
                    lbxResources.ItemsSource = reader.Items;
                }
            }
        }
    }
}
