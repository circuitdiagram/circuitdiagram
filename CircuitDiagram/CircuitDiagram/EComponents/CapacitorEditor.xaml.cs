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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CircuitDiagram.EComponents
{
    /// <summary>
    /// Interaction logic for CapacitorEditor.xaml
    /// </summary>
    public partial class CapacitorEditor : ComponentEditor
    {
        public CapacitorEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
            tbxCapacitance.Text = ((Capacitor)component).Capacitance.ToString();
        }

        public override void UpdateChanges(EComponent component)
        {
            ((Capacitor)component).Capacitance = double.Parse(tbxCapacitance.Text);
        }
    }
}
