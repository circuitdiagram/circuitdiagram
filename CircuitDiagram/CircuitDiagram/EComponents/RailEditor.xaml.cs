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
    /// Interaction logic for ResistorEditor.xaml
    /// </summary>
    public partial class RailEditor : ComponentEditor
    {
        public RailEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
            tbxVoltage.Text = ((Rail)component).Voltage.ToString();
        }

        public override void UpdateChanges(EComponent component)
        {
            ((Rail)component).Voltage = double.Parse(tbxVoltage.Text);
        }
    }
}