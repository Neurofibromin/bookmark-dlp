using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NfLogger;

namespace bookmark_dlp.ViewModels
{
    internal partial class AskConfigWindowViewModel : ViewModelBase
    {

        [ObservableProperty]
        private string _myLabel = "Starting application...";
        [ObservableProperty]
        private string _whichButton = "whichbutton";

        public void Cancel()
        {
            MyLabel = "Cancelling...";
            _cts.Cancel();
            Environment.Exit(0);
        }

        private readonly CancellationTokenSource _cts = new();

        public CancellationToken CancellationToken => _cts.Token;


        public AsyncRelayCommand<string> ButtonCommand { get; }
  

        public AskConfigWindowViewModel()
        {
            ButtonCommand = new AsyncRelayCommand<string>(HandleButtonClickAsync);
            
        }

        private async Task HandleButtonClickAsync(string buttonText)
        {
            WhichButton = buttonText;
            MessageBus.RaiseButtonClicked(buttonText);
            try
            {
                await AwaitButtonPressAsync(CancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
        }

        private async Task AwaitButtonPressAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            // Simulating some asynchronous operation
            // Implementation later
        }
    }
}
