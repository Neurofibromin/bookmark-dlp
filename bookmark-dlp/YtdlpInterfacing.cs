using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Web;
using bookmark_dlp.Models;
using NfLogger;
using Nfbookmark;

namespace bookmark_dlp;

public static class YtdlpInterfacing
{
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
            Logger.LogVerbose("YtdlpPath is not set in YtdlpInterfacing::ExecuteYtDlpProcess()",
                Logger.Verbosity.Error);
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
                Logger.LogVerbose(error, Logger.Verbosity.Error);
            }

            if (output.Contains("Unable to download webpage: HTTP Error 404") ||
                output.Contains("Unable to download API page") ||
                output.Contains("Failed to establish a new connection: [Errno -3]"))
            {
                Logger.LogVerbose(output, Logger.Verbosity.Error);
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
                Logger.LogVerbose($"yt-dlp exited with code {exitCode} for channel {url}. Error: {error}", Logger.Verbosity.Error);
                return null;
            }

            // Use LINQ for safer and more concise parsing
            List<string> lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(line => {
                             try { return JsonDocument.Parse(line); }
                             catch (JsonException ex) { 
                                 Logger.LogVerbose($"Error parsing JSON: {ex.Message} for line: {line}", Logger.Verbosity.Error);
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
            Logger.LogVerbose($"Failed to query channel {url}. Exception: {ex.Message}", Logger.Verbosity.Error);
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
            Logger.LogVerbose($"Failed to extract playlist ID from URL: {url}. Error: {ex.Message}", Logger.Verbosity.Error);
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
            Logger.LogVerbose($"URL does not contain a valid playlist ID: {url}", Logger.Verbosity.Warning);
            return null;
        }

        string playlistUrl = "https://www.youtube.com/playlist?list=" + playlistId;
        Logger.LogVerbose($"Parsed playlist url from {url} to {playlistUrl}", Logger.Verbosity.Debug);
        string output;
        try
        {
            string error;
            int exitCode;
            (output, error, exitCode) = ExecuteYtDlpProcess($"-j --flat-playlist \"{playlistUrl}\"");

            if (exitCode != 0)
            {
                Logger.LogVerbose($"yt-dlp exited with code {exitCode} for playlist {playlistUrl}. Error: {error}", Logger.Verbosity.Error);
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogVerbose($"Failed to query playlist {playlistUrl}. Exception: {ex.Message}", Logger.Verbosity.Error);
            return null;
        }

        try
        {
            // Use LINQ for safer and more concise parsing
            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => {
                    try { return JsonDocument.Parse(line); }
                    catch (JsonException ex) {
                        Logger.LogVerbose($"Error parsing JSON: {ex.Message} for line: {line}", Logger.Verbosity.Error);
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
            Logger.LogVerbose($"Failed to parse result of playlist query {playlistUrl}. Exception: {ex.Message}", Logger.Verbosity.Error);
            return null;
        }
    }

    /// <summary>
    ///     Finds yt-dlp binary. Checks multiple places. More details in project Readme.
    /// </summary>
    /// <param name="rootdir">The rootdir where bookmark-dlp is called from.</param>
    /// <param name="output_folder">The chosen output folder of bookmark-dlp</param>
    /// <returns>String containing the yt-dlp binary filepath or NULL if not installed.</returns>
    public static string? Yt_dlp_pathfinder(string? rootdir = "", string? output_folder = "")
    {
        string? ytdlp_path = null; //checks is yt-dlp binary is present in root or if it is on path, returns ytdlp_path so it can be written into the script files
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!string.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, "yt-dlp.exe")))
                ytdlp_path = Path.Combine(rootdir, "yt-dlp.exe");
            else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe")))
                ytdlp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            else if (!string.IsNullOrWhiteSpace(output_folder) &&
                     File.Exists(Path.Combine(output_folder, "yt-dlp.exe")))
                ytdlp_path = Path.Combine(output_folder, "yt-dlp.exe");
            else
            {
                //TODO: Windows path check for yt-dlp
                // maybe already works? test.
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = "yt-dlp.exe";
                    proc.Start();
                    proc.WaitForExit();
                    ytdlp_path = "yt-dlp.exe"; //if no exception was thrown, yt-dlp must be on the path
                }
                // check into Win32Exceptions and their error codes!
                catch (Win32Exception winEx)
                {
                    if (winEx.NativeErrorCode == 2 || winEx.NativeErrorCode == 3)
                    {
                        // 2 => "The system cannot find the FILE specified."
                        // 3 => "The system cannot find the PATH specified."
                        Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Warning);
                        ytdlp_path = null;
                    }
                    else
                    {
                        // unknown Win32Exception, re-throw to show the raw error msg
                        throw;
                    }
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" };
            foreach (string filename in filenames)
            {
                // check for yt-dlp executable in working rootdir
                if (!string.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                {
                    ytdlp_path = Path.Combine(rootdir, filename);
                    break;
                }

                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
                {
                    ytdlp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                    break;
                }

                if (!string.IsNullOrWhiteSpace(output_folder) && File.Exists(Path.Combine(output_folder, filename)))
                {
                    ytdlp_path = Path.Combine(output_folder, filename);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(ytdlp_path))
                Logger.LogVerbose("yt-dlp binary found at: " + ytdlp_path, Logger.Verbosity.Debug);
            else // string.IsNullOrEmpty(ytdlp_path)
            {
                // check for yt-dlp binary on path
                Logger.LogVerbose(Path.Combine(rootdir ?? "unkown_root", "yt-dlp") + " not found, searching PATH.",
                    Logger.Verbosity.Debug);
                string command = "-c \"command -v yt-dlp\"";
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "bash",
                    WorkingDirectory = rootdir,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = command
                };
                process.StartInfo = processStartInfo;
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Warning);
                    ytdlp_path = null;
                }
                else //yt-dlp is on the path
                    ytdlp_path = "yt-dlp";

                process.Close();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] filenames = { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
            foreach (string filename in filenames)
            {
                if (!string.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                {
                    ytdlp_path = Path.Combine(rootdir, filename);
                    break;
                }

                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
                {
                    ytdlp_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                    break;
                }

                if (!string.IsNullOrWhiteSpace(output_folder) && File.Exists(Path.Combine(output_folder, filename)))
                {
                    ytdlp_path = Path.Combine(output_folder, filename);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(ytdlp_path))
                Logger.LogVerbose("yt-dlp binary found at: " + ytdlp_path, Logger.Verbosity.Debug);
            else // string.IsNullOrEmpty(ytdlp_path)
            {
                throw new NotImplementedException();
                // TODO check OSX path for yt-dlp
                /*Console.WriteLine(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.");
                Console.WriteLine("Is it on the path? Y/N");
                if (Console.ReadLine().Contains("Y")) { ytdlp_path = "yt-dlp"; }
                else { throw new Exception($"yt-dlp not found in path or in rootdir, install it before continuing."); }*/
            }
        }
        return ytdlp_path;
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
                Logger.LogVerbose($"yt-dlp exited with code {exitCode} for size query on {link.url}. Error: {error}", Logger.Verbosity.Error);
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
            Logger.LogVerbose($"Failed to get estimated size for {link.url}. Exception: {ex.Message}", Logger.Verbosity.Error);
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
            Logger.LogVerbose("YtdlpPath is not set.", Logger.Verbosity.Error);
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