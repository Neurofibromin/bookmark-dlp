using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace Nfbookmark
{
    public static class PrintFolders
    {
        private readonly struct FolderPresentationDTO
        {
            public int Depth { get; }
            public int StartLine { get; }
            public string Name { get; }
            public int UrlCount { get; }
            public int Id { get; }
            public int ParentId { get; }

            public FolderPresentationDTO(int depth, int startLine, string name, int urlCount, int id, int parentId)
            {
                Depth = depth;
                StartLine = startLine;
                Name = name ?? string.Empty;
                UrlCount = urlCount;
                Id = id;
                ParentId = parentId;
            }
        }
        
        private static readonly ILogger Log = Serilog.Log.ForContext(typeof(PrintFolders));
        
        public static void PrintToStreamlegacy(List<ResolvedFolder> folders, bool wantOutputToLog = false, Stream outputStream = null)
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
            // int maxidlength = folders.Select(folder => folder.Id.ToString().Length).Max();
            int deepestdepth = 0;
            int deepestdepthlength = 0;
            int maxstartlinelength = 0;
            int maxnamelength = 0;
            int maxnumberoflinklength = 0;
            int maxidlength = 0;
            
            // Calculate the maximum lengths for formatting
            foreach (ResolvedFolder folder in folders)
            {
                deepestdepth = Math.Max(deepestdepth, folder.Depth);
                deepestdepthlength = Math.Max(deepestdepthlength, folder.Depth.ToString().Length);
                maxstartlinelength = Math.Max(maxstartlinelength, folder.StartLine.ToString().Length);
                maxnamelength = Math.Max(maxnamelength, folder.Name.Length);
                maxnumberoflinklength = Math.Max(maxnumberoflinklength, folder.Urls.Count.ToString().Length);
                maxidlength = Math.Max(maxidlength, folder.Id.ToString().Length);
            }

            if (wantOutputToLog)
                Log.Information("The following folders were found");

            int depthsymbolcounter = 0;
            ResolvedFolder previousFolder = null;
            List<ResolvedFolder> sorted = new List<ResolvedFolder>(folders);
            foreach (ResolvedFolder folder in sorted.OrderBy(a => a.Id).ToList()) //writing the depth, the starting line, the ending line, name, and number of links of all the folders
            {
                ResolvedFolder currentFolder = folder;
                int m = folder.Id;
                //if (m>0) { previousFolder = folders.Single(a => a.id == m-1); } else { previousFolder = null; }

                if (previousFolder != null)
                {
                    if (currentFolder.Depth > previousFolder.Depth) //greater depth than before
                        depthsymbolcounter = depthsymbolcounter + (currentFolder.Depth - previousFolder.Depth);
                    if (currentFolder.Depth < previousFolder.Depth) //lesser depth than before
                        depthsymbolcounter = depthsymbolcounter - (previousFolder.Depth - currentFolder.Depth);
                    if (currentFolder.Depth == previousFolder.Depth) //same depth as before
                    {
                        //depthsymbolcounter does not change
                    }
                } // string.Concat(Enumerable.Repeat("_", Math.Abs(depthsymbolcounter - deepestdepthlength)))

                //at first folder the depth does not change
                if (wantOutputToLog)
                    Log.Information("{Indent}", new string('-', depthsymbolcounter));
                if (wantOutputToStream)
                    writer.Write(string.Concat(Enumerable.Repeat("-", depthsymbolcounter)));
                    
                string write = $"{currentFolder.Depth.ToString().PadRight(deepestdepthlength, '_')}" +
                               new string('_', deepestdepth - depthsymbolcounter) +
                               $" is the depth of startline: {currentFolder.StartLine.ToString().PadLeft(maxstartlinelength, '_')} " +
                               $"[{currentFolder.Name.Replace(' ', '_').PadRight(maxnamelength, '_')}] folder, which contains [{currentFolder.Urls.Count.ToString().PadLeft(maxnumberoflinklength, '_')}] links. " +
                               $"id:\"{currentFolder.Id.ToString().PadLeft(maxidlength, '_')}\" parentId:\"{currentFolder.ParentId.ToString().PadLeft(maxidlength, '_')}";
                
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
        
        /// <summary>
        /// Centralized rendering engine for folder structures.
        /// </summary>
        private static void FolderDTOPrettyPrintHelper(IReadOnlyList<FolderPresentationDTO> folders, bool wantOutputToLog, Stream outputStream)
        {
            bool wantOutputToStream = outputStream != null;
            
            using StreamWriter writer = wantOutputToStream 
                ? new StreamWriter(outputStream, leaveOpen: true) 
                : (!wantOutputToLog ? new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true) : null);
                
            wantOutputToStream = writer != null;

            if (folders == null || folders.Count == 0)
            {
                if (wantOutputToLog) Log.Information("No folders to display.");
                if (wantOutputToStream) writer?.WriteLine("No folders to display.");
                return;
            }

            if (wantOutputToLog) Log.Information("The following folders were found");

            int deepestDepth = folders.Max(f => f.Depth);
            int deepestDepthLength = deepestDepth.ToString().Length;
            int maxStartLineLength = folders.Max(f => f.StartLine).ToString().Length;
            int maxNameLength = folders.Max(f => f.Name.Length);
            int maxNumberOfLinkLength = folders.Max(f => f.UrlCount).ToString().Length;
            int maxIdLength = folders.Max(f => f.Id).ToString().Length;

            var sortedFolders = folders.OrderBy(f => f.Id).ToList();

            foreach (var folder in sortedFolders)
            {
                int depthSymbolCounter = folder.Depth; 
                
                string logMessage = $"{folder.Depth.ToString().PadRight(deepestDepthLength, '_')}" +
                               new string('_', Math.Max(0, deepestDepth - depthSymbolCounter)) +
                               $" is the depth of startline: {folder.StartLine.ToString().PadLeft(maxStartLineLength, '_')} " +
                               $"[{folder.Name.Replace(' ', '_').PadRight(maxNameLength, '_')}] folder, which contains [{folder.UrlCount.ToString().PadLeft(maxNumberOfLinkLength, '_')}] links. " +
                               $"id:\"{folder.Id.ToString().PadLeft(maxIdLength, '_')}\" parentId:\"{folder.ParentId.ToString().PadLeft(maxIdLength, '_')}\"";

                string streamMessage = logMessage.Replace("[", "").Replace("]", "").Replace('_', ' ');

                if (wantOutputToLog)
                {
                    Log.Information("{Indent}", new string('-', depthSymbolCounter));
                    Log.Information("{FolderDetails}", logMessage);
                }

                if (wantOutputToStream)
                {
                    writer.Write(new string('-', depthSymbolCounter));
                    writer.WriteLine(streamMessage);
                }
            }

            if (wantOutputToLog) Log.Information("Altogether {FolderCount} folders were found.", folders.Count);
            if (wantOutputToStream) writer?.WriteLine($"Altogether {folders.Count} folders were found.");
            
            writer?.Flush();
        }
        
        public static void PrintToStream(List<ResolvedFolder> folders, bool wantOutputToLog = false, Stream outputStream = null)
        {
            var metrics = folders?.Select(f => new FolderPresentationDTO(
                f.Depth, f.StartLine, f.Name, f.Urls?.Count ?? 0, f.Id, f.ParentId)).ToList();
            FolderDTOPrettyPrintHelper(metrics, wantOutputToLog, outputStream);
        }
        
        public static void PrintToStream(List<MappedFolder> folders, bool wantOutputToLog = false, Stream outputStream = null)
        {
            var metrics = folders?.Select(f => new FolderPresentationDTO(
                f.Depth, f.StartLine, f.Name, f.Urls?.Count ?? 0, f.Id, f.ParentId)).ToList();
            FolderDTOPrettyPrintHelper(metrics, wantOutputToLog, outputStream);
        }
        
        public static void PrintToStream(List<ImportedFolder> folders, bool wantOutputToLog = false, Stream outputStream = null)
        {
            var metrics = folders?.Select(f => new FolderPresentationDTO(
                f.Depth, f.StartLine, f.Name, f.urls?.Count ?? 0, f.Id, f.ParentId)).ToList();
            FolderDTOPrettyPrintHelper(metrics, wantOutputToLog, outputStream);
        }
    }
}