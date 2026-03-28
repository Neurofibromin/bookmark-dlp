using System;
using System.Collections.Generic;

namespace Nfbookmark;

public class ResolvedFolder
{
    private MappedFolder MappedFolder { get; }
    public IReadOnlyList<YTLink> Links { get; }
    public DownloadStatus DownloadStatus { get; set; }

    public string Name => MappedFolder.Name;
    public int Id => MappedFolder.Id;
    public int ParentId => MappedFolder.ParentId;
    public int Depth => MappedFolder.Depth;
    public int StartLine => MappedFolder.StartLine;
    public string FolderPath => MappedFolder.FolderPath;
        
    // Defensive data publishing: ensure underlying lists cannot be cast back and mutated by clients
    public IReadOnlyList<string> Urls => MappedFolder.Urls;
    public IReadOnlyList<int> ChildrenIds => MappedFolder.ChildrenIds;
    
    public ResolvedFolder(MappedFolder mapped, IReadOnlyList<YTLink> links)
    {
        MappedFolder = mapped ?? throw new ArgumentNullException(nameof(mapped));
        Links = links ?? throw new ArgumentNullException(nameof(links));
        DownloadStatus = new DownloadStatus();
    }
}