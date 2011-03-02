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
using CircuitDiagram.EComponents;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CircuitDocument m_document;
        string m_docPath = "";

        public MainWindow()
        {
            InitializeComponent();

            // Insert code required on object creation below this point.
            m_document = circuitDisplay.Document;
            this.Title = "Untitled - Circuit Diagram";
        }

        private void btnComponentsWire_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Wire);
        }

        bool m_moveComponent = false;
        Point m_moveComponentStartPos;
        Point m_moveComponentEndPos;
        private void btnMoveComponent_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = true;
            newComponentType = null;
        }

        Point mouseDownPos;
        Type newComponentType;
        private void circuitDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPos = e.GetPosition((IInputElement)sender);
        }

        private void circuitDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_moveComponent)
            {
                m_document.SelectedComponent = null;
                return;
            }
            if (newComponentType == null)
                return;
            m_document.TempComponents.Clear();
            Point mouseUpPos = e.GetPosition((IInputElement)sender);
            EComponent newComponent = (EComponent)Activator.CreateInstance(newComponentType);
            newComponent.StartLocation = mouseDownPos;
            newComponent.EndLocation = mouseUpPos;
            m_document.Components.Add(newComponent);
            m_document.UpdateLayout(newComponent);
        }

        bool cancelSelect = true;
        private void circuitDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && m_moveComponent && m_document.SelectedComponent != null)
            {
                m_document.SelectedComponent.StartLocation = Point.Add(m_moveComponentStartPos, new Vector(-mouseDownPos.X + e.GetPosition((IInputElement)sender).X, -mouseDownPos.Y + e.GetPosition((IInputElement)sender).Y));
                m_document.SelectedComponent.EndLocation = Point.Add(m_moveComponentEndPos, new Vector(-mouseDownPos.X + e.GetPosition((IInputElement)sender).X, -mouseDownPos.Y + e.GetPosition((IInputElement)sender).Y));
                m_document.UpdateLayout(m_document.SelectedComponent);
                m_document.InvalidateVisual();
                circuitDisplay.InvalidateVisual();
            }
            else if (e.LeftButton == MouseButtonState.Pressed && newComponentType != null)
            {
                m_document.TempComponents.Clear();
                Point mouseUpPos = e.GetPosition((IInputElement)sender);
                EComponent newComponent = (EComponent)Activator.CreateInstance(newComponentType);
                EComponent.EditWindowParent = this;
                newComponent.StartLocation = mouseDownPos;
                newComponent.EndLocation = mouseUpPos;
                m_document.TempComponents.Add(newComponent);
                m_document.UpdateLayout(newComponent);
            }
            else
            {
                if (!cancelSelect)
                    return;
                m_document.SelectedComponent = null;
                foreach (EComponent component in m_document.Components)
                {
                    if (component.Intersects(e.GetPosition((IInputElement)sender)) ||
                        component.BoundingBox.IntersectsWith(new Rect(e.GetPosition((IInputElement)sender).X, e.GetPosition((IInputElement)sender).Y, 1, 1)))
                    {
                        m_document.SelectedComponent = component;
                        m_moveComponentStartPos = component.StartLocation;
                        m_moveComponentEndPos = component.EndLocation;
                    }
                }
            }
        }

        private void btnComponentsResistor_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Resistor);
        }

        private void btnComponentsSupply_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Supply);
        }

        private void mnuExportSVG_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export";
            sfd.Filter = "PNG (*.png)|*.png|Scalable Vector Graphics (*.svg)|*.svg";
            sfd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string extension = System.IO.Path.GetExtension(sfd.FileName);
                if (extension == ".svg")
                {
                    SVGLibrary.SVGDocument exportDoc = new SVGLibrary.SVGDocument();
                    SVGRenderer renderer = new SVGRenderer();
                    renderer.SVGDocument = exportDoc;
                    m_document.Render(renderer);
                    exportDoc.Save(sfd.FileName);
                }
                else if (extension == ".png")
                {
                    //RenderTargetBitmap bmp = new RenderTargetBitmap((int)circuitDisplay.Width, (int)circuitDisplay.Height, 96d, 96d, PixelFormats.Default);
                    //bmp.Render(circuitDisplay);
                    PrintRenderer pRenderer = new PrintRenderer(circuitDisplay.Width, circuitDisplay.Height);
                    pRenderer.DrawRectangle(Colors.White, Colors.White, 0.0f, new Rect(0, 0, circuitDisplay.Width, circuitDisplay.Height));
                    m_document.Render(pRenderer);
                    RenderTargetBitmap bmp = pRenderer.GetImage();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    System.IO.FileStream stream = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create);
                    encoder.Save(stream);
                    stream.Close();
                }
            }
        }

        private void circuitDisplay_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            cancelSelect = false;
        }

        private void circuitDisplay_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            cancelSelect = true;
        }

        private void CommandEditComponent_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (m_document.SelectedComponent != null);
        }

        private void CommandEditComponent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_document.SelectedComponent.ShowEditor();
        }

        private void CommandDeleteComponent_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (m_document.SelectedComponent != null);
        }

        private void CommandDeleteComponent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_document.Components.Remove(m_document.SelectedComponent);
            m_document.SelectedComponent = null;
            m_document.InvalidateVisual();
        }

        private void CommandNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void btnComponentRail_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Rail);
        }

        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnComponentsSwitch_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Switch);
        }

        private void btnComponentsLogicGate_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(LogicGate);
        }

        private void btnComponentsCounter_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Counter);
        }

        private void btnComponentsSegDecoder_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(SegDecoder);
        }

        private void btnComponentsSegDisplay_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(SegDisplay);
        }

        private void btnComponentsCapacitor_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Capacitor);
        }


        private void btnComponentsLED_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(LED);
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save As";
            sfd.Filter = "XML Files (*.xml)|*.xml";
            sfd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_document.Save(sfd.FileName, circuitDisplay.Width, circuitDisplay.Height);
                m_docPath = sfd.FileName;
                this.Title = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName) + " - Circuit Diagram";
            }
        }

        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            NewDocument nDocWin = new NewDocument();
            nDocWin.TbxWidth = "640";
            nDocWin.TbxHeight = "480";
            nDocWin.Owner = this;
            if (nDocWin.ShowDialog() == true)
            {
                m_document.Components.Clear();
                m_docPath = "";
                circuitDisplay.Width = double.Parse(nDocWin.TbxWidth);
                circuitDisplay.Height = double.Parse(nDocWin.TbxHeight);
                this.Title = "Untitled - Circuit Diagram";
            }
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pDlg = new PrintDialog();
            if (pDlg.ShowDialog() == true)
            {
                PrintRenderer printRenderer = new PrintRenderer(circuitDisplay.Width, circuitDisplay.Height);
                m_document.Render(printRenderer);
                FixedDocument document = printRenderer.GetDocument(new Size(pDlg.PrintableAreaWidth, pDlg.PrintableAreaHeight));
                pDlg.PrintDocument(document.DocumentPaginator, "Circuit Diagram");
            }
        }

        private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_docPath != "")
                m_document.Save(m_docPath, circuitDisplay.Width, circuitDisplay.Height);
            else
                mnuSaveAs_Click(sender, e);
        }

        private void CommandOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "Open";
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                double displayWidth;
                double displayHeight;
                m_document.Load(ofd.FileName, out displayWidth, out displayHeight);
                circuitDisplay.Width = displayWidth;
                circuitDisplay.Height = displayHeight;
                m_docPath = ofd.FileName;
                this.Title = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName) + " - Circuit Diagram";
                m_document.InvalidateVisual();
                circuitDisplay.InvalidateVisual();
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            winAbout aboutWindow = new winAbout();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        private void circuitDisplay_MouseLeave(object sender, MouseEventArgs e)
        {
            m_document.TempComponents.Clear();
        }

        private void btnComponentsDiode_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Diode);
        }

        private void btnComponentsOpAmp_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(OpAmp);
        }
    }
}