using CircuitDiagram.Components.Description;
using CircuitDiagram.DPIWindow;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winComponentDetails.xaml
    /// </summary>
    public partial class winComponentDetails : MetroDPIWindow
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
