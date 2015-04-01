// Toolbox.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2013  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Toolbox
{
    public class Toolbox : ItemsControl
    {
        public static readonly DependencyProperty CategoryMainItemTemplateProperty;
        public static readonly DependencyProperty CategoryItemTemplateProperty;
        public static readonly DependencyProperty SelectedCategoryItemProperty;

        public event EventHandler SelectionChanged;

        static Toolbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Toolbox), new FrameworkPropertyMetadata(typeof(Toolbox)));
            Toolbox.CategoryMainItemTemplateProperty = DependencyProperty.Register("CategoryMainItemTemplate", typeof(DataTemplate), typeof(Toolbox));
            Toolbox.CategoryItemTemplateProperty = DependencyProperty.Register("CategoryItemTemplate", typeof(DataTemplate), typeof(Toolbox));
            Toolbox.SelectedCategoryItemProperty = DependencyProperty.Register("SelectedCategoryItem", typeof(object), typeof(Toolbox));
        }

        public DataTemplate CategoryMainItemTemplate
        {
            get { return (DataTemplate)GetValue(Toolbox.CategoryMainItemTemplateProperty); }
            set { SetValue(Toolbox.CategoryMainItemTemplateProperty, value); }
        }

        public DataTemplate CategoryItemTemplate
        {
            get { return (DataTemplate)GetValue(Toolbox.CategoryItemTemplateProperty); }
            set { SetValue(Toolbox.CategoryItemTemplateProperty, value); }
        }

        public object SelectedCategoryItem
        {
            get { return GetValue(Toolbox.SelectedCategoryItemProperty); }
            set { SetValue(Toolbox.SelectedCategoryItemProperty, value); }
        }

        public void SetSelectedCategoryItem(object item)
        {
            foreach(IEnumerable<object> category in Items)
            {
                if (category.Contains(item))
                {
                    var cat = (ItemContainerGenerator.ContainerFromItem(category) as ToolboxCategory);
                    cat.SelectedItem = item;
                    SetSelected(cat);
                    break;
                }
            }
        }

        internal void SetSelected(ToolboxCategory selected)
        {
            foreach (object child in Items)
            {
                ToolboxCategory category = ItemContainerGenerator.ContainerFromItem(child) as ToolboxCategory;
                if (category != null)
                {
                    if (category == selected)
                        category.IsSelectedCategory = true;
                    else
                        category.IsSelectedCategory = false;
                }
            }

            if (selected != null)
            {
                SelectedCategoryItem = selected.SelectedItem;
            }

            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs());
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// true if the item is (or is eligible to be) its own container; otherwise, false.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ToolboxCategory;
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolboxCategory()
            {
                MainItemTemplate = CategoryMainItemTemplate,
                ItemDisplayTemplate = CategoryItemTemplate,
                ToolboxOwner = this
            };
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            (element as ToolboxCategory).ItemsSource = (System.Collections.IEnumerable)item;
        }
    }
}
