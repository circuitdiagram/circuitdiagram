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
using Prism.Mvvm;

namespace CircuitDiagram.View.Dialogs
{
    public class CheckForUpdatesViewModel : BindableBase
    {
        private readonly IUpdateVersionService updateVersionService;
        private bool isCheckingForUpdates;
        private bool isNewVersionAvailable;
        private Uri downloadUrl;
        private string newVersionName;

        public CheckForUpdatesViewModel(IUpdateVersionService updateVersionService)
        {
            this.updateVersionService = updateVersionService;
        }

        public bool IsCheckingForUpdates
        {
            get { return isCheckingForUpdates; }
            set
            {
                isCheckingForUpdates = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewVersionAvailable
        {
            get { return isNewVersionAvailable; }
            set
            {
                isNewVersionAvailable = value;
                OnPropertyChanged();
            }
        }

        public string NewVersionName
        {
            get { return newVersionName; }
            set
            {
                newVersionName = value;
                OnPropertyChanged();
            }
        }

        public Uri DownloadUrl
        {
            get { return downloadUrl; }
            set
            {
                downloadUrl = value;
                OnPropertyChanged();
            }
        }

        public async void CheckForUpdates()
        {
            IsCheckingForUpdates = true;
            IsNewVersionAvailable = false;

            var newVersion = await updateVersionService.CheckForUpdates();

            IsCheckingForUpdates = false;

            if (newVersion == null)
                return; // No new version available

            IsNewVersionAvailable = true;
            NewVersionName = newVersion.Version;
            DownloadUrl = new Uri(newVersion.DownloadUrl);
        }
    }
}
