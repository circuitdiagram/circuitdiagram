using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CircuitDiagram
{
    public static class CustomCommands
    {
        static CustomCommands()
        {
            EditComponent = new RoutedUICommand("Edit", "Edit", typeof(CustomCommands));
            DeleteComponent = new RoutedUICommand("Delete", "Delete", typeof(CustomCommands));
            NewDocument = new RoutedUICommand("New", "New", typeof(CustomCommands));
        }

        public static RoutedUICommand EditComponent
        {
            get;
            private set;
        }

        public static RoutedUICommand DeleteComponent
        {
            get;
            private set;
        }

        public static RoutedUICommand NewDocument
        {
            get;
            private set;
        }
    }
}
