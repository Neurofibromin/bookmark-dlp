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
    ///     Generating Hierarchical Observable FolderCollection from folders, used when displaying the list of folders in the
    ///     TreeDataGrid.
    /// </summary>
    /// <param name="folders"></param>
    /// <returns></returns>
    public static ObservableCollection<HierarchicalFolderclass> GenerateHierarchicalFolderclassesFromList(List<Folderclass> folders)
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
    
    public static List<Folderclass> GenerateListFolderclassesFromHierarchical(
        ObservableCollection<HierarchicalFolderclass> folders)
    {
        var flatList = new List<Folderclass>();
        foreach (var hierarchicalFolder in folders)
        {
            FlattenHierarchicalFolder(hierarchicalFolder, flatList);
        }
        flatList = flatList.OrderBy(f => f.id).ToList();
        return flatList;
    }

    private static void FlattenHierarchicalFolder(HierarchicalFolderclass hierarchicalFolder, List<Folderclass> flatList)
    {
        var folder = new Folderclass
        {
            startline = hierarchicalFolder.Startline,
            name = hierarchicalFolder.Name,
            depth = hierarchicalFolder.Depth,
            endingline = hierarchicalFolder.Endingline,
            folderpath = hierarchicalFolder.Folderpath,
            urls = hierarchicalFolder.Urls,
            links = hierarchicalFolder.Links,
            id = hierarchicalFolder.Id,
            parentId = hierarchicalFolder.ParentId,
            childrenIds = hierarchicalFolder.ChildrenIds,
            downloadStatus = new DownloadStatus
            {
                LinksWithMissingVideos = hierarchicalFolder.LinksWithMissingVideos,
                LinksWithNoMissingVideos = hierarchicalFolder.LinksWithNoMissingVideos,
                WantDownloaded = hierarchicalFolder.WantDownloaded,
                NumberOfVideosDirectlyWanted = hierarchicalFolder.NumberOfVideosDirectlyWanted,
                NumberOfVideosIndirectlyWanted = hierarchicalFolder.NumberOfVideosIndirectlyWanted,
                NumberOfDirectlyWantedVideosFound = hierarchicalFolder.NumberOfDirectlyWantedVideosFound,
                NumberOfIndirectlyWantedVideosFound = hierarchicalFolder.NumberOfIndirectlyWantedVideosFound,
                NumberOfOtherVideosFound = hierarchicalFolder.NumberOfOtherVideosFound
            }
        };

        flatList.Add(folder);

        if (hierarchicalFolder.Children != null)
        {
            foreach (var child in hierarchicalFolder.Children)
            {
                FlattenHierarchicalFolder(child, flatList);
            }
        }
    }

    public static void MethodRunner(Func<Folderclass, string> func, ObservableCollection<HierarchicalFolderclass> folders)
    {
        
    }
    
    /// <summary>
    ///     Count how many videos are wanted directly or indirectly. <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> links </item>
    ///         <item> link.member_ids </item>
    ///     </list>
    ///     Fills:
    ///     <list type="bullet">
    ///         <item> numberOfVideosDirectlyWanted </item>
    ///         <item> numberOfVideosIndirectlyWanted </item>
    ///     </list>
    /// </summary>
    /// <param name="folders"></param>
    public static void CountWantedVideos(ref List<Folderclass> folders)
    {
        foreach (Folderclass folder in folders)
        {
            foreach (YTLink link in folder.links)
            {
                if (link.linktype == Linktype.Video || link.linktype == Linktype.Short)
                {
                    folder.downloadStatus.NumberOfVideosDirectlyWanted++;
                }
                else if (link.linktype == Linktype.Channel_channel ||
                         link.linktype == Linktype.Channel_at ||
                         link.linktype == Linktype.Channel_user ||
                         link.linktype == Linktype.Channel_c ||
                         link.linktype == Linktype.Playlist)
                {
                    folder.downloadStatus.NumberOfVideosIndirectlyWanted += link.member_ids.Count;
                }
            }
        }
    }
    
    
    /// <summary>
    ///     Checks if wanted videos are found on the filesystem (are/were downloaded) and fills Folderclass fields for object
    ///     accordingly.
    ///     Only checks the filenames for the yt-id (11 characters): if yt-dlp config is set to not include such id in the
    ///     filename it will not work.<br />
    ///     Only queries filesystem, no requests to YouTube. <br />
    ///     Requires:
    ///     <list type="bullet">
    ///         <item> folderpath </item>
    ///         <item> links </item>
    ///         <item> link.member_ids </item>
    ///     </list>
    ///     Fills:
    ///     <list type="bullet">
    ///         <item> numberOfDirectlyWantedVideosFound </item>
    ///         <item> numberOfIndirectlyWantedVideosFound </item>
    ///         <item> numberOfOtherVideosFound </item>
    ///         <item> LinksWithNoMissingVideos </item>
    ///         <item> LinksWithMissingVideos </item>
    ///         <item> link.member_ids_not_found </item>
    ///         <item> link.member_ids_found </item>
    ///     </list>
    /// </summary>
    /// <param name="folders">The folders that are being checked</param>
    public static void CheckCurrentFilesystemState(ref List<Folderclass> folders)
    {
        foreach (Folderclass folder in folders)
        {
            folder.downloadStatus.NumberOfDirectlyWantedVideosFound = 0;
            folder.downloadStatus.NumberOfIndirectlyWantedVideosFound = 0;
            folder.downloadStatus.NumberOfOtherVideosFound = 0;
            if (Directory.Exists(folder.folderpath))
            {
                var files = Directory.GetFiles(folder.folderpath);
                HashSet<string>? ytIdsFoundInArchive = null;
                if (File.Exists(Path.Combine(folder.folderpath, "archive.txt"))) //checks the archive.txt written by yt-dlp if the config is used there
                {
                    string[] archivecheckerlist = File.ReadAllLines(Path.Combine(folder.folderpath, "archive.txt"));
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
                foreach (YTLink link in folder.links)
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
                                folder.downloadStatus.LinksWithNoMissingVideos.Add(link);
                                Log.Verbose("In folder {FolderPath} video {VideoUrl} found in archive.txt", folder.folderpath, link.url);
                                continue;
                            }
                        }
                        if (files.Any(s => s.Contains(link.yt_id)))
                        {
                            folder.downloadStatus.LinksWithNoMissingVideos.Add(link);
                            Log.Verbose("In folder {FolderPath} video {VideoUrl} found in files list", folder.folderpath, link.url);
                        }
                        else
                        {
                            folder.downloadStatus.LinksWithMissingVideos.Add(link);
                            Log.Verbose("In folder {FolderPath} video {VideoUrl} not found", folder.folderpath, link.url);
                        }
                        continue;
                    }
                    else if (link.linktype == Linktype.Channel_channel ||
                             link.linktype == Linktype.Channel_at ||
                             link.linktype == Linktype.Channel_user ||
                             link.linktype == Linktype.Channel_c ||
                             link.linktype == Linktype.Playlist)
                    {
                        idsToCheck = link.member_ids;
                    }
                    else //link.linktype == Linktype.Search
                    {
                        continue;
                    }
                    if (idsToCheck == null)
                    {
                        Log.Error("Could not ascertain which videos are wanted by link {Link}. May be a network error", link);
                        found = false;
                        folder.downloadStatus.LinksWithMissingVideos.Add(link);
                        continue;
                    }
                    foreach (string id in idsToCheck)
                    {
                        if (ytIdsFoundInArchive != null)
                        {
                            if (ytIdsFoundInArchive.Contains(id)) // in archive.txt
                            {
                                link.member_ids_found.Add(id);
                                Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} found in archive.txt", folder.folderpath, id, link.url);
                                continue;
                            }
                        }
                        //NOTE: this may be a slow operation
                        if (files.Any(s => s.Contains(id)))
                        {
                            link.member_ids_found.Add(id);
                            Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} found in files list", folder.folderpath, id, link.url);
                        }
                        else
                        {
                            link.member_ids_not_found.Add(id);
                            Log.Verbose("In folder {FolderPath} member id {MemberId} for link {LinkUrl} not found", folder.folderpath, id, link.url);
                            found = false;
                        }
                    }
                    if (found)
                    {
                        folder.downloadStatus.LinksWithNoMissingVideos.Add(link);
                        Log.Verbose("All members were found for {LinkUrl} in folder {FolderPath}", link.url, folder.folderpath);
                    }
                    else
                    {
                        folder.downloadStatus.LinksWithMissingVideos.Add(link);
                        Log.Verbose("Not all members were found for {LinkUrl} in folder {FolderPath}", link.url, folder.folderpath);
                    }
                }
            }
            // folder.numberOfIndirectlyWantedVideosNotFound = folder.links.Select(f => f.member_ids_not_found).ToList().Select(f => f.Count()).Sum();
            // folder.numberOfDirectlyWantedVideosNotFound = folder.LinksWithMissingVideos.Count(f => f.linktype is Linktype.Video or Linktype.Short);
            // folder.numberOfDirectlyWantedVideosNotFound = folder.numberOfVideosDirectlyWanted - folder.numberOfDirectlyWantedVideosFound;
            folder.downloadStatus.NumberOfIndirectlyWantedVideosFound = folder.links.Select(f => f.member_ids_found.Count).Sum();
            folder.downloadStatus.NumberOfDirectlyWantedVideosFound = folder.downloadStatus.LinksWithNoMissingVideos.Count(f => f.linktype is Linktype.Video or Linktype.Short);
   
        }
    }

    public static void ValidateFolderclassBeforeDownload(Folderclass folder)
    {
        foreach (YTLink link in folder.links)
        {
            if (link.linktype == Linktype.Channel_channel ||
                link.linktype == Linktype.Channel_at ||
                link.linktype == Linktype.Channel_user ||
                link.linktype == Linktype.Channel_c ||
                link.linktype == Linktype.Playlist)
            {
                HashSet<string> allids = new HashSet<string>(link.member_ids);
                HashSet<string> found_ids = new HashSet<string>(link.member_ids_found);
                HashSet<string> not_found_ids = new HashSet<string>(link.member_ids_not_found);
                found_ids.UnionWith(not_found_ids);
                Debug.Assert(allids.SetEquals(found_ids));
            }
        }
        HashSet<YTLink> found = new HashSet<YTLink>(folder.downloadStatus.LinksWithNoMissingVideos);
        HashSet<YTLink> notfound = new HashSet<YTLink>(folder.downloadStatus.LinksWithMissingVideos);
        HashSet<YTLink> all = new HashSet<YTLink>(folder.links);
        found.UnionWith(notfound);
        Debug.Assert(all.SetEquals(found));
    }
}