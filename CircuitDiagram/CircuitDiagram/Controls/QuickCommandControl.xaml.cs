//using CircuitDiagram.Components;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace CircuitDiagram.Controls
//{
//    /// <summary>
//    /// Interaction logic for QuickCommandControl.xaml
//    /// </summary>
//    public partial class QuickCommandControl : Popup
//    {
//        public event Action<object, EventArgs, string> ComponentSelected;

//        public QuickCommandControl()
//        {
//            InitializeComponent();
//        }

//        protected override void OnOpened(EventArgs e)
//        {
//            base.OnOpened(e);
//            tbxMain.Text = "";
//            tbxMain.Focus();
//        }

//        private void tbxMain_KeyDown(object sender, KeyEventArgs e)
//        {
//            if (e.Key == Key.Enter)
//            {
//                IdentifierWithShortcut selected = FindComponent();

//                if (selected == null)
//                    return;

//                this.IsOpen = false;
//                e.Handled = true;

//                if (ComponentSelected != null)
//                    ComponentSelected(this, new EventArgs(), selected.ToString());
//            }
//            else if (e.Key == Key.Escape)
//            {
//                this.IsOpen = false;
//                e.Handled = true;
//            }
//        }

//        private ExtendedIdentifierWithShortcut FindComponent()
//        {
//            ExtendedIdentifierWithShortcut selected = null;

//            if (tbxMain.Text.Trim() == "")
//                return selected;

//            // Find configuration
//            foreach (var component in ComponentHelper.ComponentDescriptions)
//            {
//                if (component.ComponentName.IndexOf(tbxMain.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
//                {
//                    selected = new ExtendedIdentifierWithShortcut() { Identifier = new ComponentIdentifier(component) };
//                    break;
//                }

//                var config = component.Metadata.Configurations.FirstOrDefault(cfg =>
//                    cfg.Name.IndexOf(tbxMain.Text, StringComparison.InvariantCultureIgnoreCase) >= 0);
//                if (config != null)
//                {
//                    selected = new ExtendedIdentifierWithShortcut() { Identifier = new ComponentIdentifier(component, config) };
//                    break;
//                }
//            }

//            return selected;
//        }

//        private void tbxMain_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            var selected = FindComponent();
//            this.DataContext = selected;
//        }

//        class ExtendedIdentifierWithShortcut : IdentifierWithShortcut
//        {
//            public bool NoConfiguration { get { return Identifier.Configuration == null; } }
//            public bool HasConfiguration { get { return Identifier.Configuration != null; } }
//        }
//    }
//}
