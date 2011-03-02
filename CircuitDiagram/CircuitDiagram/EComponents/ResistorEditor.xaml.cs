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
    public partial class ResistorEditor : ComponentEditor
    {
        public ResistorEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
            tbxResistance.Text = ((Resistor)component).Resistance.ToString();
        }

        public override void UpdateChanges(EComponent component)
        {
            try
            {
                ((Resistor)component).Resistance = double.Parse(tbxResistance.Text);
            }
            catch (Exception)
            {
                // incorrect input format
            }
        }
    }
}
