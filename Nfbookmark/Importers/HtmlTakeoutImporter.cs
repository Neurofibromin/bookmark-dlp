using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Nfbookmark.Importers
{
    /// <summary>
    /// Imports bookmarks from a Google Takeout HTML file.
    /// </summary>
    public class HtmlTakeoutImporter : IBookmarkImporter
    {
        private readonly ILogger Log = Serilog.Log.ForContext<HtmlTakeoutImporter>();

        /// <summary>
        ///     Intake of bookmarks and bookmarkfolders from a Google Takeout Html file <br />
        ///     Fills:
        ///     <list type="bullet">
        ///         <item> name </item>
        ///         <item> id </item>
        ///         <item> urls </item>
        ///         <item> depth </item>
        ///         <item> endingline </item>
        ///         <item> StartLine </item>
        ///     </list>
        /// </summary>
        /// <param name="filePath">Html file location</param>
        /// <returns>List of folders that have all their url bookmarks as children</returns>
        public List<ImportedFolder> UnsafeImport(string filePath)
        {
            // read .html
            string[] inputarray;
            int lineCount;
            try
            {
                lineCount = File.ReadLines(filePath).Count(); //how many lines are there in the file - max number of bookmarks
                inputarray = File.ReadAllLines(filePath); //read whole file into inputarray[] array
            }
            catch (FileLoadException ex) { Log.Error(ex, "Html file could not be accessed: {FilePath}", filePath); return null; }
            catch (FileNotFoundException ex) { Log.Error(ex, "Html file not found: {FilePath}", filePath); return null; }
            catch (IOException ex) { Log.Error(ex, "An IO exception occurred while reading the HTML file: {FilePath}", filePath); return null; }

            Log.Information("The html file {FilePath} is being treated as a Google Takeout file", filePath);
            Log.Debug("{ReadLines}/{TotalLines} lines were read", inputarray.Length, lineCount);
            Log.Debug("HTML intake has finished!");
            
            var parsedFolders = new List<HtmlParseData>();

            { // scope just for variable visibility
                // Finding all the lines starting with dt h3 (these lines start every folder) 
                // folders[].StartLine: the number of the first line of the given folder in the inputarray[] array
                // Find starting lines and create initial objects
                for (int j = 0; j < lineCount; j++)
                {
                    if (inputarray[j].Trim().StartsWith("<DT><H3"))
                    {
                        parsedFolders.Add(new HtmlParseData
                        {
                            Id = parsedFolders.Count,
                            StartLine = j
                        });
                    }
                }
            }
            Log.Debug("{FolderCount} folders were found in the bookmarks", parsedFolders.Count);

            // Finding the end of the folders (</DL><p>)
            // Counting the lines from the start while the folders from the back, so even in folders embedded into folders the endingline will be correct
            for (int j = 1; j < lineCount; j++)
            {
                if (inputarray[j].Trim() == "</DL><p>")
                {
                    for (int m = parsedFolders.Count - 1; m >= 0; m--)
                    {
                        if (parsedFolders[m].StartLine < j && parsedFolders[m].EndLine == 0) //finding the last folder that has a starting line earlier than this endingline, and has not yet been closed
                        {
                            parsedFolders[m].EndLine = j;
                            break;
                            //break is necessary, because in embedded folders not only the correct folder's startline would be found correct, but all the not-yet closed folders that are already open: all their parent folders
                            //the break prevents parent folders getting the same endingline as their children
                        }
                    }
                }
            }
            
            // 3. Populate the rest of the data on the temporary objects.
            foreach (var pFolder in parsedFolders)
            {
                // Finding names
                string[] line = inputarray[pFolder.StartLine].Trim().Split('>');
                int whereisthechar = line[line.Length - 2].IndexOf("<"); //TODO: change this as this may cause faulty name if the folder name contains '<'
                pFolder.Name = line[line.Length - 2].Substring(0, whereisthechar);

                // Finding depths
                string[] depthLine = inputarray[pFolder.StartLine].Split('<');
                pFolder.Depth = depthLine[0].Length / 8;

                // Add links
                //google side bug of duplicating all folders and bookmarks, resulting in 3 line long empty folders as well as not empty folders, which contain two copies of every bookmark.
                //shouldn't have too much of an effect on the end results,
                //just divide most numbers by 2. yt-dlp already uses archive.txt, so only lookup time is wasted, not downloads
                if (pFolder.EndLine - pFolder.StartLine > 2)
                {
                    for (int lineindex = pFolder.StartLine; lineindex < pFolder.EndLine + 1; lineindex++)
                    {
                        if (inputarray[lineindex] != null && inputarray[lineindex].Trim().StartsWith("<DT><A"))
                        {
                            string[] linkLine = inputarray[lineindex].Trim().Split(' ');
                            string link = linkLine[1].Trim().Substring(6, linkLine[1].Trim().Length - 7);
                            pFolder.Urls.Add(link);
                        }
                    }
                }
                else
                {
                    Log.Warning("Folder {FolderName} with id {FolderId} has less than 2 lines. Start: {StartLine}, End: {EndLine}", pFolder.Name, pFolder.Id, pFolder.StartLine, pFolder.EndLine);
                }
            }

            
            List<ImportedFolder> finalFolders = new List<ImportedFolder>();
            foreach (var pFolder in parsedFolders)
            {
                ImportedFolder finalFolder = new ImportedFolder
                {
                    Id = pFolder.Id,
                    Name = pFolder.Name,
                    Depth = pFolder.Depth,
                    StartLine = pFolder.StartLine,
                    urls = pFolder.Urls,
                };

                // Find parent
                HtmlParseData parent = parsedFolders
                    .Where(p => p.Depth == pFolder.Depth - 1 && p.StartLine < pFolder.StartLine && p.EndLine > pFolder.EndLine)
                    .OrderByDescending(p => p.StartLine) // Find the closest containing parent
                    .FirstOrDefault();

                if (parent != null)
                {
                    finalFolder.ParentId = parent.Id;
                }

                finalFolders.Add(finalFolder);
            }
            return finalFolders;
        }
    }
}