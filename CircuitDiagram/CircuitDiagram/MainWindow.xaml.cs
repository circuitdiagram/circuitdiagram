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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

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

        public static bool m_moveComponent = false;
        Point m_moveComponentStartPos;
        Point m_moveComponentEndPos;
        private void btnMoveComponent_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = true;
            newComponentType = null;
        }

        Point mouseDownPos;
        Type newComponentType;
        ComponentResizeMode m_resizing = ComponentResizeMode.None;
        private void circuitDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPos = e.GetPosition((IInputElement)sender);
            if (m_document.SelectedComponent != null && m_moveComponent)
            {
                if (m_document.SelectedComponent.Horizontal)
                {
                    Rect leftHandle = new Rect(m_document.SelectedComponent.BoundingBox.X - 3, m_document.SelectedComponent.BoundingBox.Y +
                        m_document.SelectedComponent.BoundingBox.Height / 2 - 3f, 6, 6);
                    Rect rightHandle = new Rect(m_document.SelectedComponent.BoundingBox.Right - 3, m_document.SelectedComponent.BoundingBox.Y + m_document.SelectedComponent.BoundingBox.Height / 2 - 3f, 6f, 6f);
                    if (leftHandle.IntersectsWith(new Rect(mouseDownPos.X, mouseDownPos.Y, 1, 1)))
                        m_resizing = ComponentResizeMode.Left;
                    else if (rightHandle.IntersectsWith(new Rect(mouseDownPos.X, mouseDownPos.Y, 1, 1)))
                        m_resizing = ComponentResizeMode.Right;
                }
                else
                {
                    Rect topHandle = new Rect(m_document.SelectedComponent.BoundingBox.X + m_document.SelectedComponent.BoundingBox.Width / 2 - 3f, m_document.SelectedComponent.BoundingBox.Y - 3f, 6f, 6f);
                    Rect bottomHandle = new Rect(m_document.SelectedComponent.BoundingBox.X + m_document.SelectedComponent.BoundingBox.Width / 2 - 3f, m_document.SelectedComponent.BoundingBox.Y + m_document.SelectedComponent.BoundingBox.Height - 3f, 6f, 6f);
                    if (topHandle.IntersectsWith(new Rect(mouseDownPos.X, mouseDownPos.Y, 1, 1)))
                        m_resizing = ComponentResizeMode.Top;
                    else if (bottomHandle.IntersectsWith(new Rect(mouseDownPos.X, mouseDownPos.Y, 1, 1)))
                        m_resizing = ComponentResizeMode.Bottom;
                }
            }
            else
                m_resizing = ComponentResizeMode.None;
        }

        private void circuitDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_resizing != ComponentResizeMode.None)
            {
                m_resizing = ComponentResizeMode.None;
                return;
            }
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
            circuitDisplay.InvalidateVisual();
        }

        bool cancelSelect = true;
        private void circuitDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && m_resizing != ComponentResizeMode.None)
            {
                if (m_resizing == ComponentResizeMode.Left)
                    m_document.SelectedComponent.StartLocation = new Point(e.GetPosition(circuitDisplay).X, m_document.SelectedComponent.StartLocation.Y);
                else if (m_resizing == ComponentResizeMode.Right)
                    m_document.SelectedComponent.EndLocation = new Point(e.GetPosition(circuitDisplay).X, m_document.SelectedComponent.StartLocation.Y);
                else if (m_resizing == ComponentResizeMode.Top)
                    m_document.SelectedComponent.StartLocation = new Point(m_document.SelectedComponent.StartLocation.X, e.GetPosition(circuitDisplay).Y);
                else if (m_resizing == ComponentResizeMode.Bottom)
                    m_document.SelectedComponent.EndLocation = new Point(m_document.SelectedComponent.StartLocation.X, e.GetPosition(circuitDisplay).Y);
                m_document.UpdateLayout(m_document.SelectedComponent);
                circuitDisplay.InvalidateVisual();
            }
            else if (m_resizing != ComponentResizeMode.None)
            {
            }
            else if (e.LeftButton == MouseButtonState.Pressed && m_moveComponent && m_document.SelectedComponent != null)
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

            if (m_document.SelectedComponent != null && m_moveComponent)
            {
                Rect mouseRect = new Rect(e.GetPosition(circuitDisplay).X, e.GetPosition(circuitDisplay).Y, 1, 1);
                if (m_document.SelectedComponent.Horizontal)
                {
                    Rect leftHandle = new Rect(m_document.SelectedComponent.BoundingBox.X - 3, m_document.SelectedComponent.BoundingBox.Y +
                        m_document.SelectedComponent.BoundingBox.Height / 2 - 3f, 6, 6);
                    Rect rightHandle = new Rect(m_document.SelectedComponent.BoundingBox.Right - 3, m_document.SelectedComponent.BoundingBox.Y + m_document.SelectedComponent.BoundingBox.Height / 2 - 3f, 6f, 6f);
                    if (m_resizing != ComponentResizeMode.None || leftHandle.IntersectsWith(mouseRect) || rightHandle.IntersectsWith(mouseRect))
                        circuitDisplay.Cursor = Cursors.SizeWE;
                    else
                        circuitDisplay.Cursor = Cursors.Arrow;
                }
                else
                {
                    Rect topHandle = new Rect(m_document.SelectedComponent.BoundingBox.X + m_document.SelectedComponent.BoundingBox.Width / 2 - 3f, m_document.SelectedComponent.BoundingBox.Y - 3f, 6f, 6f);
                    Rect bottomHandle = new Rect(m_document.SelectedComponent.BoundingBox.X + m_document.SelectedComponent.BoundingBox.Width / 2 - 3f, m_document.SelectedComponent.BoundingBox.Y + m_document.SelectedComponent.BoundingBox.Height - 3f, 6f, 6f);
                    if (m_resizing != ComponentResizeMode.None || topHandle.IntersectsWith(mouseRect) || bottomHandle.IntersectsWith(mouseRect))
                        circuitDisplay.Cursor = Cursors.SizeNS;
                    else
                        circuitDisplay.Cursor = Cursors.Arrow;
                }
            }
            else
            {
                circuitDisplay.Cursor = Cursors.Arrow;
            }
        }

        private void circuitDisplay_MouseLeave(object sender, MouseEventArgs e)
        {
            m_document.TempComponents.Clear();
            m_resizing = ComponentResizeMode.None;
            //m_document.SelectedComponent = null;
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

        #region New Component Type Selection
        private void btnComponentsWire_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Wire);
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
        #endregion

        #region Menu Options
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
                m_document.UpdateLayout(null);
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
        #endregion

        private void btnComponentsMosfet_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Mosfet);
        }

        private void btnComponentsLamp_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(Lamp);
        }

        private void btnComponentsExternalConnection_Click(object sender, RoutedEventArgs e)
        {
            m_moveComponent = false;
            newComponentType = typeof(ExternalConnection);
        }

        private void mnuHelpDocumentation_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://circuitdiagram.codeplex.com/documentation");
        }
    }

    enum ComponentResizeMode
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }
}