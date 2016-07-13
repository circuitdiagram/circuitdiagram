// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Threading.Tasks;
using CircuitDiagram.View.Services;
using Microsoft.Practices.ServiceLocation;
using System.Windows;

namespace CircuitDiagram.Dialogs
{
    class DialogService : IDialogService
    {
        public bool? ShowDialog(string title, object viewModel)
        {
            var viewTypeName = viewModel.GetType().FullName.Replace("ViewModel", "View");
            var viewType = viewModel.GetType().Assembly.GetType(viewTypeName);

            // Build the view and model, and bind them
            var view = (FrameworkElement)ServiceLocator.Current.GetInstance(viewType);
            view.DataContext = viewModel;

            var host = new DialogWindow
            {
                Title = title,
                Owner = Application.Current.MainWindow
            };

            host.SetChild(view);

            return host.ShowDialog();
        }
    }
}
