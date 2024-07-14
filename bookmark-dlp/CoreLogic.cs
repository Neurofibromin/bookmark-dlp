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
            ParserResult<CommandLineOptions> commandLineOptions = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);
            CommandLineOptions setOptions = commandLineOptions.Value;
            AppMethods.ValidateCommandLineOptions(setOptions);

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
                Methods.verbosity = Methods.Verbosity.debug;
                bool downloadPlaylists = false;
                bool downloadShorts = false;
                bool downloadChannels = false;
                bool concurrent_downloads = false;
                bool cookies_autoextract = false;

                Methods.LogVerbose("Interactive CLI session");
                if (setOptions.HtmlFileLocation != null) { localhtml = setOptions.HtmlFileLocation; }

                if (File.Exists(localhtml))
                {
                    Methods.LogVerbose($"{localhtml} is the html import file");
                    importSourceFound = true;
                    ishtml = true;
                }
                else
                {
                    Methods.LogVerbose($"{localhtml} not found.");
                }
                if (!importSourceFound)
                {

                    Methods.LogVerbose("Choose import location.");
                    while (true)
                    {
                        Methods.LogVerbose("HTML file? Y/N");
                        string yn = Console.ReadLine();
                        yn = yn.Trim();
                        if (yn.ToLower() == "y" || yn.ToLower() == "n") { ishtml = (yn == "y") ? true : false; break; }
                    }
                    if (ishtml)
                    {
                        while (true)
                        {
                            Methods.LogVerbose("Source of html file? (path) eg.: /home/user/Desktop/mybookmarks.html");
                            localhtml = Console.ReadLine();
                            if (File.Exists(localhtml)) { Methods.LogVerbose($"{localhtml} is the html import file"); break; }
                            else { Methods.LogVerbose($"{localhtml} not found."); }
                        }
                    }
                    else
                    {
                        //not html
                        Methods.LogVerbose("No html set, proceeding with search in installed browser default locations"); //goig to autoimport, as no .html present
                        filePath = Import.QueryChosenBookmarksFile(Import.GetBrowserBookmarkFilesPaths());
                    }
                    //A source was chosen
                    importSourceFound = true;
                }

                (downloadPlaylists, downloadShorts, downloadChannels) = AppMethods.Wantcomplex();

                Methods.LogVerbose("Concurrent downloads? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { concurrent_downloads = true; }
                Methods.LogVerbose("Cookies autoextract? Y/N");
                if (Console.ReadKey().ToString().ToLower().Equals("y")) { cookies_autoextract = true; }
                if (setOptions.Outputfolder == null)
                {
                    while (true)
                    {
                        Methods.LogVerbose($"Output folder? default: current directory {rootdir}");
                        string readOutputFolder = Console.ReadLine();
                        if (String.IsNullOrEmpty(readOutputFolder)) { setOptions.Outputfolder = rootdir; break; }
                        if (Directory.Exists(readOutputFolder)) { setOptions.Outputfolder = readOutputFolder; break; }
                        try { Directory.CreateDirectory(readOutputFolder); }
                        catch { Methods.LogVerbose($"Could not create directory {readOutputFolder}.", Methods.Verbosity.error); }
                    }
                }

                string ytdlp_path = AppMethods.Yt_dlp_pathfinder(rootdir); //check if yt-dlp is in the root folder, on the path or not available
                if (String.IsNullOrEmpty(ytdlp_path)) { ytdlp_path = AppMethods.Yt_dlp_pathfinder(setOptions.Outputfolder); }
                if (String.IsNullOrEmpty(ytdlp_path))
                {
                    while (true)
                    {
                        Methods.LogVerbose("yt-dlp not found, add path now? Y/N");
                        if (Console.ReadKey().ToString().ToLower().Equals("n")) { Methods.LogVerbose("Cannnot continue", Methods.Verbosity.error); Environment.Exit(2); }
                        Methods.LogVerbose("Choose path. e.g.: /usr/bin/yt-dlp");
                        string readYtdlpPath = Console.ReadLine();
                        if (Directory.Exists(readYtdlpPath)) { ytdlp_path = readYtdlpPath; }
                    }
                }
                //at this point all settings are set

                if (ishtml)
                {
                    //html intake
                    folders = Import.HtmlTakeoutIntake(localhtml); //TODO: Use SmartImport instead
                }
                else if (!String.IsNullOrEmpty(filePath))
                {
                    //not html AutoImport
                    folders = Import.SmartImport(filePath);
                }
                else { Methods.LogVerbose("No source file!", Methods.Verbosity.error); Environment.Exit(1); }
                //now import is finished
                Methods.LogVerbose("Import finished", Methods.Verbosity.info);

                int deepestdepth = 0; //Finding the deepest folder depth
                for (int q = 0; q < folders.Count; q++)
                {
                    if (deepestdepth < folders[q].depth)
                    {
                        deepestdepth = folders[q].depth;
                    }
                }
                
                AutoImport.Createfolderstructure(ref folders, rootdir);
                totalyoutubelinksnumber = AutoImport.WritelinkstotxtFromFolderclasses(ref folders, rootdir, downloadPlaylists, downloadShorts, downloadChannels);
                
                AppMethods.Dumptoconsole(folders); //dump all the folder info to console
                AppMethods.Scriptwriter(folders, ytdlp_path); //writing the scripts that call yt-dlp and add .txt with the links in the arguments //NOT the method that creates the .txt files
                AppMethods.Deleteemptyfolders(folders); //deletes the folders from the folder structure that are empty (no youtube links were written into them)
                Console.WriteLine("Running the scripts after ENTER.");
                Console.ReadKey();
                AppMethods.Runningthescripts(folders);
                //Methods.Checkformissing //checking if all the desired links have indeed been downloaded, archive.txt integrity as well
                AppMethods.Dumptoconsole(folders);
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

                Methods.LogVerbose("Non-Interactive CLI session", Methods.Verbosity.info);
                Methods.verbosity = Methods.Verbosity.warning;

                bool downloadPlaylists = setOptions.DownloadPlaylists;
                bool downloadShorts = setOptions.DownloadShorts;
                bool downloadChannels = setOptions.DownloadChannels;
                bool concurrent_downloads = setOptions.Concurrent_downloads;
                bool cookies_autoextract = setOptions.Cookies_autoextract;

                if (setOptions.Outputfolder == null) { setOptions.Outputfolder = Directory.GetCurrentDirectory(); }
                if (setOptions.HtmlFileLocation != null) { localhtml = setOptions.HtmlFileLocation; }
                string ytdlp_path = AppMethods.Yt_dlp_pathfinder(rootdir);
                if (String.IsNullOrEmpty(ytdlp_path)) { ytdlp_path = AppMethods.Yt_dlp_pathfinder(setOptions.Outputfolder); }
                if (String.IsNullOrEmpty(ytdlp_path)) { Methods.LogVerbose("yt-dlp not found", Methods.Verbosity.error); Environment.Exit(1); }

                if (File.Exists(localhtml))
                {
                    Methods.LogVerbose($"{localhtml} is the html import file", Methods.Verbosity.info);
                    importSourceFound = true;
                    ishtml = true;
                }
                else
                {
                    // import from browser
                    // not html
                    Methods.LogVerbose("No html set, proceeding with search in installed browser default locations"); //goig to autoimport, as no .html present
                    throw new NotImplementedException();
                    //TODO: cannot as its interactive : filePath = AutoImport.QueryChosenBookmarksFile(AutoImport.FindBrowserBookmarkFilesPaths());
                }
                if (ishtml)
                {
                    //html intake
                    folders = Import.HtmlTakeoutIntake(localhtml); //TODO: use smartimport instead
                }
                else if (!String.IsNullOrEmpty(filePath))
                {
                    //not html => AutoImport
                    folders = Import.SmartImport(filePath);
                }
                else { throw new FileNotFoundException("No source file!"); }
                //now import is finished
                Methods.LogVerbose("Import finished", Methods.Verbosity.debug);

                int deepestdepth = 0; //Finding the deepest folder depth
                for (int q = 0; q < folders.Count; q++)
                {
                    if (deepestdepth < folders[q].depth)
                    {
                        deepestdepth = folders[q].depth;
                    }
                }
                
                AutoImport.Createfolderstructure(ref folders, rootdir);
                totalyoutubelinksnumber = AutoImport.WritelinkstotxtFromFolderclasses(ref folders, rootdir, downloadPlaylists, downloadShorts, downloadChannels);
                
                
                /////////////////////////////////////////
                //TODO: totalyoutubelinknumbers
                /////////////////////////////////////////


                AppMethods.Scriptwriter(folders, ytdlp_path); //writing the scripts that call yt-dlp and add .txt with the links in the arguments //NOT the method that creates the .txt files
                AppMethods.Deleteemptyfolders(folders); //deletes the folders from the folder structure that are empty (no youtube links were written into them)
                AppMethods.Runningthescripts(folders);
                //Methods.Checkformissing
                Environment.Exit(0); //leaving the program, so it does not contiue running according to Program.cs
            }
        }
    }
}
