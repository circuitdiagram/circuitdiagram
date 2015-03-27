using CircuitDiagram.DPIWindow;
using CircuitDiagram.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CircuitDiagram
{
    /// <summary>
    /// Interaction logic for winDocumentLoadResult.xaml
    /// </summary>
    public partial class winDocumentLoadResult : MetroDPIWindow
    {
        public winDocumentLoadResult()
        {
            InitializeComponent();

            lbxErrors.Visibility = System.Windows.Visibility.Collapsed;
            lblUnavailableComponents.Visibility = System.Windows.Visibility.Collapsed;
            trvUnavailableComponents.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void SetUnavailableComponents(IList<IOComponentType> unavailableComponents)
        {
            trvUnavailableComponents.Items.Clear();

            Dictionary<string, List<string>> processed = new Dictionary<string, List<string>>();
            foreach (IOComponentType item in unavailableComponents)
            {
                string itemName = item.Item;
                if (String.IsNullOrEmpty(item.Item) && !String.IsNullOrEmpty(item.Name))
                    itemName = item.Name;
                else if (String.IsNullOrEmpty(item.Item) && item.GUID != Guid.Empty)
                    itemName = item.GUID.ToString();
                else if (String.IsNullOrEmpty(item.Item))
                    itemName = "(unnamed)";

                if (item.Collection == null)
                {
                    if (!processed.ContainsKey("(unknown)"))
                        processed.Add("(unknown)", new List<string>());
                    processed["(unknown)"].Add(itemName);
                }
                else if (!processed.ContainsKey(item.Collection))
                {
                    processed.Add(item.Collection, new List<string>());
                    processed[item.Collection].Add(itemName);
                }
                else
                {
                    processed[item.Collection].Add(itemName);
                }
            }

            foreach (KeyValuePair<string, List<string>> item in processed)
            {
                TreeViewItem collectionItem = new TreeViewItem();
                collectionItem.Header = item.Key;

                item.Value.ForEach(leaf => collectionItem.Items.Add(leaf));

                collectionItem.IsExpanded = true;
                trvUnavailableComponents.Items.Add(collectionItem);
            }

            trvUnavailableComponents.Visibility = (unavailableComponents.Count > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed);
            lblUnavailableComponents.Visibility = trvUnavailableComponents.Visibility;
        }

        public void SetErrors(IEnumerable<string> errors)
        {
            lbxErrors.ItemsSource = errors;
            lbxErrors.Visibility = (errors.Count() > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed);
        }

        public void SetMessage(DocumentLoadResultType message)
        {
            switch (message)
            {
                case DocumentLoadResultType.FailIncorrectFormat:
                    lblMessage.Text = "The document was not in the correct format.";
                    break;
                case DocumentLoadResultType.SuccessNewerVersion:
                    lblMessage.Text = "The document was created in a newer version of Circuit Diagram and may not have loaded correctly.";
                    break;
                case DocumentLoadResultType.FailNewerVersion:
                    lblMessage.Text = "The document was created in a newer version of Circuit Diagram and could not be loaded correctly.";
                    break;
                case DocumentLoadResultType.FailUnknown:
                    lblMessage.Text = "An unknown error occurred while loading the document.";
                    break;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
