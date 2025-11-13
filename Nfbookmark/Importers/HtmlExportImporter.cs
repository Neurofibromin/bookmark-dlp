using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Nfbookmark.Importers
{
    /// <summary>
    /// Imports bookmarks from a browser-exported HTML file.
    /// </summary>
    public class HtmlExportImporter : IBookmarkImporter
    {
        private readonly ILogger Log = Serilog.Log.ForContext<HtmlExportImporter>();

        /// <summary>
        ///     Intake of bookmarks and bookmarkfolders from a browser exported Html file <br />
        ///     Fills:
        ///     <list type="bullet">
        ///         <item> name </item>
        ///         <item> id </item>
        ///         <item> urls </item>
        ///         <item> depth </item>
        ///         <item> endingline </item>
        ///         <item> startline </item>
        ///     </list>
        /// </summary>
        /// <param name="filePath">Html file location</param>
        /// <returns>List of folders that have all their url bookmarks as children</returns>
        public List<Folderclass> Import(string filePath)
        {
            // read .html
            string[] inputarray;
            int lineCount;
            try
            {
                // StreamReader reader = new StreamReader(htmlfilepath); //read the file containing all the bookmarks - a single file using chrome export
                lineCount = File.ReadLines(filePath).Count(); //how many lines are there in the file - max number of bookmarks
                inputarray = File.ReadAllLines(filePath); //read whole file into inputarray[] array
            }
            catch (FileLoadException ex) { Log.Error(ex, "Html file could not be accessed."); return null; }
            catch (FileNotFoundException ex) { Log.Error(ex, "Html file was not found."); return null; }
            catch (IOException ex) { Log.Error(ex, "Html file IOException."); return null; }

            List<Folderclass> folders = new List<Folderclass>();

            { // scope just for variable visibility
                // Finding all the lines starting with dt h3 (these lines start every folder) and adding the number of these lines (j) to the object array folders[].startline
                // the folders[].startline gives us the number of the first line of the given folder in the inputarray[] array (in is like the endingline in the next loop, just for the start)
                // This also gives us the number of folders (numberoffolders)
                string[] line = new string[1000]; // limitation: there cannot be a line/bookmark with more than 1000 spaces in it. Probably not relevant?
                int numberoffolders = 0;
                for (int j = 0; j < lineCount; j++)
                {
                    line = inputarray[j].Trim().Split(' ');
                    if (line[0].Trim() == "<DT><H3")
                    {
                        Folderclass currentfolder = new Folderclass();
                        currentfolder.startline = j;
                        currentfolder.id = numberoffolders;
                        folders.Add(currentfolder);
                        numberoffolders++;
                    }
                }
            }
            Log.Debug("{FolderCount} folders were found in the bookmarks", folders.Count);

            // Finding the end of the folders (</DL><p>) and adding the line number to the object array (folders[].endingline)
            // Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                string oneline = inputarray[j].Trim();
                if (oneline == "</DL><p>") // if we find a line that ends a folder
                {
                    for (int m = folders.Count - 1; m >= 0; m--)
                    {
                        if (folders[m].startline < j && folders[m].endingline == 0) //finding the last folder that has a starting line earlier than this endingline, and has not yet been closed
                        {
                            folders[m].endingline = j;
                            break;
                            //break is necessary, because in embedded folders not only the correct folder's startline would be found correct, but all the not-yet closed folders that are already open: all their parent folders
                            //the break prevents parent folders getting the same endingline as their children
                        }
                    }
                }
            }

            // Finding the folder names and adding them to the object array (folders[].name)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Trim().Split('>');
                int whereisthechar = line[line.Length - 2].IndexOf("<");
                folders[m].name = line[line.Length - 2].Substring(0, whereisthechar);
                //Console.WriteLine(line[line.Length-2].Substring(0,whereisthechar));
                //Console.WriteLine(folders[m].startline + " " + folders[m].name);
                //Console.WriteLine(folders[m] + " line " + whereisthechar + " " + line[line.Length - 2]);
            }

            // Finding the folder depths (how embedded they are) and adding them to the object array (folders[].depth)
            for (int m = 0; m < folders.Count; m++)
            {
                string[] line = inputarray[folders[m].startline].Split('<');
                folders[m].depth = line[0].Length / 8 + 1;
            }

            // Add links to their folder
            for (int k = 0; k < folders.Count; k++)
            {
                //google side bug of duplicating all folders and bookmarks, resulting in 3 line long empty folders as well as not empty folders, which contain two copies of every bookmark.
                //shouldn't have too much of an effect on the end results,
                //just divide most numbers by 2. yt-dlp already uses archive.txt, so only lookup time is wasted, not downloads
                //Console.WriteLine(folders[j].name + " " + folders[j].endingline + " " + folders[j].startline + " " + (folders[j].endingline - folders[j].startline));
                if (folders[k].endingline - folders[k].startline > 2)
                {
                    for (int lineindex = folders[k].startline; lineindex < folders[k].endingline + 1; lineindex++) //going through all the lines that are in the given folder
                    {
                        if (inputarray[lineindex] != null)
                        {
                            string[] line = inputarray[lineindex].Trim().Split(' ');
                            if (line[0].Trim() == "<DT><A")
                            {
                                string link = line[1].Trim().Substring(6, line[1].Trim().Length - 7);
                                folders[k].urls.Add(link);
                            }
                        }
                    }
                }
                else
                {
                    Log.Warning("Folder {FolderName} with id {FolderId} has less than 2 lines. Start {StartLine} end {EndLine}", folders[k].name, folders[k].id, folders[k].startline, folders[k].endingline);
                }
            }
            return folders;
        }
    }
}