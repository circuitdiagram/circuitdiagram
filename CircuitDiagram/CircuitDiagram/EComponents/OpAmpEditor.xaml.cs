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
    /// Interaction logic for OpAmpEditor.xaml
    /// </summary>
    public partial class OpAmpEditor : ComponentEditor
    {
        public OpAmpEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
            chbFlipPN.IsChecked = ((OpAmp)component).FlipInputs;
        }

        public override void UpdateChanges(EComponent component)
        {
            ((OpAmp)component).FlipInputs = chbFlipPN.IsChecked.Value;
        }
    }
}
