using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Serilog;

namespace Nfbookmark.Importers
{
    public class HtmlExportImporter : IBookmarkImporter
    {
        private readonly ILogger Log = Serilog.Log.ForContext<HtmlExportImporter>();

        public List<ImportedFolder> UnsafeImport(string filePath)
        {
            // --- STAGE 1: LOAD ---
            Log.Information("--- STAGE 1: Loading File ---");
            if (!File.Exists(filePath))
            {
                Log.Error("Html file was not found: {FilePath}", filePath);
                return null;
            }

            var doc = new HtmlDocument
            {
                // Netscape bookmark files often lack closing tags for DT and DD. 
                // FixNestedTags is crucial here.
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };

            try
            {
                doc.Load(filePath);
                Log.Debug("File loaded successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load or parse HTML file.");
                return null;
            }
            
            List<ImportedFolder> flatFolderList = new List<ImportedFolder>();

            // --- STAGE 2: ROOT IDENTIFICATION ---
            Log.Information("--- STAGE 2: Identifying Root ---");
            
            // Usually the first DL is the root container
            HtmlNode rootDl = doc.DocumentNode.SelectSingleNode("//dl");

            if (rootDl == null)
            {
                Log.Warning("No bookmark structure (<DL> tag) found in file.");
                // Fallback: Attempt to look at body just in case
                Log.Debug("Dumping Body InnerHtml for diagnosis: {HtmlSnippet}", 
                    doc.DocumentNode.SelectSingleNode("//body")?.InnerHtml ?? "No Body found");
                return flatFolderList;
            }
            
            Log.Debug("Root DL node found. Line: {Line}, Position: {Pos}", rootDl.Line, rootDl.LinePosition);
            
            // Print attributes of the root (usually contains 'p' or 'compact')
            foreach (HtmlAttribute attr in rootDl.Attributes)
            {
                Log.Debug("Root Attribute: [{Name}] = '{Value}'", attr.Name, attr.Value);
            }

            // Print the raw HTML of just the start of the root to ensure we grabbed what we think we grabbed
            // Taking substring to avoid flooding logs
            string rawSnippet = rootDl.OuterHtml.Length > 300 
                ? rootDl.OuterHtml.Substring(0, 300) + "..." 
                : rootDl.OuterHtml;
            Log.Debug("Root Content Preview: {RawHtml}", rawSnippet);


            // --- STAGE 3: VISUALIZING THE PARSE TREE ---
            Log.Information("--- STAGE 3: Visualizing Parse Tree ---");
            Log.Debug("Starting recursive debug walk...");
            
            // This will print the hierarchy to the console/log so you can design your loop
            DebugPrintNodeTree(rootDl, 0);

            Log.Information("--- Debug Walk Complete ---");

            // TODO: Call your actual recursive parsing logic here
            // var rootFolder = ParseRecursive(rootDl);

            return flatFolderList;
        }

        /// <summary>
        /// A recursive helper designed purely for debugging.
        /// It prints the structure, indentation, and node types (Folder vs Bookmark).
        /// </summary>
        private void DebugPrintNodeTree(HtmlNode node, int depth)
        {
            // Create visual indentation based on recursion depth
            string indent = new string(' ', depth * 2); 
            string nodeInfo;

            // HAP often creates "#text" nodes for whitespace/newlines between tags.
            // We usually want to ignore these unless they contain actual words.
            if (node.Name == "#text")
            {
                if (string.IsNullOrWhiteSpace(node.InnerText)) return; // Skip whitespace noise
                nodeInfo = $"[Text Content]: \"{node.InnerText.Trim()}\"";
            }
            else if (node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase))
            {
                // H3 usually indicates a Folder Title in Netscape format
                nodeInfo = $"[FOLDER TITLE]: \"{node.InnerText}\"";
            }
            else if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                // A usually indicates a Bookmark
                string href = node.GetAttributeValue("href", "NO_URL");
                nodeInfo = $"[BOOKMARK]: Title=\"{node.InnerText}\" URL=\"{href}\"";
            }
            else if (node.Name.Equals("dl", StringComparison.OrdinalIgnoreCase))
            {
                nodeInfo = "[SUB-LIST / FOLDER CONTENTS]";
            }
            else if (node.Name.Equals("dt", StringComparison.OrdinalIgnoreCase))
            {
                nodeInfo = "[ITEM WRAPPER]";
            }
            else
            {
                nodeInfo = $"[Tag: {node.Name}]";
            }

            // Log the node with indentation
            Log.Debug("{Indent}|- {NodeType} {Details}", indent, node.Name, nodeInfo);

            // Recursively print children
            foreach (HtmlNode child in node.ChildNodes)
            {
                DebugPrintNodeTree(child, depth + 1);
            }
        }
    }
}