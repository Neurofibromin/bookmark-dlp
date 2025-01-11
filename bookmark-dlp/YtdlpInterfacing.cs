using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NfLogger;

namespace bookmark_dlp
{
    public static class YtdlpInterfacing
    {
        public static string ytdlppath = AppMethods.Yt_dlp_pathfinder();
        
        public static void SetYtdlpPath(string rootdir)
        {
            ytdlppath = AppMethods.Yt_dlp_pathfinder(rootdir);
            Logger.LogVerbose("Ytdlp path in YtdlpInterfacing set to: " + ytdlppath, Logger.Verbosity.Trace);
        }
        
        /// <summary>
        /// Gets list of video ids from a given channel. Requires internet.
        /// </summary>
        /// <param name="url">The FQDN of the channel. May be https://www.youtube.com/@SomeChannel or (...)/@SomeChannel/Videos</param>
        /// <returns>List of 11 char long youtube ids for the videos uploaded by the channel. Or null if channel does not exist or no internet.</returns>
        public static List<string>? GetVideoIdsFromChannelUrl(string url)
        {
            string channelUrl = url;
            string ytDlpPath = ytdlppath;
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
        public static List<string> GetVideoIdsFromPlaylistUrl(string url)
        {
            throw new NotImplementedException();
        }

        public static void Testing()
        {
            List<string> myursl = YtdlpInterfacing.GetVideoIdsFromChannelUrl("somechannelurl");
            if (myursl != null)
            {
                foreach (string s in myursl)
                {
                    Logger.LogVerbose(s, Logger.Verbosity.Critical);
                }
            }
            else
            {
                Logger.LogVerbose("No video found", Logger.Verbosity.Critical);
            }
            Logger.LogVerbose("num: " + myursl.Count, Logger.Verbosity.Critical);
        }
    }
}