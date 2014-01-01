// ComponentEditor.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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

using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CircuitDiagram.Components
{
    public class ComponentEditor : ContentControl, IComponentEditor
    {
        public CircuitDocument Document { get; set; }
        public event ComponentUpdatedDelegate ComponentUpdated;

        public Dictionary<ComponentProperty, object> EditorControls { get; private set; }

        Component Component { get; set; }

        private string m_previousData;

        public string GetComponentData()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            Component.Serialize(properties);
            return ComponentDataString.ConvertToString(properties);
        }

        public ComponentEditor(Component component)
        {
            EditorControls = new Dictionary<ComponentProperty, object>();

            ComponentUpdated += new ComponentUpdatedDelegate(ComponentEditor_ComponentUpdated);

            Component = component;

            ComponentProperty[] propertyInfo = component.Description.Properties;

            this.DataContext = component;

            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            int i = 0;
            foreach (ComponentProperty info in component.Description.Properties)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition() { Height = System.Windows.GridLength.Auto });

                if (info.Type == typeof(bool))
                {
                    CheckBox checkbox = new CheckBox();
                    checkbox.Content = info.DisplayName;
                    checkbox.IsChecked = (bool)component.GetProperty(info);
                    checkbox.Margin = new Thickness(5d);
                    checkbox.Tag = info;

                    checkbox.SetValue(Grid.RowProperty, i);
                    checkbox.SetValue(Grid.ColumnProperty, 0);
                    checkbox.SetValue(Grid.ColumnSpanProperty, 2);

                    checkbox.Checked += new System.Windows.RoutedEventHandler(BoolChanged);
                    checkbox.Unchecked += new System.Windows.RoutedEventHandler(BoolChanged);

                    mainGrid.Children.Add(checkbox);
                    EditorControls.Add(info, checkbox);
                }
                else if (info.Type == typeof(double))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    textbox.Tag = info;
                    textbox.Text = component.GetProperty(info).ToString();
                    textbox.Margin = new Thickness(5d);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    textbox.TextChanged += new TextChangedEventHandler(DoubleChanged);

                    mainGrid.Children.Add(textbox);
                    EditorControls.Add(info, textbox);
                }
                else if (info.Type == typeof(int))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    textbox.Margin = new Thickness(5d);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    textbox.TextChanged += new TextChangedEventHandler(DoubleChanged);

                    mainGrid.Children.Add(textbox);
                    EditorControls.Add(info, textbox);
                }
                else if (info.Type == typeof(string) && info.EnumOptions != null && info.EnumOptions.Length > 0)
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    ComboBox combobox = new ComboBox();
                    combobox.Margin = new Thickness(5d);
                    combobox.Tag = info;
                    foreach (string option in info.EnumOptions)
                        combobox.Items.Add(option);
                    combobox.SelectedItem = component.GetProperty(info);

                    combobox.SetValue(Grid.RowProperty, i);
                    combobox.SetValue(Grid.ColumnProperty, 1);

                    combobox.SelectionChanged += new SelectionChangedEventHandler(EnumChanged);

                    mainGrid.Children.Add(combobox);
                    EditorControls.Add(info, combobox);
                }
                else if (info.Type == typeof(string))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    textbox.Margin = new Thickness(5d);
                    textbox.Tag = info;
                    textbox.Text = component.GetProperty(info) as string;
                    textbox.TextChanged += new TextChangedEventHandler(StringChanged);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    mainGrid.Children.Add(textbox);
                    EditorControls.Add(info, textbox);
                }

                i++;
            }

            this.Content = mainGrid;

            this.Loaded += new RoutedEventHandler(AutomaticEditor_Loaded); // Set previous component data
        }

        void StringChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            Component.SetProperty(((sender as Control).Tag as ComponentProperty), (sender as TextBox).Text);
            Component.InvalidateVisual();
            ComponentUpdated(this, new ComponentUpdatedEventArgs(Component, m_previousData));
            m_previousData = GetComponentData();
        }

        void ComponentEditor_ComponentUpdated(object sender, ComponentUpdatedEventArgs e)
        {
            // Do nothing
        }

        void AutomaticEditor_Loaded(object sender, RoutedEventArgs e)
        {
            m_previousData = GetComponentData();
        }

        void DoubleChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            double doubleValue;
            if (double.TryParse((sender as TextBox).Text, out doubleValue))
            {
                Component.SetProperty(((sender as Control).Tag as ComponentProperty), doubleValue);
                Component.InvalidateVisual();
                ComponentUpdated(this, new ComponentUpdatedEventArgs(Component, m_previousData));
                m_previousData = GetComponentData();
            }
        }

        void BoolChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            Component.SetProperty(((sender as Control).Tag as ComponentProperty), (sender as CheckBox).IsChecked.Value);
            Component.InvalidateVisual();
            ComponentUpdated(this, new ComponentUpdatedEventArgs(Component, m_previousData));
            m_previousData = GetComponentData();
        }

        void EnumChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            Component.SetProperty(((sender as Control).Tag as ComponentProperty), (sender as ComboBox).SelectedItem);
            Component.InvalidateVisual();
            ComponentUpdated(this, new ComponentUpdatedEventArgs(Component, m_previousData));
            m_previousData = GetComponentData();
        }

        public void Update()
        {
            // Update editor values for properties
            foreach (var control in EditorControls)
            {
                if (control.Value.GetType() == typeof(CheckBox))
                    (control.Value as CheckBox).IsChecked = (bool)Component.GetProperty(control.Key);
                else if (control.Value.GetType() == typeof(TextBox))
                    (control.Value as TextBox).Text = Component.GetProperty(control.Key).ToString();
                else if (control.Value.GetType() == typeof(ComboBox))
                    (control.Value as ComboBox).SelectedItem = Component.GetProperty(control.Key).ToString();
            }
        }
    }

    static class ComponentEditorHelper
    {
        public static IComponentEditor CreateEditor(Component component)
        {
            return new ComponentEditor(component);
        }
    }
}