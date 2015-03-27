using CircuitDiagram.DPIWindow;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winExportPNG.xaml
    /// </summary>
    public partial class winExportPNG : MetroDPIWindow
    {
        public winExportPNG()
        {
            InitializeComponent();
        }

        public void Update()
        {
            OutputWidth = OriginalWidth;
            OutputHeight = OriginalHeight;
        }

        public double OriginalWidth { get; set; }
        public double OriginalHeight { get; set; }

        private double m_outputWidth;
        public double OutputWidth
        {
            get { return m_outputWidth; }
            set
            {
                m_outputWidth = value;
                lblOutputWidth.Content = m_outputWidth;
            }
        }

        private double m_outputHeight;
        public double OutputHeight
        {
            get { return m_outputHeight; }
            set
            {
                m_outputHeight = value;
                lblOutputHeight.Content = m_outputHeight;
            }
        }

        public string OutputBackgroundColour
        {
            get { return (cbxBackgroundColour.SelectedIndex == 0 ? "White" : "Transparent"); }
        }

        private void tbxCustomWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                OutputWidth = Math.Round(double.Parse(tbxCustomWidth.Text));
                if (OutputWidth < 200)
                    OutputWidth = 200;
                if (OutputWidth > 5000)
                    OutputWidth = 5000;
                double aspect = OriginalWidth / OriginalHeight;
                OutputHeight = Math.Round(OutputWidth * (1 / aspect));
                lblOutputDpiX.Content = Math.Round((OutputWidth / OriginalWidth) * 96d);
                lblOutputDpiY.Content = Math.Round((OutputHeight / OriginalHeight) * 96d);
            }
            catch (Exception)
            {
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
