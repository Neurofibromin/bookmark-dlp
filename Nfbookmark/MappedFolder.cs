using System;
using System.Collections.Generic;

namespace Nfbookmark;

public class MappedFolder
{
    private ImportedFolder Folder { get; }
    public string FolderPath { get; set; }
    
    public string Name => Folder.Name;
    public int Id => Folder.Id;
    public int ParentId => Folder.ParentId;
    public int Depth => Folder.Depth;
    public int StartLine => Folder.StartLine;
    
    public IReadOnlyList<string> Urls => Folder.urls?.AsReadOnly();
    public IReadOnlyList<int> ChildrenIds => Folder.ChildrenIds?.AsReadOnly();
    
    public MappedFolder(ImportedFolder folder) 
    {
        Folder = folder;
    }
}