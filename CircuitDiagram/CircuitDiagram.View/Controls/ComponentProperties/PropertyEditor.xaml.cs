// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using CircuitDiagram.View.Controls.ComponentProperties;
using CircuitDiagram.View.Editor;

namespace CircuitDiagram.View.Controls
{
    /// <summary>
    /// Interaction logic for PropertyEditor.xaml
    /// </summary>
    public partial class PropertyEditor : UserControl
    {
        public static readonly DependencyProperty EditableComponentsProperty = DependencyProperty.Register(
            "EditableComponents", typeof(ObservableCollection<PositionalComponent>), typeof(PropertyEditor), new PropertyMetadata(default(ObservableCollection<PositionalComponent>), EditableComponentsChanged));
        
        public static readonly DependencyProperty DescriptionLookupProperty = DependencyProperty.Register(
            "DescriptionLookup", typeof(IComponentDescriptionLookup), typeof(PropertyEditor), new PropertyMetadata(default(IComponentDescriptionLookup)));

        public static readonly DependencyProperty ComponentPropertyChangedProperty = DependencyProperty.Register(
            "ComponentPropertyChanged", typeof(Action<PositionalComponent>), typeof(PropertyEditor), new PropertyMetadata(default(Action<PositionalComponent>)));

        public Action<PositionalComponent> ComponentPropertyChanged
        {
            get { return (Action<PositionalComponent>)GetValue(ComponentPropertyChangedProperty); }
            set { SetValue(ComponentPropertyChangedProperty, value); }
        }

        public PropertyEditor()
        {
            InitializeComponent();
            Properties = new ObservableCollection<EditableProperty>();
        }

        public IComponentDescriptionLookup DescriptionLookup
        {
            get { return (IComponentDescriptionLookup)GetValue(DescriptionLookupProperty); }
            set { SetValue(DescriptionLookupProperty, value); }
        }

        public ObservableCollection<PositionalComponent> EditableComponents
        {
            get { return (ObservableCollection<PositionalComponent>)GetValue(EditableComponentsProperty); }
            set { SetValue(EditableComponentsProperty, value); }
        }

        public ObservableCollection<EditableProperty> Properties { get; private set; }

        private static void EditableComponentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (PropertyEditor)d;
            ((ObservableCollection<PositionalComponent>)e.NewValue).CollectionChanged += (sender, args) => editor.Update();
        }

        private void Update()
        {
            Properties.Clear();

            if (EditableComponents.Count != 1)
                return;

            var description = DescriptionLookup.GetDescription(EditableComponents[0].Type);

            foreach (var property in EditableComponents[0].Properties)
            {
                Properties.Add(new EditableProperty
                {
                    Component = EditableComponents[0],
                    Property = description.Properties.FirstOrDefault(x => x.SerializedName == property.Key),
                    ComponentDescription = description,
                    ValueChanged = () => ComponentPropertyChanged(EditableComponents[0])
                });
            }
        }
    }
}
