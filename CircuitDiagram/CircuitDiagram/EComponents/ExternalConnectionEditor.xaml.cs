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
    /// Interaction logic for ExternalConnectionEditor.xaml
    /// </summary>
    public partial class ExternalConnectionEditor : ComponentEditor
    {
        public ExternalConnectionEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
             tbxConnectionText.Text = ((ExternalConnection)component).ConnectionText;
             chbTopOrLeft.IsChecked = ((ExternalConnection)component).ConnectionTopLeft;
        }

        public override void UpdateChanges(EComponent component)
        {
            ((ExternalConnection)component).ConnectionText = tbxConnectionText.Text;
            ((ExternalConnection)component).ConnectionTopLeft = chbTopOrLeft.IsChecked.Value;
        }
    }
}
