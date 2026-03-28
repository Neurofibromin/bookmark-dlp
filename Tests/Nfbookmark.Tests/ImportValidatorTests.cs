using System.Collections.Generic;
using Nfbookmark;
using Nfbookmark.Importers;
using Xunit;

namespace Nfbookmark.Tests
{
    public class ImportValidatorTests
    {
        [Theory]
        [InlineData("ValidName", "ValidName")]
        [InlineData("Folder/With\\Slashes", "FolderWithSlashes")]
        [InlineData("TrailingSpaces   ", "TrailingSpaces")]
        [InlineData("TrailingPeriods...", "TrailingPeriods")]
        [InlineData(".HiddenFolder", "HiddenFolder")]
        [InlineData("<Illegal*Chars?>", "IllegalChars")]
        public void ValidateFolderNames_SanitizesStringsCorrectly(string inputName, string expectedName)
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 1, Name = inputName, ParentId = 0 }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedName, result[0].Name);
        }

        [Theory]
        [InlineData("CON")]
        [InlineData("prn")]
        [InlineData("COM1")]
        [InlineData("LPT9")]
        [InlineData("NUL")]
        public void ValidateFolderNames_CatchesReservedDosNames(string reservedName)
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 99, Name = reservedName, ParentId = 0 }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            // The validator should completely replace reserved Names with the fallback ID format
            Assert.Equal("ID99", result[0].Name);
        }

        [Fact]
        public void ValidateFolderNames_ResolvesSiblingDuplicates()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 10, Name = "DuplicateFolder", ParentId = 5 },
                new ImportedFolder { Id = 11, Name = "DuplicateFolder", ParentId = 5 }, // Collision
                new ImportedFolder { Id = 12, Name = "DuplicateFolder", ParentId = 6 }  // Different parent, no collision
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("DuplicateFolder", result[0].Name);
            Assert.Equal("DuplicateFolder_ID11", result[1].Name); // Mutated to prevent collision
            Assert.Equal("DuplicateFolder", result[2].Name); // Untouched due to different parent
        }
        
        [Fact]
        public void ValidateFolderNames_WithNullOrWhitespace_GeneratesFallbackId()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 1, Name = null },
                new ImportedFolder { Id = 2, Name = "   " },
                new ImportedFolder { Id = 3, Name = "\t" }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal("ID1", result.Single(f => f.Id == 1).Name);
            Assert.Equal("ID2", result.Single(f => f.Id == 2).Name);
            Assert.Equal("ID3", result.Single(f => f.Id == 3).Name);
        }

        [Fact]
        public void ValidateFolderNames_WithIllegalCharacters_ReplacesWithUnderscore()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 10, Name = "My/Folder:Name?" }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal("My_Folder_Name_", result.First().Name);
        }

        [Fact]
        public void ValidateFolderNames_WithDosReservedNames_AppendsId()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 99, Name = "CON" },
                new ImportedFolder { Id = 100, Name = "prn" },
                new ImportedFolder { Id = 101, Name = "LPT1" }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal("CON_ID99", result.Single(f => f.Id == 99).Name);
            Assert.Equal("prn_ID100", result.Single(f => f.Id == 100).Name);
            Assert.Equal("LPT1_ID101", result.Single(f => f.Id == 101).Name);
        }

        [Fact]
        public void ValidateFolderNames_WithCaseInsensitiveSiblings_AppendsIdToPreventCollision()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 5, ParentId = 1, Name = "Homework" },
                new ImportedFolder { Id = 6, ParentId = 1, Name = "homework" },
                new ImportedFolder { Id = 7, ParentId = 2, Name = "homework" }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal("Homework", result.Single(f => f.Id == 5).Name);
            Assert.Equal("homework_ID6", result.Single(f => f.Id == 6).Name);
            Assert.Equal("homework", result.Single(f => f.Id == 7).Name);
        }

        [Fact]
        public void ValidateFolderNames_WithTrailingPeriodsAndSpaces_TrimsAndFallsBack()
        {
            // Arrange
            var folders = new List<ImportedFolder>
            {
                new ImportedFolder { Id = 8, Name = "Folder Name . " },
                new ImportedFolder { Id = 9, Name = " . . " }
            };

            // Act
            var result = ImportValidator.ValidateFolderNames(folders);

            // Assert
            Assert.Equal("Folder Name", result.Single(f => f.Id == 8).Name);
            Assert.Equal("ID9", result.Single(f => f.Id == 9).Name);
        }
        

        [Fact]
        public void FoldernameValidation_RemovesForbiddenCharacters()
        {
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = "folder/1", Id = 1, Depth = 0, ParentId = 0 },
                new ImportedFolder { Name = "folder?2", Id = 2, Depth = 0, ParentId = 0 }
            };
            var result = ImportValidator.ValidateFolderNames(folders);
            Assert.Equal("folder1", result[0].Name);
            Assert.Equal("folder2", result[1].Name);
        }

        [Fact]
        public void FoldernameValidation_HandlesEmptyNames()
        {
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = "", Id = 1, Depth = 0, ParentId = 0 }
            };
            var result = ImportValidator.ValidateFolderNames(folders);
            Assert.Equal("ID1", result[0].Name);
        }

        [Fact]
        public void FoldernameValidation_HandlesSpacesAndPeriods()
        {
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = " . ", Id = 1, Depth = 0, ParentId = 0 }
            };
            var result = ImportValidator.ValidateFolderNames(folders);
            Assert.Equal("ID1", result[0].Name);
        }

        [Fact]
        public void FoldernameValidation_HandlesNamesStartingWithPeriods()
        {
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = ".hidden", Id = 1, Depth = 0, ParentId = 0 }
            };
            var result = ImportValidator.ValidateFolderNames(folders);
            Assert.Equal("ID1", result[0].Name);
        }

        [Fact]
        public void FoldernameValidation_HandlesDuplicateNamesAtSameDepthAndParent()
        {
            List<ImportedFolder> folders = new List<ImportedFolder>
            {
                new ImportedFolder { Name = "duplicate", Id = 1, Depth = 0, ParentId = 0 },
                new ImportedFolder { Name = "duplicate", Id = 2, Depth = 0, ParentId = 0 }
            };
            var result = ImportValidator.ValidateFolderNames(folders);
            // First occurrence is kept, subsequent ones get ID suffix
            Assert.Equal("duplicate", result[0].Name);
            Assert.Equal("duplicate_ID2", result[1].Name);
        }
    
    }
}