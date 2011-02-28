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

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for CircuitDisplay.xaml
    /// </summary>
    public partial class CircuitDisplay : UserControl
    {
        private CircuitDocument m_document;

        internal CircuitDocument Document
        {
            get { return m_document; }
            set
            {
                m_document = value;
                m_document.VisualInvalidated += new EventHandler(m_document_VisualInvalidated);
            }
        }

        void m_document_VisualInvalidated(object sender, EventArgs e)
        {
            this.InvalidateVisual();
        }

        public CircuitDisplay()
        {
            InitializeComponent();
            vRenderer = new VisualRenderer();
            Document = new CircuitDocument();
        }

        VisualRenderer vRenderer;
        protected override void OnRender(DrawingContext drawingContext)
        {
            vRenderer.DrawingContext = drawingContext;
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(0, 0, this.Width, this.Height));
            m_document.Render(vRenderer);
        }
    }
}
