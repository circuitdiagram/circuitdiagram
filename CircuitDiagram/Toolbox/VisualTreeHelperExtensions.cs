using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Toolbox
{
    static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Finds a parent of a given control in the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of Parent to find.</typeparam>
        /// <param name="child">Child to find parent for.</param>
        /// <returns>Returns the first parent item that matched the type (T) if one is found, else null.</returns>
        public static T FindParent<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(obj);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static T FindChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
