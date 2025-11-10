using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Nfbookmark
{
    public static class Legacy
    {
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(Legacy));

        /// <summary>
        ///     Legacy code, but still used. Basically globaly variables but in a struct that is passed to functions as necessary.
        /// </summary>
        public struct GlobalState
        {
            public int folderid; //used a lot instead of numberoffolders, maybe not ideal?
            public int endingline;
        }
        
        /// <summary>
        ///     Pretty prints the folder structure to selected output.
        ///     Can print to Log, arbitrary Stream or StdOut.
        ///     If Log is selected and no stream is given, then will not print to StdOut.
        ///     If Log not selected and no stream is given, then will print to StdOut.<br />
        ///     Requires:
        ///     Name
        ///     Depth
        ///     Startline
        ///     Endingline
        ///     urls
        ///     Parent
        ///     Id
        /// </summary>
        /// <param name="folders">The folder structure to be printed</param>
        /// <param name="wantOutputToLog">If true uses Serilog to print (as well)</param>
        /// <param name="outputStream">The stream to print to</param>
        public static void PrintToStream(List<Folderclass> folders, bool wantOutputToLog = false, Stream outputStream = null)
        {
            bool wantOutputToStream = outputStream != null;
            StreamWriter writer = null;
            if (outputStream != null)
                writer = new StreamWriter(outputStream);
            if (outputStream == null && !wantOutputToLog)
            {
                outputStream = Console.OpenStandardOutput();
                writer = new StreamWriter(outputStream);
                wantOutputToStream = true;
            }

            if (folders == null || folders.Count == 0)
            {
                if (wantOutputToLog)
                    Log.Information("No folders to display.");
                if (wantOutputToStream)
                    writer.WriteLine("No folders to display.");
                return;
            }

            // int deepestdepth = folders.Select(t => t.depth).Prepend(0).Max(); //Finding the deepest folder depth
            // int maxnamelength = folders.Select(t => t.name.Length).Prepend(0).Max();
            // int maxidlength = folders.Select(folder => folder.id.ToString().Length).Max();
            int deepestdepth = 0;
            int deepestdepthlength = 0;
            int maxstartlinelength = 0;
            int maxendlinelength = 0;
            int maxnamelength = 0;
            int maxnumberoflinklength = 0;
            int maxidlength = 0;
            
            // Calculate the maximum lengths for formatting
            foreach (Folderclass folder in folders)
            {
                deepestdepth = Math.Max(deepestdepth, folder.depth);
                deepestdepthlength = Math.Max(deepestdepthlength, folder.depth.ToString().Length);
                maxstartlinelength = Math.Max(maxstartlinelength, folder.startline.ToString().Length);
                maxendlinelength = Math.Max(maxendlinelength, folder.endingline.ToString().Length);
                maxnamelength = Math.Max(maxnamelength, folder.name.Length);
                maxnumberoflinklength = Math.Max(maxnumberoflinklength, folder.urls.Count.ToString().Length);
                maxidlength = Math.Max(maxidlength, folder.id.ToString().Length);
            }

            if (wantOutputToLog)
                Log.Information("The following folders were found");

            int depthsymbolcounter = 0;
            Folderclass previousFolder = null;
            List<Folderclass> sorted = new List<Folderclass>(folders);
            foreach (Folderclass folder in sorted.OrderBy(a => a.id).ToList()) //writing the depth, the starting line, the ending line, name, and number of links of all the folders
            {
                Folderclass currentFolder = folder;
                int m = folder.id;
                //if (m>0) { previousFolder = folders.Single(a => a.id == m-1); } else { previousFolder = null; }

                if (previousFolder != null)
                {
                    if (currentFolder.depth > previousFolder.depth) //greater depth than before
                        depthsymbolcounter = depthsymbolcounter + (currentFolder.depth - previousFolder.depth);
                    if (currentFolder.depth < previousFolder.depth) //lesser depth than before
                        depthsymbolcounter = depthsymbolcounter - (previousFolder.depth - currentFolder.depth);
                    if (currentFolder.depth == previousFolder.depth) //same depth as before
                    {
                        //depthsymbolcounter does not change
                    }
                } // string.Concat(Enumerable.Repeat("_", Math.Abs(depthsymbolcounter - deepestdepthlength)))

                //at first folder the depth does not change
                if (wantOutputToLog)
                    Log.Information("{Indent}", new string('-', depthsymbolcounter));
                if (wantOutputToStream)
                    writer.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)));
                    
                string write = $"{currentFolder.depth.ToString().PadRight(deepestdepthlength, '_')}" +
                               new string('_', deepestdepth - depthsymbolcounter) +
                               $" is the depth of {currentFolder.startline.ToString().PadLeft(maxstartlinelength, '_')}/{currentFolder.endingline.ToString().PadLeft(maxendlinelength, '_')} " +
                               $"[{currentFolder.name.Replace(' ', '_').PadRight(maxnamelength, '_')}] folder, which contains [{currentFolder.urls.Count.ToString().PadLeft(maxnumberoflinklength, '_')}] links. " +
                               $"id:\"{currentFolder.id.ToString().PadLeft(maxidlength, '_')}\" parentId:\"{currentFolder.parentId.ToString().PadLeft(maxidlength, '_')}";
                
                if (wantOutputToLog)
                    Log.Information("{FolderDetails}", write);
                    
                string[] words = write.Split(' ');
                foreach (string word in words)
                {
                    string cleanedWord = word.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (wantOutputToStream)
                        writer.Write(cleanedWord.Replace('_', ' ') + " ");
                }
                
                if (wantOutputToStream)
                    writer.Write("\n");
                    
                previousFolder = currentFolder;
            }
            if (wantOutputToLog)
                Log.Information("Altogether {FolderCount} folders were found.", folders.Count);
            if (wantOutputToStream)
                writer.WriteLine("Altogether " + folders.Count + " folders were found.");
                
            outputStream?.Flush();
        }
    }
}