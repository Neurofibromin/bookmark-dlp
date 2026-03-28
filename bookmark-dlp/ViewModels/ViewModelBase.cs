using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bookmark_dlp.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string>? _errorMessages;

    protected ViewModelBase()
    {
        ErrorMessages = new ObservableCollection<string>();
    }
}