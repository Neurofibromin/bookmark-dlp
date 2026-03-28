using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Web;
using bookmark_dlp.Models;
using Nfbookmark;
using Serilog;

namespace bookmark_dlp;

public static class YtdlpInterfacing
{
    private static readonly ILogger Log = Serilog.Log.ForContext(typeof(YtdlpInterfacing));
    public static string? YtdlpPath { get; set; }

    /// <summary>
    ///     Synchronously executes the yt-dlp process with the given arguments.
    /// </summary>
    /// <param name="arguments">The arguments to pass to yt-dlp.</param>
    /// <returns>A tuple containing the standard output, standard error, and exit code.</returns>
    private static (string Output, string Error, int ExitCode) ExecuteYtDlpProcess(string arguments)
    {
        if (string.IsNullOrEmpty(YtdlpPath))
        {
            Log.Error("YtdlpPath is not set in {MethodName}", nameof(ExecuteYtDlpProcess));
            throw new InvalidOperationException("YtDlpPath is not set.");
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = YtdlpPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        int exitCode = process.ExitCode;
        process.Close();
        if (exitCode != 0)
        {
            if (error.Contains("Unable to download webpage: HTTP Error 404") ||
                error.Contains("Unable to download API page") ||
                error.Contains("Failed to establish a new connection: [Errno -3]"))
            {
                Log.Error("yt-dlp error (404/API/Connection): {Error}", error);
            }

            if (output.Contains("Unable to download webpage: HTTP Error 404") ||
                output.Contains("Unable to download API page") ||
                output.Contains("Failed to establish a new connection: [Errno -3]"))
            {
                Log.Error("yt-dlp output error (404/API/Connection): {Output}", output);
            }
        }
        
        return (output, error, exitCode);
    }

    /// <summary>
    ///     Gets list of video ids from a given channel. Requires internet.
    /// </summary>
    /// <param name="url">The FQDN of the channel. May be https://www.youtube.com/@SomeChannel or (...)/@SomeChannel/Videos</param>
    /// <returns>
    ///     List of 11 char long youtube ids for the videos uploaded by the channel. Or null if channel does not exist or
    ///     no internet.
    /// </returns>
    public static List<string>? GetVideoIdsFromChannelUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (url.ToLower().EndsWith("/videos"))
            url = url.Substring(0, url.Length - "/videos".Length);

        try
        {
            var (output, error, exitCode) = ExecuteYtDlpProcess($"-j --flat-playlist \"{url}\"");

            if (exitCode != 0)
            {
                Log.Error("yt-dlp exited with code {ExitCode} for channel {Url}. Error: {Error}", exitCode, url, error);
                return null;
            }

            // Use LINQ for safer and more concise parsing
            List<string> lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(line => {
                             try { return JsonDocument.Parse(line); }
                             catch (JsonException ex) { 
                                 Log.Error(ex, "Error parsing JSON for line: {Line}", line);
                                 return null; 
                             }
                         })
                         .Where(doc => doc != null)
                         .Select(doc => doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null)
                         .Where(id => !string.IsNullOrEmpty(id))
                         .ToList();
            return lines;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to query channel {Url}", url);
            return null;
        }
    }
    
    /// <summary>
    ///     Extracts the playlist ID from a given YouTube URL.
    /// </summary>
    /// <param name="url">
    ///     The YouTube URL, e.g., "https://www.youtube.com/watch?v=12345678912&amp;
    ///     list=PL123456789123456789-4568789123&amp;index=1"
    /// </param>
    /// <returns>The playlist ID if found, otherwise null.</returns>
    public static string? ExtractPlaylistId(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        try
        {
            Uri uri = new Uri(url);
            NameValueCollection queryParameters = HttpUtility.ParseQueryString(uri.Query);
            return queryParameters["list"];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to extract playlist ID from URL: {Url}", url);
            return null;
        }
    }

    /// <summary>
    ///     Gets a list of video IDs from a given playlist URL. Requires internet.
    /// </summary>
    /// <param name="url">The FQDN of the playlist. E.g., https://www.youtube.com/playlist?list=PL123456789123456789-4568789123</param>
    /// <returns>
    ///     List of 11-character-long YouTube video IDs from the playlist. Returns null if the playlist does not exist or
    ///     no internet is available.
    /// </returns>
    public static List<string>? GetVideoIdsFromPlaylistUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        string? playlistId = ExtractPlaylistId(url);
        if (playlistId is null)
        {
            Log.Warning("URL does not contain a valid playlist ID: {Url}", url);
            return null;
        }

        string playlistUrl = "https://www.youtube.com/playlist?list=" + playlistId;
        Log.Debug("Parsed playlist url from {OriginalUrl} to {PlaylistUrl}", url, playlistUrl);
        string output;
        try
        {
            string error;
            int exitCode;
            (output, error, exitCode) = ExecuteYtDlpProcess($"-j --flat-playlist \"{playlistUrl}\"");

            if (exitCode != 0)
            {
                Log.Error("yt-dlp exited with code {ExitCode} for playlist {PlaylistUrl}. Error: {Error}", exitCode, playlistUrl, error);
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to query playlist {PlaylistUrl}", playlistUrl);
            return null;
        }

        try
        {
            // Use LINQ for safer and more concise parsing
            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => {
                    try { return JsonDocument.Parse(line); }
                    catch (JsonException ex) {
                        Log.Error(ex, "Error parsing JSON for line: {Line}", line);
                        return null;
                    }
                })
                .Where(doc => doc != null)
                .Select(doc => doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse result of playlist query {PlaylistUrl}", playlistUrl);
            return null;
        }
    }

    /// <summary>
    ///     Finds the yt-dlp binary by checking several standard locations, including the system's PATH.
    /// </summary>
    /// <param name="rootdir">The directory the application is called from.</param>
    /// <param name="output_folder">The configured output folder.</param>
    /// <returns>The full path to the yt-dlp binary if found; otherwise, null.</returns>
    public static string? Yt_dlp_pathfinder(string? rootdir = "", string? output_folder = "")
    {
        // 1. Define OS-specific executable names.
        string[] executableNames;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            executableNames = new[] { "yt-dlp.exe" };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            executableNames = new[] { "yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            executableNames = new[] { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
        }
        else
        {
            return null; // Unsupported OS
        }

        // 2. Check specified local directories first.
        var localSearchDirs = new List<string?>
        {
            rootdir,
            AppDomain.CurrentDomain.BaseDirectory,
            output_folder
        }.Where(d => !string.IsNullOrWhiteSpace(d));

        string? foundPath = SearchInDirectories(localSearchDirs!, executableNames);
        if (foundPath != null)
        {
            Log.Debug("yt-dlp binary found at: {FoundPath}", foundPath);
            return foundPath;
        }

        // 3. If not found locally, check the system's PATH.
        Log.Debug("yt-dlp not found in local directories, searching PATH.");
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator);
            if (pathDirs != null)
            {
                 foundPath = SearchInDirectories(pathDirs, executableNames);
            }
        }
        else // For Linux and macOS
        {
            foreach (var name in executableNames)
            {
                string command = $"-c \"command -v {name}\""; 
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = command,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(result))
                {
                    foundPath = File.Exists(result) ? result : name;
                    break;
                }
            }
        }
        
        if (foundPath != null) 
        {
             Log.Debug("yt-dlp found on system PATH: {FoundPath}", foundPath);
        }
        else
        {
            Log.Warning("yt-dlp not installed or not on PATH.");
        }

        return foundPath;
    }

    /// <summary>
    /// Searches a list of directories for the first occurrence of any of the specified file names.
    /// </summary>
    /// <param name="directories">The directories to search in.</param>
    /// <param name="fileNames">The names of the files to search for.</param>
    /// <returns>The full path of the first file found, or null if no file is found.</returns>
    private static string? SearchInDirectories(IEnumerable<string> directories, IEnumerable<string> fileNames)
    {
        foreach (var dir in directories)
        {
            foreach (var fileName in fileNames)
            {
                var fullPath = Path.Combine(dir, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }
        return null;
    }

    /// <summary>
    ///     Searches for yt-dlp configuration files in all standard locations.
    ///     This includes portable, user, and system-wide paths as defined by the official yt-dlp documentation.
    /// </summary>
    /// <param name="rootdir">The directory where the bookmark-dlp program is called from.</param>
    /// <param name="ytdlp_path">The full path to the yt-dlp executable.</param>
    /// <param name="output_folder">The configured output folder for downloads.</param>
    /// <returns>An ObservableCollection of unique, existing yt-dlp config file paths.</returns>
    public static ObservableCollection<string> Yt_dlp_configfinder(string? rootdir = "", string? ytdlp_path = "",
        string? output_folder = "")
    {
        // Use a HashSet to store unique file paths and avoid duplicates.
        var ytdlpConfigPaths = new HashSet<string>();

        // Helper function to check for a file's existence and add it to the set.
        void AddExistingFile(string? path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                ytdlpConfigPaths.Add(path);
            }
        }

        // 1. Directory where bookmark-dlp is called from.
        if (!string.IsNullOrEmpty(rootdir))
        {
            AddExistingFile(Path.Combine(rootdir, "yt-dlp.conf"));
        }

        // 2. Directory where the yt-dlp executable is found (Portable Configuration).
        if (!string.IsNullOrEmpty(ytdlp_path))
        {
            var ytdlpDir = Path.GetDirectoryName(ytdlp_path);
            if (!string.IsNullOrEmpty(ytdlpDir) && Directory.Exists(ytdlpDir))
            {
                AddExistingFile(Path.Combine(ytdlpDir, "yt-dlp.conf"));
            }
        }

        // 3. The configured output folder.
        if (!string.IsNullOrEmpty(output_folder))
        {
            AddExistingFile(Path.Combine(output_folder, "yt-dlp.conf"));
        }
        
        // 4. Directory where the bookmark-dlp executable is found.
        AddExistingFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.conf"));

        // 5. Default locations yt-dlp looks for (User and System Configurations).
        var potentialLocations = new List<string>();
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // User Configuration Locations
        if (OperatingSystem.IsWindows())
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            potentialLocations.AddRange(new[]
            {
                Path.Combine(appData, "yt-dlp.conf"),
                Path.Combine(appData, "yt-dlp", "config"),
                Path.Combine(appData, "yt-dlp", "config.txt")
            });
        }
        else // For Linux and macOS, respect the XDG Base Directory Specification.
        {
            string? xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (string.IsNullOrEmpty(xdgConfigHome) || !Directory.Exists(xdgConfigHome))
            {
                xdgConfigHome = Path.Combine(userProfile, ".config");
            }

            potentialLocations.AddRange(new[]
            {
                Path.Combine(xdgConfigHome, "yt-dlp.conf"),
                Path.Combine(xdgConfigHome, "yt-dlp", "config"),
                Path.Combine(xdgConfigHome, "yt-dlp", "config.txt")
            });
        }

        // Home directory locations (common for all OS).
        potentialLocations.AddRange(new[]
        {
            Path.Combine(userProfile, "yt-dlp.conf"),
            Path.Combine(userProfile, "yt-dlp.conf.txt"),
            Path.Combine(userProfile, ".yt-dlp", "config"),
            Path.Combine(userProfile, ".yt-dlp", "config.txt")
        });

        // System-wide locations (Linux/macOS).
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            potentialLocations.AddRange(new[]
            {
                "/etc/yt-dlp.conf",
                "/etc/yt-dlp/config",
                "/etc/yt-dlp/config.txt"
            });
        }

        // Check all potential default locations.
        foreach (string location in potentialLocations)
        {
            AddExistingFile(location);
        }

        return new ObservableCollection<string>(ytdlpConfigPaths);
    }

    /// <summary>
    ///     Wrapper for private functions
    /// </summary>
    public static async Task GetEstimatedSizes(ObservableCollection<HierarchicalFolderclass> folders)
    {
        await Task.WhenAll(folders.Select(GetFolderEstimatedSizes));
    }

    /// <summary>
    ///     Wrapper for link-by-link getting
    /// </summary>
    private static async Task GetFolderEstimatedSizes(HierarchicalFolderclass rootfolder)
    {
        rootfolder.EstimatedSize = 0;
        foreach (YTLink link in rootfolder.LinksWithMissingVideos)
        {
            rootfolder.EstimatedSize += await GetEstimatedSize(link);
        }

        if (rootfolder.Children?.Count > 0) 
            await Task.WhenAll(rootfolder.Children.Select(GetFolderEstimatedSizes));
    }

    /// <summary>
    ///     Calls yt-dlp to get the approximate filesize of a video.
    /// </summary>
    private static async Task<int> GetEstimatedSize(YTLink link)
    {
        try
        {
            var (output, error, exitCode) = await ExecuteYtDlpProcessAsync($"--print \"filesize_approx\" -q \"{link.url}\"");

            if (exitCode != 0)
            {
                Log.Error("yt-dlp exited with code {ExitCode} for size query on {Url}. Error: {Error}", exitCode, link.url, error);
                return 0;
            }

            // Parse the first valid integer from the output.
            string? firstLine = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (int.TryParse(firstLine, out int parsedSize))
            {
                return parsedSize;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get estimated size for {Url}", link.url);
            return 0;
        }
    }
    
    /// <summary>
    ///     Asynchronously executes the yt-dlp process with the given arguments.
    /// </summary>
    /// <param name="arguments">The arguments to pass to yt-dlp.</param>
    /// <returns>A tuple containing the standard output, standard error, and exit code.</returns>
    private static async Task<(string Output, string Error, int ExitCode)> ExecuteYtDlpProcessAsync(string arguments)
    {
        if (string.IsNullOrEmpty(YtdlpPath))
        {
            Log.Error("YtdlpPath is not set.");
            throw new InvalidOperationException("YtDlpPath is not set.");
        }
    
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = YtdlpPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };
    
        process.Start();
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
    
        await process.WaitForExitAsync();
    
        string output = await outputTask;
        string error = await errorTask;
        int exitCode = process.ExitCode;
        process.Close();

        return (output, error, exitCode);
    }
}