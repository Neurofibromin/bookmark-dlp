using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Web;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using NfLogger;

namespace bookmark_dlp
{
    public static class YtdlpInterfacing
    {
        public static string YtdlpPath;
        
        /// <summary>
        /// Gets list of video ids from a given channel. Requires internet.
        /// </summary>
        /// <param name="url">The FQDN of the channel. May be https://www.youtube.com/@SomeChannel or (...)/@SomeChannel/Videos</param>
        /// <returns>List of 11 char long youtube ids for the videos uploaded by the channel. Or null if channel does not exist or no internet.</returns>
        public static List<string>? GetVideoIdsFromChannelUrl(string url)
        {
            if (string.IsNullOrEmpty(YtdlpPath))
            {
                Logger.LogVerbose($"YtdlpPath is not set in YtdlpInterfacing::GetVideoIdsFromChannelUrl()", Logger.Verbosity.Error);
                return null;
            }
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            if (url.ToLower().EndsWith("/videos"))
                url = url.Substring(0, url.Length - "/videos".Length);
            string channelUrl = url;
            string ytDlpPath = YtdlpPath;
            List<string> videoIds = new List<string>();
            // Run yt-dlp
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = $"-j --flat-playlist \"{channelUrl}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            String error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            int exitcode = process.ExitCode;
            process.Close();
            if (exitcode != 0)
            {
                if (error.Contains("Unable to download webpage: HTTP Error 404") || error.Contains("Unable to download API page") || error.Contains("Failed to establish a new connection: [Errno -3]"))
                {
                    Logger.LogVerbose(error, Logger.Verbosity.Error);
                    return null;
                }
                else if (result.Contains("Unable to download webpage: HTTP Error 404") || result.Contains("Unable to download API page") || result.Contains("Failed to establish a new connection: [Errno -3]"))
                {
                    Logger.LogVerbose(result, Logger.Verbosity.Error);
                    return null;
                }
                else
                {
                    Logger.LogVerbose($"yt-dlp exit code: {exitcode}, failed to query videos for channel: {channelUrl}", Logger.Verbosity.Error);
                    return null;
                }
            }
            string[] lines = result.Split('\n');
            foreach (string line in lines)
            {
                try
                {
                    // Parse JSON for each video
                    JsonDocument jsonDoc = JsonDocument.Parse(line);
                    if (jsonDoc.RootElement.TryGetProperty("id", out JsonElement idElement))
                    {
                        videoIds.Add(idElement.GetString());
                    }
                    else
                    {
                        Logger.LogVerbose($"There was no id property in jsondoc: {line}", Logger.Verbosity.Warning);
                    }
                }
                catch (JsonException ex)
                {
                    Logger.LogVerbose($"Error parsing JSON: {ex.Message}", Logger.Verbosity.Error);
                }
            }
            return videoIds;
        }
        
        /// <summary>
        /// Extracts the playlist ID from a given YouTube URL.
        /// </summary>
        /// <param name="url">The YouTube URL, e.g., "https://www.youtube.com/watch?v=12345678912&amp;list=PL123456789123456789-4568789123&amp;index=1"</param>
        /// <returns>The playlist ID if found, otherwise null.</returns>
        public static string? ExtractPlaylistId(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            try
            {
                // Parse the URL
                Uri uri = new Uri(url);
                // Extract the query parameters
                string query = uri.Query;
                // Parse the query parameters into a dictionary
                var queryParameters = System.Web.HttpUtility.ParseQueryString(query);
                // Return the value of the "list" parameter if it exists
                return queryParameters["list"];
            }
            catch (Exception ex)
            {
                // Log error if necessary
                Logger.LogVerbose($"Failed to extract playlist ID from URL: {url}. Error: {ex.Message}", Logger.Verbosity.Error);
                return null;
            }
        }
        
        /// <summary>
        /// Gets a list of video IDs from a given playlist URL. Requires internet.
        /// </summary>
        /// <param name="url">The FQDN of the playlist. E.g., https://www.youtube.com/playlist?list=PL123456789123456789-4568789123</param>
        /// <returns>List of 11-character-long YouTube video IDs from the playlist. Returns null if the playlist does not exist or no internet is available.</returns>
        public static List<string>? GetVideoIdsFromPlaylistUrl(string url)
        {
            if (string.IsNullOrEmpty(YtdlpPath))
            {
                Logger.LogVerbose("YtdlpPath is not set in YtdlpInterfacing::GetVideoIdsFromPlaylistUrl()", Logger.Verbosity.Error);
                return null;
            }
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            string oldurl = url;
            if(ExtractPlaylistId(url) != null)
                url = "https://www.youtube.com/playlist?list=" + ExtractPlaylistId(url);
            else
            {
                Logger.LogVerbose($"Url was not Playlist: {url}", Logger.Verbosity.Warning);
                return null;
            }    
            Logger.LogVerbose($"Parsed playlist url from {oldurl} to {url}", Logger.Verbosity.Debug);
            
            List<string> videoIds = new List<string>();
            // Run yt-dlp
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = YtdlpPath,
                    Arguments = $"-j --flat-playlist \"{url}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Close();
            if (exitCode != 0)
            {
                if (error.Contains("Unable to download webpage: HTTP Error 404") || error.Contains("Unable to download API page") || error.Contains("Failed to establish a new connection: [Errno -3]"))
                {
                    Logger.LogVerbose(error, Logger.Verbosity.Error);
                    return null;
                }
                if (result.Contains("Unable to download webpage: HTTP Error 404") || result.Contains("Unable to download API page") || result.Contains("Failed to establish a new connection: [Errno -3]"))
                {
                    Logger.LogVerbose(result, Logger.Verbosity.Error);
                    return null;
                }
                Logger.LogVerbose($"yt-dlp exit code: {exitCode}, failed to query videos for playlist: {url}", Logger.Verbosity.Error);
                return null;
            }

            string[] lines = result.Split('\n');
            foreach (string line in lines)
            {
                try
                {
                    // Parse JSON for each video
                    JsonDocument jsonDoc = JsonDocument.Parse(line);
                    if (jsonDoc.RootElement.TryGetProperty("id", out JsonElement idElement))
                        videoIds.Add(idElement.GetString());
                    else
                        Logger.LogVerbose($"No 'id' property found in JSON: {line}", Logger.Verbosity.Error);
                }
                catch (JsonException ex)
                {
                    Logger.LogVerbose($"Error parsing JSON: {ex.Message}", Logger.Verbosity.Error);
                }
            }
            return videoIds;
        }

        /// <summary>
        /// Finds yt-dlp binary. Checks multiple places. More details in project Readme.
        /// </summary>
        /// <param name="rootdir">The rootdir where bookmark-dlp is called from.</param>
        /// <returns>String containing the yt-dlp binary filepath or NULL if not installed.</returns>
        public static string Yt_dlp_pathfinder(string rootdir = "")
        {
            string ytdlp_path = ""; //checks is yt-dlp binary is present in root or if it is on path, returns ytdlp_path so it can be written into the script files
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, "yt-dlp.exe")))
                {
                    //Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " found");
                    ytdlp_path = Path.Combine(rootdir, "yt-dlp.exe");
                }
                else
                {
                    //Console.WriteLine(Path.Combine(rootdir, "yt-dlp.exe") + " not found, searching PATH.");
                    //TODO: Windows path check for yt-dlp
                    // maybe already works? test.
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = "yt-dlp.exe";
                        proc.Start();
                        proc.WaitForExit();
                        ytdlp_path = "yt-dlp.exe";
                    }
                    // check into Win32Exceptions and their error codes!
                    catch (Win32Exception winEx)
                    {
                        if (winEx.NativeErrorCode == 2 || winEx.NativeErrorCode == 3)
                        {
                            // 2 => "The system cannot find the FILE specified."
                            // 3 => "The system cannot find the PATH specified."
                            Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Warning);
                            return null;
                        }
                        else
                        {
                            // unknown Win32Exception, re-throw to show the raw error msg
                            throw;
                        }
                    }
                    ytdlp_path = "yt-dlp.exe"; //no exception was thrown, so yt-dlp must be on the path
                }
                return ytdlp_path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string[] filenames = { "yt-dlp", "yt-dlp_linux", "yt-dlp_linux_aarch64", "yt-dlp_linux_armv7l" };
                foreach (string filename in filenames)
                {
                    // check for yt-dlp executable in working rootdir
                    if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                    {
                        Logger.LogVerbose("yt-dlp binary found at: " + Path.Combine(rootdir, filename), Logger.Verbosity.Debug);
                        ytdlp_path = Path.Combine(rootdir, filename);
                        break;
                    }
                }
                if (ytdlp_path == "")
                {
                    // check for yt-dlp binary on path
                    Logger.LogVerbose(Path.Combine(rootdir, "yt-dlp") + " not found, searching PATH.", Logger.Verbosity.Info);
                    string command = "-c \"command -v yt-dlp\"";
                    Process process = new Process();
                    // Console.WriteLine("command: " + command);
                    var processStartInfo = new ProcessStartInfo()
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
                    String error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    // Console.WriteLine("Result: " + result.Trim());
                    // Console.WriteLine("Error: " + error);
                    if (process.ExitCode != 0)
                    {
                        Logger.LogVerbose("yt-dlp not installed!", Logger.Verbosity.Warning);
                        return null;
                    }
                    else //yt-dlp is on the path
                    {
                        ytdlp_path = result.Trim();
                    }
                    // Console.WriteLine("ExitCode: {0}", process.ExitCode);
                    process.Close();
                }
                return ytdlp_path;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string[] filenames = { "yt-dlp", "yt-dlp_macos", "yt-dlp_macos_legacy" };
                foreach (string filename in filenames)
                {
                    if (!String.IsNullOrWhiteSpace(rootdir) && File.Exists(Path.Combine(rootdir, filename)))
                    {
                        // Console.WriteLine(Path.Combine(rootdir, filename) + " found");
                        ytdlp_path = Path.Combine(rootdir, filename);
                        break;
                    }
                }
                if (ytdlp_path == "")
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
    }
}