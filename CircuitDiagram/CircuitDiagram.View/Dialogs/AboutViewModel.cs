using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.View.Services;

namespace CircuitDiagram.View.Dialogs
{
    public class AboutViewModel
    {
        public AboutViewModel(IUpdateVersionService updateVersionService)
        {
            SelectedUpdateChannel = updateVersionService.GetSelectedUpdateChannel();
            AvailableUpdateChannels = updateVersionService.GetUpdateChannels().ToArray();
            Version = updateVersionService.GetAppDisplayVersion();
        }

        public string SelectedUpdateChannel { get; set; }

        public IList<string> AvailableUpdateChannels { get; }

        public string Version { get; }
    }
}
