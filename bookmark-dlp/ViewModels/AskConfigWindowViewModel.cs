using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bookmark_dlp.ViewModels;

/// <summary>
///     Separate window to ask the user where to save the config file for the bookmark-dlp application.
/// </summary>
internal partial class AskConfigWindowViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    [ObservableProperty] private string _myLabel = "Starting application...";

    [ObservableProperty] private string _whichButton = "whichbutton";


    public AskConfigWindowViewModel()
    {
        ButtonCommand = new AsyncRelayCommand<string>(HandleButtonClickAsync);
    }

    private CancellationToken CancellationToken => _cts.Token;


    public AsyncRelayCommand<string> ButtonCommand { get; }

    public void Cancel()
    {
        MyLabel = "Cancelling...";
        _cts.Cancel();
        Environment.Exit(0);
    }

    private async Task HandleButtonClickAsync(string? buttonText)
    {
        if (buttonText == null)
            return;
        WhichButton = buttonText;
        MessageBus.RaiseButtonClicked(buttonText);
        try
        {
            await AwaitButtonPressAsync(CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation: nothing to do
        }
    }

    private async Task AwaitButtonPressAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        // Simulating some asynchronous operation
        // Implementation later
    }
}