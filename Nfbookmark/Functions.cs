using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace bookmark_dlp
{
    /// <summary>
    /// Contains functions relating to the management of bookmark folders and logging
    /// </summary>
    public class Functions
    {

        /// <summary>
        /// Pretty prints the folder structure to console
        /// </summary>
        /// <param name="folders">The folder structure to be printed</param>
        public static void PrintToConsole(List<Folderclass> folders)
        {
            if (folders == null || folders.Count == 0)
            {
                Console.WriteLine("No folders to display.");
                return;
            }

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
                maxnumberoflinklength = Math.Max(maxnumberoflinklength, folder.numberoflinks.ToString().Length);
                maxidlength = Math.Max(maxidlength, folder.id.ToString().Length);
            }

            Console.WriteLine("The following folders were found");

            /* for (int m = 0; m < folders.Count; m++)
                Folderclass currentFolder = folders[m];
                Folderclass previousFolder;
                if (m > 0) { previousFolder = folders[m - 1]; } else { previousFolder = null; }*/
            
            int depthsymbolcounter = 0;
            Folderclass previousFolder = null;
            foreach (Folderclass folder in folders.OrderBy(a => a.id).ToList()) //writing the depth, the starting line, the ending line, name, and number of links of all the folders
            {
                /*Console.WriteLine(folder.id + " " + folders.IndexOf(folder));
                continue;*/
                Folderclass currentFolder = folder;
                
                int m = folder.id;
                //if (m>0) { previousFolder = folders.Single(a => a.id == m-1); } else { previousFolder = null; }

                if (previousFolder != null)
                {
                    if (currentFolder.depth > previousFolder.depth) //greater depth than before
                    {
                        depthsymbolcounter = depthsymbolcounter + (currentFolder.depth - previousFolder.depth);
                    }
                    if (currentFolder.depth < previousFolder.depth) //lesser depth than before
                    {
                        depthsymbolcounter = depthsymbolcounter - (previousFolder.depth - currentFolder.depth);
                    }
                    if (currentFolder.depth == previousFolder.depth) //same depth as before
                    {
                        //depthsymbolcounter does not change
                    }
                }               // string.Concat(Enumerable.Repeat("_", Math.Abs(depthsymbolcounter - deepestdepthlength)))
                else { } //at first folder the depth does not change
                Console.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)));
                // Console.Write("-" + depthsymbolcounter.ToString());
                string write = $"{currentFolder.depth.ToString().PadRight(deepestdepthlength, '_')}" + new string('_', deepestdepth - depthsymbolcounter) +
                    $" is the depth of {currentFolder.startline.ToString().PadLeft(maxstartlinelength, '_')}/{currentFolder.endingline.ToString().PadLeft(maxendlinelength, '_')} " +
                    $"[{currentFolder.name.Replace(' ', '_').PadRight(maxnamelength, '_')}] folder, which contains [{currentFolder.numberoflinks.ToString().PadLeft(maxnumberoflinklength, '_')}] links. " +
                    $"id:\"{currentFolder.id.ToString().PadLeft(maxidlength, '_')}\" parent:\"{currentFolder.parent.ToString().PadLeft(maxidlength, '_')}";
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.ResetColor();


                string[] words = write.Split(' ');

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (word.StartsWith("[") || word.EndsWith("]"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        word = word.Replace("[", string.Empty);
                        word = word.Replace("]", string.Empty);
                    }

                    Console.Write(word.Replace('_', ' ') + " ");
                    Console.ResetColor();
                }
                Console.WriteLine();
                previousFolder = currentFolder;
            }
            Console.WriteLine("Alltogether " + folders.Count + " folders were found.");
            /*if (totalyoutubelinknumber != 0)
            {
                Console.WriteLine(totalyoutubelinknumber + " youtube links were found, written into " + folders.Count + " folders.");
            }*/
        }

        /// <summary>
        /// Attempt to make sure all bookmark folder names are good for filesystems folder names. If needed, changes the folder.name value<br/>
        /// Neccessary because bookmark folders can have 1) empty names 2) the same names 3) contain not allowed characters or character combinations
        /// </summary>
        /// <param name="folders">The bookmark folders to operate on</param>
        public static void FoldernameValidation(ref List<Folderclass> folders)
        {
            string[] forbiddencharacters = { "/", ":", "?", "<", ">", "*", "|" , "\\" , "\"" };
            // If name empty
            foreach (Folderclass folder in folders)
            {
                foreach (string ch in forbiddencharacters)
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

            int deepestdepth = 0; //Finding the deepest folder depth
            for (int q = 0; q < folders.Count; q++)
            {
                if (deepestdepth < folders[q].depth)
                {
                    deepestdepth = folders[q].depth;
                }
            }
            // If two folders have the same name and same depth and same parent
            for (int depth = 0; depth < deepestdepth + 1; depth++)
            {
                for (int i = 0; i < folders.Count; i++)
                {
                    for (int j = 0; j < folders.Count; j++)
                    {
                        if (folders[i].name.ToLower() == folders[j].name.ToLower() && folders[i].depth == folders[j].depth && folders[i].parent == folders[j].parent)
                        {
                            folders[j].name = folders[j].name + $"ID{folders[j].id}";
                            folders[i].name = folders[i].name + $"ID{folders[i].id}";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creating the folder structure on filesystems and storing the access paths to folders[].folderpath
        /// </summary>
        /// <param name="folders">Bookmark folders to be made into filesystem folders</param>
        /// <param name="rootdir">Fiilesystems directory to contain all the folders</param>
        public static void Createfolderstructure(ref List<Folderclass> folders, string rootdir)
        {
            FoldernameValidation(ref folders);
            if (!Directory.Exists(rootdir)) { Directory.CreateDirectory(rootdir); }
            Directory.SetCurrentDirectory(rootdir);
            System.IO.Directory.CreateDirectory("Bookmarks");
            Directory.SetCurrentDirectory("Bookmarks");
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
            }
        }
    }
}
