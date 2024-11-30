using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using CommandLine;


namespace bookmark_dlp
{
    public class CoreLogic
    {
        public static void CoreLogicMain(string[] args)
        {
#if DEBUG
            Logger.verbosity = Logger.Verbosity.Critical;
            // Functions.PrintToConsole(Import.SmartImport(Import.GetBrowserBookmarkFilesPaths()[0].foundProfiles[0]));
            Console.WriteLine("starting");
            Functions.PrintToConsole(Import.SmartImport("test1.html"));
            Console.WriteLine("\n\n\n");
            Functions.PrintToConsole(Import.SmartImport("test2.html"));
            Console.ReadKey();
            Environment.Exit(0);
#endif
            ParserResult<CommandLineOptions> commandLineOptions = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);
            CommandLineOptions setOptions = commandLineOptions.Value;
            int retu = ValidateCommandLineOptions(setOptions);
            if (retu == 1) { Environment.Exit(1); }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string rootdir = Directory.GetCurrentDirectory(); //current directory
            string localhtml = System.IO.Path.Combine(rootdir, "Bookmarks.html");

            List<Folderclass> folders = new List<Folderclass>();
            bool importSourceFound = false;
            bool ishtml = false;
            string filePath = ""; //if not html
            int totalyoutubelinksnumber;

            if (setOptions.Interactive)
            {
                ///======
                ///    Interactive
                ///======
                ///
                Logger.verbosity = Logger.Verbosity.Debug;
                bool downloadPlaylists = false;
                bool downloadShorts = false;
                bool downloadChannels = false;
                bool concurrent_downloads = false;
                bool cookies_autoextract = false;

                Logger.LogVerbose("Interactive CLI session");
                if (setOptions.HtmlFileLocation != null) { localhtml = setOptions.HtmlFileLocation; }

                if (File.Exists(localhtml))
                {
                    Logger.LogVerbose($"{localhtml} is the html import file");
                    importSourceFound = true;
                    ishtml = true;
                }
                else
                {
                    Logger.LogVerbose($"{localhtml} not found.");
                }
                if (!importSourceFound)
                {

                    Logger.LogVerbose("Choose import location.");
                    while (true)
                    {
                        Logger.LogVerbose("HTML file? Y/N");
                        string yn = Console.ReadLine();
                        yn = yn.Trim();
                        if (yn.ToLower() == "y" || yn.ToLower() == "n") { ishtml = (yn == "y") ? true : false; break; }
                    }
                    if (ishtml)
                    {
                        while (true)
                        {
                            Logger.LogVerbose("Source of html file? (path) eg.: /home/user/Desktop/mybookmarks.html");
                            localhtml = Console.ReadLine();
                            if (File.Exists(localhtml)) { Logger.LogVerbose($"{localhtml} is the html import file"); break; }
                            else { Logger.LogVerbose($"{localhtml} not found."); }
                        }
                    }
                    else
                    {
                        //not html
                        Logger.LogVerbose("No html set, proceeding with search in installed browser default locations"); //goig to autoimport, as no .html present
                        filePath = BrowserLocations.QueryChosenBookmarksFile(BrowserLocations.GetBrowserBookmarkFilesPaths());
                    }
                    //A source was chosen
                    importSourceFound = true;
                }

                (downloadPlaylists, downloadShorts, downloadChannels) = AppMethods.Wantcomplex();

                Logger.LogVerbose("Concurrent downloads? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { concurrent_downloads = true; }
                Logger.LogVerbose("Cookies autoextract? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { cookies_autoextract = true; }
                if (setOptions.Outputfolder == null)
                {
                    while (true)
                    {
                        Logger.LogVerbose($"Output folder? default: current directory {rootdir}");
                        string readOutputFolder = Console.ReadLine();
                        if (String.IsNullOrEmpty(readOutputFolder)) { setOptions.Outputfolder = rootdir; break; }
                        if (Directory.Exists(readOutputFolder)) { setOptions.Outputfolder = readOutputFolder; break; }
                        try { Directory.CreateDirectory(readOutputFolder); }
                        catch { Logger.LogVerbose($"Could not create directory {readOutputFolder}.", Logger.Verbosity.Error); }
                    }
                }

                string ytdlp_path = AppMethods.Yt_dlp_pathfinder(rootdir); //check if yt-dlp is in the root folder, on the path or not available
                if (String.IsNullOrEmpty(ytdlp_path)) { ytdlp_path = AppMethods.Yt_dlp_pathfinder(setOptions.Outputfolder); }
                if (String.IsNullOrEmpty(ytdlp_path))
                {
                    while (true)
                    {
                        Logger.LogVerbose("yt-dlp not found, add path now? Y/N");
                        if (Console.ReadKey().ToString().ToLower().Equals("n")) { Logger.LogVerbose("Cannnot continue", Logger.Verbosity.Error); Environment.Exit(2); }
                        Logger.LogVerbose("Choose path. e.g.: /usr/bin/yt-dlp");
                        string readYtdlpPath = Console.ReadLine();
                        if (Directory.Exists(readYtdlpPath)) { ytdlp_path = readYtdlpPath; }
                    }
                }
                //at this point all settings are set

                if (ishtml)
                {
                    filePath = localhtml;
                }
                if (!String.IsNullOrEmpty(filePath))
                {
                    // Intake for both html and sql and json
                    folders = Import.SmartImport(filePath);
                }
                else { Logger.LogVerbose("No source file!", Logger.Verbosity.Error); Environment.Exit(1); }
                //now import is finished
                Logger.LogVerbose("Import finished", Logger.Verbosity.Info);

                int deepestdepth = 0; //Finding the deepest folder depth
                for (int q = 0; q < folders.Count; q++)
                {
                    if (deepestdepth < folders[q].depth)
                    {
                        deepestdepth = folders[q].depth;
                    }
                }
                
                Functions.Createfolderstructure(ref folders, rootdir);
                totalyoutubelinksnumber = AutoImport.WritelinkstotxtFromFolderclasses(ref folders, rootdir, downloadPlaylists, downloadShorts, downloadChannels);
                
                Functions.PrintToConsole(folders); //dump all the folder info to console
                AppMethods.Scriptwriter(folders, ytdlp_path); //writing the scripts that call yt-dlp and add .txt with the links in the arguments //NOT the method that creates the .txt files
                AppMethods.Deleteemptyfolders(folders); //deletes the folders from the folder structure that are empty (no youtube links were written into them)
                Console.WriteLine("Running the scripts after ENTER.");
                Console.ReadKey();
                AppMethods.Runningthescripts(folders);
                //AppMethods.Checkformissing //checking if all the desired links have indeed been downloaded, archive.txt integrity as well
                Functions.PrintToConsole(folders);
                Console.WriteLine("Press enter to exit");
                Console.Read();
                Environment.Exit(0);
            }
            else
            {
                ///======
                ///    Non-Interactive
                ///======
                ///

                Logger.LogVerbose("Non-Interactive CLI session", Logger.Verbosity.Info);
                Logger.verbosity = Logger.Verbosity.Warning;

                bool downloadPlaylists = setOptions.DownloadPlaylists;
                bool downloadShorts = setOptions.DownloadShorts;
                bool downloadChannels = setOptions.DownloadChannels;
                bool concurrent_downloads = setOptions.Concurrent_downloads;
                bool cookies_autoextract = setOptions.Cookies_autoextract;

                if (setOptions.Outputfolder == null) { setOptions.Outputfolder = Directory.GetCurrentDirectory(); }
                if (setOptions.HtmlFileLocation != null) { localhtml = setOptions.HtmlFileLocation; }
                string ytdlp_path = AppMethods.Yt_dlp_pathfinder(rootdir);
                if (String.IsNullOrEmpty(ytdlp_path)) { ytdlp_path = AppMethods.Yt_dlp_pathfinder(setOptions.Outputfolder); }
                if (String.IsNullOrEmpty(ytdlp_path)) { Logger.LogVerbose("yt-dlp not found", Logger.Verbosity.Error); Environment.Exit(1); }

                if (File.Exists(localhtml))
                {
                    Logger.LogVerbose($"{localhtml} is the html import file", Logger.Verbosity.Info);
                    importSourceFound = true;
                    ishtml = true;
                }
                else
                {
                    // import from browser
                    // not html
                    Logger.LogVerbose("No html set, proceeding with search in installed browser default locations"); //goig to autoimport, as no .html present
                    throw new NotImplementedException();
                    //TODO: cannot as its interactive : filePath = AutoImport.QueryChosenBookmarksFile(AutoImport.FindBrowserBookmarkFilesPaths());
                }
                if (ishtml)
                {
                    filePath = localhtml;
                }
                else if (!String.IsNullOrEmpty(filePath))
                {
                    // Import for both json and sql and html
                    folders = Import.SmartImport(filePath);
                }
                else { throw new FileNotFoundException("No source file!"); }
                //now import is finished
                Logger.LogVerbose("Import finished", Logger.Verbosity.Debug);

                int deepestdepth = 0; //Finding the deepest folder depth
                for (int q = 0; q < folders.Count; q++)
                {
                    if (deepestdepth < folders[q].depth)
                    {
                        deepestdepth = folders[q].depth;
                    }
                }
                
                Functions.Createfolderstructure(ref folders, rootdir);
                totalyoutubelinksnumber = AutoImport.WritelinkstotxtFromFolderclasses(ref folders, rootdir, downloadPlaylists, downloadShorts, downloadChannels);
                
                
                /////////////////////////////////////////
                //TODO: totalyoutubelinknumbers
                /////////////////////////////////////////


                AppMethods.Scriptwriter(folders, ytdlp_path); //writing the scripts that call yt-dlp and add .txt with the links in the arguments //NOT the method that creates the .txt files
                AppMethods.Deleteemptyfolders(folders); //deletes the folders from the folder structure that are empty (no youtube links were written into them)
                AppMethods.Runningthescripts(folders);
                //Functions.Checkformissing
                Environment.Exit(0); //leaving the program, so it does not contiue running according to Program.cs
            }
        }


        private static int ValidateCommandLineOptions(CommandLineOptions options)
        {
            if (options == null) { return 1; }
            if (options.Interactive)
            {
                return 0; //if interactive the options don't matter
            }
            if (options.HtmlFileLocation != null)
            {
                if (!File.Exists(options.HtmlFileLocation)) { return 1; }
                if (Path.GetExtension(options.HtmlFileLocation) != ".html") { return 1; }
            }
            if (options.Yt_dlp_binary_path != null)
            {
                if (!File.Exists(options.Yt_dlp_binary_path)) { return 1; }
            }
            if (options.Outputfolder != null)
            {
                if (!Directory.Exists(options.HtmlFileLocation))
                {
                    try
                    {
                        Directory.CreateDirectory(options.Outputfolder);
                    }
                    catch (Exception)
                    {
                        Logger.LogVerbose("Could not create directory: " + options.Outputfolder, Logger.Verbosity.Error);
                        return 1;
                    }
                }
                return 0;
            }
            return 0;
        }


    }
}
