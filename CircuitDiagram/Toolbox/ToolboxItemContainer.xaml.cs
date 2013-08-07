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

namespace Toolbox
{
    /// <summary>
    /// Interaction logic for ToolBoxItemContainer.xaml
    /// </summary>
    public partial class ToolboxItemContainer : Canvas
    {
        private bool m_hasSetDisplayIndex = false;

        public ToolboxOverflow Overflow
        {
            get { return this.Children.OfType<ToolboxOverflow>().First(); }
        }

        public ToolboxItemContainer()
        {
            InitializeComponent();
            this.AddHandler(FrameworkElement.MouseLeaveEvent, new MouseEventHandler(ToolboxItemContainer_MouseLeave));
        }

        public void ShowOverflow()
        {
            if (!m_hasSetDisplayIndex)
            {
                this.Children.OfType<ToolboxButton>().First().DisplayIndex = 0;
                this.Children.OfType<ToolboxButton>().First().ToolboxItemContainer = this;
                for (int i = 0; i < Overflow.Items.Count; i++)
                {
                    ((ToolboxButton)Overflow.Items[i]).DisplayIndex = i + 1;
                    ((ToolboxButton)Overflow.Items[i]).ToolboxItemContainer = this;
                }
            }
            Overflow.Visibility = System.Windows.Visibility.Visible;
        }

        public void HideOverflow()
        {
            Overflow.Visibility = System.Windows.Visibility.Hidden;
        }

        void ToolboxItemContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            HideOverflow();
        }

        public void SetNonOverflowButton(ToolboxButton button)
        {
            ToolboxButton currentNonOverflowButton = this.Children.OfType<ToolboxButton>().First();
            if (currentNonOverflowButton != button)
            {
                List<ToolboxButton> allButtons = new List<ToolboxButton>();
                allButtons.Add(currentNonOverflowButton);
                foreach (ToolboxButton item in Overflow.Items)
                    allButtons.Add(item);
                allButtons.Remove(button);
                allButtons.Sort(delegate(ToolboxButton one, ToolboxButton two) { return one.DisplayIndex.CompareTo(two.DisplayIndex); });
                Overflow.Items.Clear();
                allButtons.ForEach(delegate(ToolboxButton item) { Overflow.Items.Add(item); });
            }
        }
    }
}
