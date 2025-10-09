using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bookmark_dlp;
using NfLogger;

namespace Nfbookmark
{
    /// <summary>
    ///     Contains functions relating to the management of bookmark folders and logging
    /// </summary>
    public class Functions
    {
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
        /// <param name="wantOutputToLog">If true uses Nflogger.Log.Logverbose to print (as well)</param>
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
                    Logger.LogVerbose("No folders to display.");
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

            Logger.LogVerbose("The following folders were found");

            /* for (int m = 0; m < folders.Count; m++)
                Folderclass currentFolder = folders[m];
                Folderclass previousFolder;
                if (m > 0) { previousFolder = folders[m - 1]; } else { previousFolder = null; }*/

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
                    Logger.LogVerbose(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)));
                if (wantOutputToStream)
                    writer.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)));
                string write = $"{currentFolder.depth.ToString().PadRight(deepestdepthlength, '_')}" +
                               new string('_', deepestdepth - depthsymbolcounter) +
                               $" is the depth of {currentFolder.startline.ToString().PadLeft(maxstartlinelength, '_')}/{currentFolder.endingline.ToString().PadLeft(maxendlinelength, '_')} " +
                               $"[{currentFolder.name.Replace(' ', '_').PadRight(maxnamelength, '_')}] folder, which contains [{currentFolder.urls.Count.ToString().PadLeft(maxnumberoflinklength, '_')}] links. " +
                               $"id:\"{currentFolder.id.ToString().PadLeft(maxidlength, '_')}\" parentId:\"{currentFolder.parentId.ToString().PadLeft(maxidlength, '_')}";
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.ResetColor();
                if (wantOutputToLog)
                    Logger.LogVerbose(write);
                string[] words = write.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (word.StartsWith("[") || word.EndsWith("]"))
                    {
                        // Console.ForegroundColor = ConsoleColor.Red;
                        word = word.Replace("[", string.Empty);
                        word = word.Replace("]", string.Empty);
                    }

                    if (wantOutputToStream)
                        writer.Write(word.Replace('_', ' ') + " ");
                    // Console.Write();
                    // Console.ResetColor();
                }
                if (wantOutputToLog)
                    Logger.LogVerbose("");
                if (wantOutputToStream)
                    writer.Write("\n");
                previousFolder = currentFolder;
            }
            if (wantOutputToLog)
                Logger.LogVerbose("Alltogether " + folders.Count + " folders were found.");
            if (wantOutputToStream)
                writer.WriteLine("Alltogether " + folders.Count + " folders were found.");
            outputStream?.Flush();
        }

        #region FolderFunctions

        /// <summary>
        ///     Attempt to make sure all bookmark folder names are good for filesystems folder names. If needed, changes the
        ///     folder.name value<br />
        ///     Necessary because bookmark folders can have 1) empty names 2) the same names 3) contain not allowed characters or
        ///     character combinations<br />
        ///     Requires:
        ///     Name
        ///     Parent
        /// </summary>
        /// <param name="folders">The bookmark folders to operate on, folder names may be changed</param>
        public static void FoldernameValidation(List<Folderclass> folders)
        {
            string[] forbiddenCharacters = { "/", ":", "?", "<", ">", "*", "|", "\\", "\"" };
            // If name empty
            foreach (Folderclass folder in folders)
            {
                foreach (string ch in forbiddenCharacters)
                {
                    if (folder.name.Contains(ch))
                    {
                        string newfoldername = folder.name.Replace(ch, string.Empty);
                        Logger.LogVerbose($"foldername {folder.name} contained illegal character {ch}. New name:{newfoldername}", Logger.Verbosity.Warning);
                        folder.name = newfoldername;
                    }
                }
                if (folder.name.Trim().Replace(" ", string.Empty).Distinct().ToList().Count() == 1 && folder.name.Trim().Replace(" ", string.Empty).Distinct().ToList()[0] == '.') 
                {
                    // name is only made up of spaces and .
                    string newfoldername = $"ID{folder.id}";
                    Logger.LogVerbose($"foldername {folder.name} contained only spaces and periods. New name:{newfoldername}", Logger.Verbosity.Warning);
                    folder.name = newfoldername;
                }
                if (folder.name.StartsWith("."))
                {
                    string newfoldername = $"ID{folder.id}";
                    Logger.LogVerbose($"foldername {folder.name} started with period. New name:{newfoldername}", Logger.Verbosity.Warning);
                    folder.name = newfoldername;
                }
                if (String.IsNullOrWhiteSpace(folder.name))
                {
                    string newfoldername = $"ID{folder.id}";
                    Logger.LogVerbose($"foldername {folder.name} contained only spaces. New name:{newfoldername}", Logger.Verbosity.Warning);
                    folder.name = newfoldername;
                }
            }

            // If two folders have the same name and same parent (and same depth)
            for (int i = 0; i < folders.Count - 1; i++)
            {
                for (int j = i + 1; j < folders.Count; j++)
                {
                    if (string.Equals(folders[i].name, folders[j].name, StringComparison.CurrentCultureIgnoreCase) &&
                        folders[i].depth == folders[j].depth &&
                        folders[i].parentId == folders[j].parentId)
                    {
                        folders[j].name = folders[j].name + $"ID{folders[j].id}";
                        folders[i].name = folders[i].name + $"ID{folders[i].id}";
                    }
                }
            }
        }

        /// <summary>
        ///     Creating the folder structure on filesystems and storing the access paths to folders[].folderpath <br />
        ///     Requires:
        ///     <list type="bullet">
        ///         <item> Name </item>
        ///         <item> Depth </item>
        ///         <item> Parent </item>
        ///     </list>
        ///     Fills:
        ///     <list type="bullet">
        ///         <item> Folderpath </item>
        ///     </list>
        /// </summary>
        /// <param name="folders">Bookmark folders to be made into filesystem folders</param>
        /// <param name="rootdir">Fiilesystems directory to contain all the folders</param>
        public static void Createfolderstructure(List<Folderclass> folders, string rootdir)
        {
            FoldernameValidation(folders);
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);
            Directory.SetCurrentDirectory(rootdir);
            Directory.CreateDirectory("Bookmarks");
            Directory.SetCurrentDirectory("Bookmarks");
            string bookmarkroot = Directory.GetCurrentDirectory();
            string parentdir;
            List<Folderclass> ordered = new List<Folderclass>(folders);
            foreach (Folderclass folder in ordered.OrderBy(f => f.depth))
            {
                try
                {
                    if (folder.depth != 0 && folder.depth != folders[folder.parentId].depth + 1)
                    {
                        Logger.LogVerbose(
                            $"Depth of folder {folder.name} is {folder.depth}, not 1 more than its parent's {folders[folder.parentId].name} depth: {folders[folder.parentId].depth}", Logger.Verbosity.Error);
                        throw new InvalidDataException(
                            $"Depth of folder {folder.name} is {folder.depth}, not 1 more than its parent's {folders[folder.parentId].name} depth: {folders[folder.parentId].depth}");
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Logger.LogVerbose($"Folder has no parent? Folder name: {folder.name}, parent id: {folder.parentId}, number of folders: {folders.Count}", Logger.Verbosity.Error);
                    throw;
                }

                if (folder.depth == 0)
                {
                    parentdir = bookmarkroot;
                    Directory.CreateDirectory(Path.Combine(parentdir, folder.name));
                    folder.folderpath = Path.Combine(parentdir, folder.name); //path
                    Logger.LogVerbose($"Folderpath created for folder {folder.name} is {folder.folderpath}", Logger.Verbosity.Trace);
                }
                else
                {
                    try
                    {
                        parentdir = folders[folder.parentId].folderpath;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Logger.LogVerbose(
                            $"Folder has no parent? Folder name: {folder.name}, parent id: {folder.parentId}, number of folders: {folders.Count}",
                            Logger.Verbosity.Error);
                        throw;
                    }

                    if (!Directory.Exists(parentdir))
                    {
                        if (folder.depth <= folders[folder.parentId].depth)
                        {
                            Logger.LogVerbose(
                                $"Folder's parent has not lower depth than folder. Folder name: {folder.name}, depth: {folder.depth}," +
                                $" parent name: {folders[folder.parentId].name} parent depth: {folders[folder.parentId].depth}",
                                Logger.Verbosity.Error);
                        }

                        throw new InvalidDataException("Folder's parent directory does not exist. Folder: " +
                                                       folder.name +
                                                       " Parent: " + folders[folder.parentId].name + " Parent dir: " +
                                                       parentdir);
                    }

                    Directory.CreateDirectory(Path.Combine(parentdir, folder.name));
                    folder.folderpath = Path.Combine(parentdir, folder.name); //path
                    Logger.LogVerbose($"Folderpath created for folder {folder.name} is {folder.folderpath}",
                        Logger.Verbosity.Trace);
                }
            }

            /* Old method, did not take parent into account, assumed only the element in the list directly before the folder may be its parent
             for (int m = 0; m < folders.Count; m++)
            {
                if (m > 0)
                {
                    if (folders[m].depth > folders[m - 1].depth) //more depth than previous folder
                    {
                        Directory.SetCurrentDirectory(folders[m - 1].name);
                        Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                    if (folders[m].depth < folders[m - 1].depth) //less depth than the previous folder
                    {
                        for (int q = 0; q < (folders[m - 1].depth - folders[m].depth); q++) //the depth may have decreased by more than 1
                        {
                            Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".."));
                        }
                        Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                    if (folders[m].depth == folders[m - 1].depth) //the same depth as the previous folder
                    {
                        Directory.CreateDirectory(folders[m].name);
                        Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                        folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                        Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                    }

                }
                else //it is the first folder
                {
                    System.IO.Directory.CreateDirectory(folders[m].name);
                    Directory.SetCurrentDirectory(folders[m].name); //going into the folder
                    folders[m].folderpath = Directory.GetCurrentDirectory(); //path
                    Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "..")); //coming out of the folder
                }
            }*/
        }

        /// <summary>
        ///     Delete filesystem folders that are associated with bookmark folders if
        ///     1) filesystems folder has no files AND
        ///     2) filesystem folder has no folders <br />
        ///     Requires:
        ///     <list type="bullet">
        ///         <item> name </item>
        ///         <item> depth </item>
        ///         <item> parentId </item>
        ///         <item> folderpath </item>
        ///     </list>
        /// </summary>
        /// <param name="folders"></param>
        public static void Deleteemptyfolders(List<Folderclass> folders)
        {
            int a = 0;
            int deepestdepth = folders.Select(f => f.depth).Prepend(0).Max(); //Finding the deepest folder depth
            for (int q = deepestdepth; q > 0; q--) //deleting empty folders from the deepest layer upwards
            {
                foreach (Folderclass t in folders)
                {
                    if (t.depth == q) //only check folders with the given depth
                    {
                        bool thisfolderisempty = true;
                        string path = t.folderpath;
                        if (Directory.Exists(path))
                        {
                            if (Directory.GetDirectories(path).Length != 0) //check if the given directory has any children directories
                                thisfolderisempty = false;
                            if (Directory.GetFiles(t.folderpath).Length != 0) //check if the given directory has any files
                                thisfolderisempty = false;
                            if (thisfolderisempty)
                            {
                                Directory.Delete(t.folderpath);
                                a++;
                            }
                        }
                    }
                }
            }
            Logger.LogVerbose($"{a} empty folders deleted");
        }

        #endregion FolderFunctions
    }
}