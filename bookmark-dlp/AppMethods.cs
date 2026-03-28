using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using bookmark_dlp.Models;
using Nfbookmark;
using Serilog;

namespace bookmark_dlp;

public static class AppMethods
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(AppMethods));
    public enum ProgramUI { GUI, CLI }

    public static ProgramUI programUI;

    /// <summary>
    ///     Asks user if they want playlists, shorts and channels downloaded.
    /// </summary>
    /// <returns>Want (_downloadPlaylists, _downloadShorts, _downloadChannels) in this order.</returns>
    public static (bool, bool, bool) PromptForAdvancedDownloadOptions()
    {
        Log.Information("Do you want to write and download not video links? (eg. playlists and channels. by default: no)");
        Log.Information("Depending on the yt-dlp conf settings this can result in very large downloads, a single bookmark can lead to hundreds of videos being downloaded.");
        Log.Information("Y/N");
        bool downloadPlaylists = false;
        bool downloadShorts = false;
        bool downloadChannels = false;
        //TODO: only if interactive:
        if (string.Equals(Console.ReadKey().ToString() ?? "", "y", StringComparison.OrdinalIgnoreCase))
        {
            Log.Information("Playlists? Y/N");
            if (string.Equals(Console.ReadKey().ToString() ?? "", "y", StringComparison.OrdinalIgnoreCase))
                downloadPlaylists = true;
            Log.Information("Shorts? Y/N");
            if (string.Equals(Console.ReadKey().ToString() ?? "", "y", StringComparison.OrdinalIgnoreCase))
                downloadShorts = true; 
            Log.Information("Channels? Y/N");
            if (string.Equals(Console.ReadKey().ToString() ?? "", "y", StringComparison.OrdinalIgnoreCase))
                downloadChannels = true;
        }
        
        return (downloadPlaylists, downloadShorts, downloadChannels);
    }

    /// <summary>
    /// Finds config file for bookmark-dlp. Searches multiple locations, more details in project Readme. 
    /// </summary>
    /// <returns>Found config path or NULL, if config not found.</returns>
    public static string? ConfigFileLocation()
    {
        string configpath_local = Path.Combine(Directory.GetCurrentDirectory(), "bookmark-dlp.conf");
        if (File.Exists(configpath_local)) return configpath_local;
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bookmark-dlp.conf")))
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bookmark-dlp.conf");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string configpath_osx = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_osx)) { return configpath_osx; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string configpath_windows = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_windows)) { return configpath_windows; }
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string configpath_linux = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bookmark-dlp", "bookmark-dlp.conf");
            if (File.Exists(configpath_linux)) { return configpath_linux; }
        }
        return null; //no config paths were found
    }

    public static bool IsConfigPresent()
    {
        if (ConfigFileLocation() == null) return false;

        return true;
    }

    /// <summary>
    ///     Generating Hierarchical Observable FolderCollection from imported folders, used when displaying
    ///     the list of folders in the TreeDataGrid before links are resolved.
    /// </summary>
    /// <param name="folders">The imported folders to build the tree from.</param>
    /// <returns>An ObservableCollection of root-level HierarchicalFolderclass items.</returns>
    public static ObservableCollection<HierarchicalFolderclass> GenerateHierarchicalFolderclassesFromList(List<ImportedFolder> folders)
    {
        var hierarchicalFolders = folders.Select(f => new HierarchicalFolderclass(f)).ToList();
        var folderMap = hierarchicalFolders.ToDictionary(f => f.Id);
        var rootFolders = new ObservableCollection<HierarchicalFolderclass>();
    
        foreach (var folder in hierarchicalFolders)
        {
            if (folder.ParentId != 0 && folderMap.TryGetValue(folder.ParentId, out var parent))
            {
                parent._children ??= new ObservableCollection<HierarchicalFolderclass>();
                parent._children.Add(folder);
                parent.HasChildren = true;
            }
            else
            {
                // Set IsExpanded for root items
                folder.IsExpanded = true;
                rootFolders.Add(folder);
                if (folder.ParentId != 0)
                    Log.Error("The following folder has no parent despite depth != 0: {FolderName}", folder.Name);
            }
        }
    
        return rootFolders;
    }
    
    /// <summary>
    ///     Count how many videos are wanted directly or indirectly. <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> Links </item>
    ///         <item> link.MemberIds </item>
    ///     </list>
    ///     Fills:
    ///     <list type="bullet">
    ///         <item> DownloadStatus.NumberOfVideosDirectlyWanted </item>
    ///         <item> DownloadStatus.NumberOfVideosIndirectlyWanted </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">The resolved folders to count videos for.</param>
    public static void CountWantedVideos(List<ResolvedFolder> folders)
    {
        foreach (ResolvedFolder folder in folders)
        {
            foreach (YTLink link in folder.Links)
            {
                if (link.linktype == Linktype.Video || link.linktype == Linktype.Short)
                {
                    folder.DownloadStatus.NumberOfVideosDirectlyWanted++;
                }
                else if (link.linktype == Linktype.Channel_channel ||
                         link.linktype == Linktype.Channel_at ||
                         link.linktype == Linktype.Channel_user ||
                         link.linktype == Linktype.Channel_c ||
                         link.linktype == Linktype.Playlist)
                {
                    folder.DownloadStatus.NumberOfVideosIndirectlyWanted += link.MemberIds.Count;
                }
            }
        }
    }
    
    
    /// <summary>
    ///     Checks if wanted videos are found on the filesystem (are/were downloaded) and fills DownloadStatus
    ///     accordingly.
    ///     Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the
    ///     filename it will not work.<br />
    ///     Only queries filesystem, no requests to YouTube. <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> FolderPath </item>
    ///         <item> Links </item>
    ///         <item> link.MemberIds </item>
    ///     </list>
    ///     Fills:
    ///     <list type="bullet">
    ///         <item> DownloadStatus.NumberOfDirectlyWantedVideosFound </item>
    ///         <item> DownloadStatus.NumberOfIndirectlyWantedVideosFound </item>
    ///         <item> DownloadStatus.NumberOfOtherVideosFound </item>
    ///         <item> DownloadStatus.LinksWithNoMissingVideos </item>
    ///         <item> DownloadStatus.LinksWithMissingVideos </item>
    ///         <item> link.MemberIdsNotFound </item>
    ///         <item> link.MemberIdsFound </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">The resolved folders that are being checked.</param>
    public static void CheckCurrentFilesystemState(List<ResolvedFolder> folders)
    {
        foreach (ResolvedFolder folder in folders)
        {
            folder.DownloadStatus.NumberOfDirectlyWantedVideosFound = 0;
            folder.DownloadStatus.NumberOfIndirectlyWantedVideosFound = 0;
            folder.DownloadStatus.NumberOfOtherVideosFound = 0;
            if (Directory.Exists(folder.FolderPath))
            {
                var files = Directory.GetFiles(folder.FolderPath);
                HashSet<string>? ytIdsFoundInArchive = null;
                if (File.Exists(Path.Combine(folder.FolderPath, "archive.txt")))
                {
                    string[] archivecheckerlist = File.ReadAllLines(Path.Combine(folder.FolderPath, "archive.txt"));
                    string[] ytIdsFoundInArchiveTxt = archivecheckerlist.Where(f => f.StartsWith("youtube")).Select(t => t.Substring(8, 11)).ToArray();
                    ytIdsFoundInArchive = new HashSet<string>(ytIdsFoundInArchiveTxt);
                    /*foreach (string link in archivecheckerlist)
                    {
                        if (link.StartsWith("youtube")) //only check for youtube videos downloaded by yt-dlp in the given folder
                        {
                            string youtubeid = link.Substring(8, 11); //start at coloumn 8 because archive.txt does not store links, rather only the platform name and the video id
                        }
                    }*/
                }
                foreach (YTLink link in folder.Links)
                {
                    List<string>? idsToCheck = null;
                    bool found = true;
                    if (link.linktype == Linktype.Short ||
                        link.linktype == Linktype.Video)
                    {
                        if (ytIdsFoundInArchive != null)
                        {
                            if (ytIdsFoundInArchive.Contains(link.yt_id)) // in archive.txt
                            {
                                folder.DownloadStatus.LinksWithNoMissingVideos.Add(link);
                                Log.Verbose("In folder {FolderPath} video {VideoUrl} found in archive.txt", folder.FolderPath, link.url);
                                continue;
                            }
                        }
                        if (files.Any(s => s.Contains(link.yt_id)))
                        {
                            folder.DownloadStatus.LinksWithNoMissingVideos.Add(link);
                            Log.Verbose("In folder {FolderPath} video {VideoUrl} found in files list", folder.FolderPath, link.url);
                        }
                        else
                        {
                            folder.DownloadStatus.LinksWithMissingVideos.Add(link);
                            Log.Verbose("In folder {FolderPath} video {VideoUrl} not found", folder.FolderPath, link.url);
                        }
                        continue;
                    }
                    else if (link.linktype == Linktype.Channel_channel ||
                             link.linktype == Linktype.Channel_at ||
                             link.linktype == Linktype.Channel_user ||
                             link.linktype == Linktype.Channel_c ||
                             link.linktype == Linktype.Playlist)
                    {
                        idsToCheck = link.MemberIds;
                    }
                    else //link.linktype == Linktype.Search
                    {
                        continue;
                    }
                    if (idsToCheck == null)
                    {
                        Log.Error("Could not ascertain which videos are wanted by link {Link}. May be a network error", link);
                        found = false;
                        folder.DownloadStatus.LinksWithMissingVideos.Add(link);
                        continue;
                    }
                    foreach (string id in idsToCheck)
                    {
                        if (ytIdsFoundInArchive != null)
                        {
                            if (ytIdsFoundInArchive.Contains(id)) // in archive.txt
                            {
                                link.MemberIdsFound.Add(id);
                                Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} found in archive.txt", folder.FolderPath, id, link.url);
                                continue;
                            }
                        }
                        if (files.Any(s => s.Contains(id)))
                        {
                            link.MemberIdsFound.Add(id);
                            Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} found in files list", folder.FolderPath, id, link.url);
                        }
                        else
                        {
                            link.MemberIdsNotFound.Add(id);
                            Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} not found", folder.FolderPath, id, link.url);
                            found = false;
                        }
                    }
                    if (found)
                    {
                        folder.DownloadStatus.LinksWithNoMissingVideos.Add(link);
                        Log.Verbose("All members were found for {LinkUrl} in folder {FolderPath}", link.url, folder.FolderPath);
                    }
                    else
                    {
                        folder.DownloadStatus.LinksWithMissingVideos.Add(link);
                        Log.Verbose("Not all members were found for {LinkUrl} in folder {FolderPath}", link.url, folder.FolderPath);
                    }
                }
            }
            folder.DownloadStatus.NumberOfIndirectlyWantedVideosFound = folder.Links.Select(f => f.MemberIdsFound.Count).Sum();
            folder.DownloadStatus.NumberOfDirectlyWantedVideosFound = folder.DownloadStatus.LinksWithNoMissingVideos.Count(f => f.linktype is Linktype.Video or Linktype.Short);
        }
    }

    public static void ValidateFolderBeforeDownload(ResolvedFolder folder)
    {
        foreach (YTLink link in folder.Links)
        {
            if (link.linktype is Linktype.Channel_channel or Linktype.Channel_at or Linktype.Channel_user or Linktype.Channel_c or Linktype.Playlist)
            {
                HashSet<string> allids = new HashSet<string>(link.MemberIds);
                HashSet<string> found_ids = new HashSet<string>(link.MemberIdsFound);
                HashSet<string> not_found_ids = new HashSet<string>(link.MemberIdsNotFound);
                found_ids.UnionWith(not_found_ids);
                Debug.Assert(allids.SetEquals(found_ids));
            }
        }
        HashSet<YTLink> found = new HashSet<YTLink>(folder.DownloadStatus.LinksWithNoMissingVideos);
        HashSet<YTLink> notfound = new HashSet<YTLink>(folder.DownloadStatus.LinksWithMissingVideos);
        HashSet<YTLink> all = new HashSet<YTLink>(folder.Links);
        found.UnionWith(notfound);
        Debug.Assert(all.SetEquals(found));
    }
}