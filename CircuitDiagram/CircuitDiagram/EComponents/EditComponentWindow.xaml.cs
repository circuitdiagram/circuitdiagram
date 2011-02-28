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
using System.Windows.Shapes;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for EditComponentWindow.xaml
    /// </summary>
    public partial class EditComponentWindow : Window
    {
        public bool CustomResult = false;

        public EditComponentWindow()
        {
            InitializeComponent();
        }

        public void SetEditor(ComponentEditor editor)
        {
            this.BaseGrid.Children.Clear();
            this.BaseGrid.Children.Add(editor);
            this.Title = editor.Title;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.CustomResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.CustomResult = false;
            this.Close();
        }
    }
}
