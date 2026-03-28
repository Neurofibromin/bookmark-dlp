using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using Classic.Avalonia.Theme;
using System;

namespace bookmark_dlp.Models;

public static class ThemeManager
{
    public static void ApplyTheme(AppTheme theme)
    {
        if (Application.Current is null) return;

        Application.Current.Styles.Clear();

        // 1. Add the base theme
        switch (theme)
        {
            case AppTheme.Simple:
                Application.Current.Styles.Add(new SimpleTheme());
                break;
            case AppTheme.Classic:
                Application.Current.Styles.Add(new ClassicTheme());
                break;
            case AppTheme.Fluent:
            default:
                Application.Current.Styles.Add(new FluentTheme());
                break;
        }

        // 2. Add common resources required by themes (like TreeDataGrid styles and Icons)
        Uri treeDataGridUri = theme == AppTheme.Fluent
            ? new Uri("avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml")
            : new Uri("avares://Semi.Avalonia.TreeDataGrid/Index.axaml");
        
        Application.Current.Styles.Add(new StyleInclude(treeDataGridUri) { Source = treeDataGridUri });

        var localIconsUri = new Uri("avares://bookmark-dlp/Assets/Icons.axaml");
        Application.Current.Styles.Add(new StyleInclude(localIconsUri) { Source = localIconsUri });
    }
}