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
    public partial class LogicGateEditor : ComponentEditor
    {
        public LogicGateEditor()
        {
            InitializeComponent();
        }

        public override void LoadComponent(EComponent component)
        {
            LogicGate lGate = (LogicGate)component;
            radLogicAND.IsChecked = false;
            radLogicOR.IsChecked = false;
            radLogicNAND.IsChecked = false;
            radLogicNOR.IsChecked = false;
            radLogicNOT.IsChecked = false;
            switch (lGate.LogicType)
            {
                case LogicType.AND:
                    radLogicAND.IsChecked = true;
                    break;
                case LogicType.OR:
                    radLogicOR.IsChecked = true;
                    break;
                case LogicType.NAND:
                    radLogicNAND.IsChecked = true;
                    break;
                case LogicType.NOR:
                    radLogicNOR.IsChecked = true;
                    break;
                case LogicType.NOT:
                    radLogicNOT.IsChecked = true;
                    break;
            }
        }

        public override void UpdateChanges(EComponent component)
        {
            LogicGate lGate = (LogicGate)component;
            if (radLogicAND.IsChecked == true)
                lGate.LogicType = LogicType.AND;
            else if (radLogicOR.IsChecked == true)
                lGate.LogicType = LogicType.OR;
            else if (radLogicNAND.IsChecked == true)
                lGate.LogicType = LogicType.NAND;
            else if (radLogicNOR.IsChecked == true)
                lGate.LogicType = LogicType.NOR;
            else if (radLogicNOT.IsChecked == true)
                lGate.LogicType = LogicType.NOT;
        }
    }
}
