using bookmark_dlp;
using Xunit;

namespace Nfbookmark.Tests
{
    public class DataStructuresTests
    {
        [Fact]
        public void Bookmarks_folders_empty_at_start()
        {
            Bookmarks bookmark = new Bookmarks();
            Assert.Empty(bookmark.folders);
        }
        
        [Fact]
        public void Folderclass_children_empty_at_start()
        {
            Folderclass folder = new Folderclass();
            Assert.Empty(folder.children);
        }
        
        [Fact]
        public void Folderclass_stringrepresentation()
        {
            Folderclass folder = new Folderclass()
            {
                startline = 0,
                name = "test",
                depth = 3,
                endingline = 10,
                folderpath = "/one/two/three/four",
                numberoflinks = 100,
                numberofmissinglinks = 2,
                id = 0,
                parent = 0
            };
            string strrepr = folder.ToString();
            Assert.Equal("Name:test, id:0, depth:3, number of urls:0", strrepr);
        }
    }    
}