using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CircuitDiagram.EComponents
{
    class AutomaticEditor : ComponentEditorBase
    {
        EComponent Component { get; set; }

        private string m_previousData;

        public string GetComponentData()
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            Component.Serialize(properties);
            return ComponentStringDescription.ConvertToString(properties);
        }

        public AutomaticEditor(EComponent component)
        {
            Component = component;

            List<ComponentPropertyInfo> propertyInfo = component.SerializableProperties();

            this.DataContext = component;

            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            int i = 0;
            foreach (ComponentPropertyInfo info in propertyInfo)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition() { Height = System.Windows.GridLength.Auto });

                if (info.PropertyInfo.PropertyType == typeof(bool))
                {
                    CheckBox checkbox = new CheckBox();
                    checkbox.Content = info.DisplayName;
                    Binding binding = new Binding();
                    binding.Path = new System.Windows.PropertyPath(info.PropertyInfo.Name);
                    binding.Mode = BindingMode.TwoWay;
                    checkbox.SetBinding(CheckBox.IsCheckedProperty, binding);
                    checkbox.Margin = new Thickness(5d);

                    checkbox.SetValue(Grid.RowProperty, i);
                    checkbox.SetValue(Grid.ColumnProperty, 1);

                    if (info.DisplayAlignLeft)
                    {
                        checkbox.SetValue(Grid.ColumnProperty, 0);
                        checkbox.SetValue(Grid.ColumnSpanProperty, 2);
                        checkbox.Margin = new Thickness(7d, 5d, 5d, 5d);
                    }

                    checkbox.Checked += new System.Windows.RoutedEventHandler(PropertyUpdated);
                    checkbox.Unchecked += new System.Windows.RoutedEventHandler(PropertyUpdated);

                    mainGrid.Children.Add(checkbox);
                }
                else if (info.PropertyInfo.PropertyType == typeof(double))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    Binding binding = new Binding();
                    binding.Path = new System.Windows.PropertyPath(info.PropertyInfo.Name);
                    binding.Mode = BindingMode.TwoWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    binding.Converter = new DoubleToStringConverter();
                    textbox.SetBinding(TextBox.TextProperty, binding);
                    textbox.Margin = new Thickness(5d);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    textbox.TextChanged += new TextChangedEventHandler(PropertyUpdated);

                    mainGrid.Children.Add(textbox);
                }
                else if (info.PropertyInfo.PropertyType == typeof(int))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Margin = new Thickness(5d);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    Binding binding = new Binding();
                    binding.Path = new System.Windows.PropertyPath(info.PropertyInfo.Name);
                    binding.Mode = BindingMode.TwoWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    binding.Converter = new IntToStringConverter();
                    textbox.SetBinding(TextBox.TextProperty, binding);
                    textbox.Margin = new Thickness(5d);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    textbox.TextChanged += new TextChangedEventHandler(PropertyUpdated);

                    mainGrid.Children.Add(textbox);
                }
                else if (info.PropertyInfo.PropertyType == typeof(string))
                {
                    Label label = new Label();
                    label.Content = info.DisplayName;
                    label.SetValue(Grid.RowProperty, i);
                    label.SetValue(Grid.ColumnProperty, 0);

                    mainGrid.Children.Add(label);

                    TextBox textbox = new TextBox();
                    Binding binding = new Binding();
                    binding.Path = new System.Windows.PropertyPath(info.PropertyInfo.Name);
                    binding.Mode = BindingMode.TwoWay;
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    textbox.SetBinding(TextBox.TextProperty, binding);

                    textbox.SetValue(Grid.RowProperty, i);
                    textbox.SetValue(Grid.ColumnProperty, 1);

                    textbox.TextChanged += new TextChangedEventHandler(PropertyUpdated);

                    mainGrid.Children.Add(textbox);
                }

                i++;
            }

            this.Content = mainGrid;

            this.Loaded += new RoutedEventHandler(AutomaticEditor_Loaded); // Set previous component data
        }

        void AutomaticEditor_Loaded(object sender, RoutedEventArgs e)
        {
            m_previousData = GetComponentData();
        }

        void PropertyUpdated(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;
            base.CallComponentUpdated(Component, m_previousData);
            m_previousData = GetComponentData();
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double parsed = 0;
            double.TryParse(value.ToString(), out parsed);
            parsed = Math.Min(parsed, 1000000);
            return parsed;
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int parsed = 0;
            int.TryParse(value.ToString(), out parsed);
            parsed = Math.Min(parsed, 20);
            return parsed;
        }
    }
}
