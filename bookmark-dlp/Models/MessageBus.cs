namespace bookmark_dlp.Models;

public class MessageBus
{
    public static event EventHandler<string>? ButtonClicked;

    public static void RaiseButtonClicked(string buttonText)
    {
        ButtonClicked?.Invoke(null, buttonText);
    }
}