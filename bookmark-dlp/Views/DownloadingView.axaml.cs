using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media;
using bookmark_dlp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nfbookmark;
using NfLogger;

namespace bookmark_dlp.Views;

public partial class DownloadingView : UserControl
{
    private static IconConverter? _iconConverter;
    
    public DownloadingView()
    {
        InitializeComponent();
    }
    
    #region IconConverter

    private class IconConverter : IMultiValueConverter
    {
        private readonly StreamGeometry _folderCollapsed;
        private readonly StreamGeometry _folderExpanded;

        public IconConverter(StreamGeometry folderExpanded, StreamGeometry folderCollapsed)
        {
            _folderExpanded = folderExpanded;
            _folderCollapsed = folderCollapsed;
        }

        public object? Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is null || values.Count < 1)
                return null;
            if (values[0] is bool isExpanded) return isExpanded ? _folderExpanded : _folderCollapsed;
            return null;
        }
    }

    /// <summary>
    ///     Implements IconConverter for folder icons of folderIconopen and folderIconopen
    /// </summary>
    public static IMultiValueConverter FileIconConverter
    {
        get
        {
            // Logger.LogVerbose("Getting FileIconConverter", Logger.Verbosity.Trace);
            if (_iconConverter is null)
            {
                Logger.LogVerbose("FileIconConverter is NULL", Logger.Verbosity.Trace);
                bool a = Application.Current!.Styles.TryGetResource("folder_regular",
                    Application.Current.ActualThemeVariant, out object? folderIconregular);
                bool b = Application.Current.Styles.TryGetResource("folder_open_regular",
                    Application.Current.ActualThemeVariant, out object? folderIconopen);

                if (a && b && folderIconopen is StreamGeometry openFolderGeometry &&
                    folderIconregular is StreamGeometry regularFolderGeometry)
                {
                    Logger.LogVerbose("FileIconConverter found", Logger.Verbosity.Trace);
                    _iconConverter = new IconConverter(openFolderGeometry, regularFolderGeometry);
                }
                else
                {
                    if (a || b)
                    {
                        if (folderIconopen is StreamGeometry openFolderGeometry2)
                        {
                            Logger.LogVerbose("Only folderIconopen found", Logger.Verbosity.Error);
                            _iconConverter = new IconConverter(openFolderGeometry2, new StreamGeometry());
                        }
                        else if (folderIconregular is StreamGeometry regularFolderGeometry2)
                        {
                            Logger.LogVerbose("Only folderIconregular found", Logger.Verbosity.Error);
                            _iconConverter = new IconConverter(new StreamGeometry(), regularFolderGeometry2);
                        }
                        else
                        {
                            Logger.LogVerbose("Failed to load folder icons. Using default values.",
                                Logger.Verbosity.Error);
                            _iconConverter =
                                new IconConverter(new StreamGeometry(), new StreamGeometry()); // Provide default values
                        }
                    }
                    else
                    {
                        Logger.LogVerbose("Failed to load folder icons. Using default values.", Logger.Verbosity.Error);
                        _iconConverter =
                            new IconConverter(new StreamGeometry(), new StreamGeometry()); // Provide default values
                    }
                }


                /*if (a && b)
                    Logger.LogVerbose("FileIconConverter found", Logger.Verbosity.Trace);
                else
                    Logger.LogVerbose("FileIconConverter not found", Logger.Verbosity.Error);
                Logger.LogVerbose("Found folder icon resources.", Logger.Verbosity.Trace);
                _iconConverter = new IconConverter((StreamGeometry) folderIconopen, (StreamGeometry) folderIconregular);*/
            }

            return _iconConverter;
        }
    }

    #endregion
}