using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp.ViewModels
{
    internal partial class AskConfigWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private string _myLabel = "Starting application...";

        public void Cancel()
        {
            MyLabel = "Cancelling...";
            _cts.Cancel();
        }

        private readonly CancellationTokenSource _cts = new();

        public CancellationToken CancellationToken => _cts.Token;
    }
}
