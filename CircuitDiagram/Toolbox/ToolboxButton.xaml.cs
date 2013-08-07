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
using System.Windows.Threading;

namespace Toolbox
{
    /// <summary>
    /// Interaction logic for ToolboxButton.xaml
    /// </summary>
    public partial class ToolboxButton : Button
    {
        public double DisplayIndex { get; set; }

        DispatcherTimer m_timer = new DispatcherTimer();

        MouseButtonEventArgs m_tempArgs;
        public event MouseButtonEventHandler LongPress;

        public ToolboxItemContainer ToolboxItemContainer
        {
            get;
            set;
        }

        public ToolboxButton()
        {
            InitializeComponent();
            this.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(LongPressButton_MouseDown), true);
            this.AddHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(LongPressButton_MouseUp), true);

            m_timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            m_timer.Tick += new EventHandler(m_timer_Tick);
            LongPress += new MouseButtonEventHandler(ToolboxButton_LongPress);
        }

        void ToolboxButton_LongPress(object sender, MouseButtonEventArgs e)
        {
            ((ToolboxItemContainer)this.Parent).ShowOverflow();
        }

        bool m_cancelMouseUp = false;
        void m_timer_Tick(object sender, EventArgs e)
        {
            // long press event
            m_timer.Stop();
            m_cancelMouseUp = true;
            if (LongPress != null)
                LongPress(this, m_tempArgs);
        }

        void LongPressButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_timer.Stop();
            if (m_cancelMouseUp)
            {
                m_cancelMouseUp = false;
                e.Handled = true;
            }
        }

        void LongPressButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_tempArgs = e;
            m_timer.Start();
        }
    }
}
