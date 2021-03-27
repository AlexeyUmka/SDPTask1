using System.Collections.Generic;
using System.Linq;
using Moq;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Models;
using SDPFileVisitor.Core.Services;
using Xunit;

namespace SDPFileVisitor.Core.Tests
{
    public class FileSystemVisitorServiceTests
    {
        private const string RootPath = "root";
        private const string DefaultFileName = "file";
        private const string DefaultFileExtension = "exe";
        private const string DefaultDirectoryName = "child";
        private const int FirstLevelFilesAmount = 20;
        private const int FirstLevelDirectoriesAmount = 1;
        private const int SecondLevelFilesAmount = 10;
        private const int SecondLevelDirectoriesAmount = 1;
        private static string FirstPath => getDirectoryPath(0);
        private static string SecondPath => getDirectoryPath(1);
        private static string ThirdPath => getDirectoryPath(2);
        private static IEnumerable<FileSystemInfoModel> FirstLevelFiles => GetDefaultFiles(FirstPath, FirstLevelFilesAmount).ToList();
        private static IEnumerable<FileSystemInfoModel> FirstLevelDirectories => GetDefaultDirectories(SecondPath).ToList();
        private static IEnumerable<FileSystemInfoModel> SecondLevelFiles => GetDefaultFiles(ThirdPath, SecondLevelFilesAmount).ToList();
        private static IEnumerable<FileSystemInfoModel> SecondLevelDirectories => GetDefaultDirectories(ThirdPath).ToList();

        private static int AllElementsAmount => GetSum(FirstLevelFilesAmount, FirstLevelDirectoriesAmount,
            SecondLevelFilesAmount, SecondLevelDirectoriesAmount);

        [Fact]
        public void GetFileSystemInfo_WithoutFilteringFlags_ShouldReturnProperAmountOfTheElements()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(AllElementsAmount, searchResult.Count());
        }
        
        [Fact]
        public void GetFileSystemInfo_WithoutFilteringFlags_ShouldCallAppropriateEvents()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            var searchStartedCalledAmount = 0;
            visitor.SearchStarted += (sender, eventArgs) => searchStartedCalledAmount++;
            var searchFinishedCalledAmount = 0;
            visitor.SearchFinished += (sender, eventArgs) => searchFinishedCalledAmount++;
            var fileFoundCalledAmount = 0;
            var fileFilteredCalledAmount = 0;
            visitor.FileFound += (sender, eventArgs) => fileFoundCalledAmount++;
            visitor.FileFiltered += (sender, eventArgs) => fileFilteredCalledAmount++;
            var directoryFoundCalledAmount = 0;
            var directoryFilteredCalledAmount = 0;
            visitor.DirectoryFound += (sender, eventArgs) => directoryFoundCalledAmount++;
            visitor.DirectoryFiltered += (sender, eventArgs) => directoryFilteredCalledAmount++;
            var expectedFilesAmount = GetSum(FirstLevelFilesAmount, SecondLevelFilesAmount);
            var expectedDirectoriesAmount = GetSum(FirstLevelDirectoriesAmount, SecondLevelDirectoriesAmount);
            
            // Act
            visitor.Search().ToList();
            
            // Assert
            Assert.Equal(1, searchStartedCalledAmount);
            Assert.Equal(1, searchFinishedCalledAmount);
            Assert.Equal(expectedFilesAmount, fileFoundCalledAmount);
            Assert.Equal(expectedFilesAmount, fileFilteredCalledAmount);
            Assert.Equal(expectedDirectoriesAmount, directoryFoundCalledAmount);
            Assert.Equal(expectedDirectoriesAmount, directoryFoundCalledAmount);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithFilteringFlags_ShouldReturnEmptyResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath,_ => false, directoryInfoMock.Object);
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Empty(searchResult);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterStartSearch_ShouldReturnEmptyResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.SearchStarted += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Empty(searchResult);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterSearchFinished_ShouldNotAffect()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.SearchFinished += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(AllElementsAmount, searchResult.Count);
        }

        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterFileFound_ShouldReturnEmptyResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Empty(searchResult);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterFileFiltered_ShouldReturnEmptyResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFiltered += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Empty(searchResult);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterDirectoryFound_ShouldReturnFirstLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFound += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(FirstLevelFilesAmount, searchResult.Count);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithStopSearchAfterDirectoryFiltered_ShouldReturnFirstLevelFilesAndSecondLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFiltered += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(GetSum(FirstLevelFilesAmount, SecondLevelFilesAmount), searchResult.Count);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithExcludeAfterFileFound_ShouldReturnAllInternalDirectoriesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(GetSum(FirstLevelDirectoriesAmount, SecondLevelDirectoriesAmount), searchResult.Count);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithExcludeAfterFileFiltered_ShouldReturnAllInternalDirectoriesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFiltered += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(GetSum(FirstLevelDirectoriesAmount, SecondLevelDirectoriesAmount), searchResult.Count);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithExcludeAfterDirectoryFound_ShouldReturnFirstLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFound += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(FirstLevelFilesAmount, searchResult.Count);
        }
        
        [Fact]
        public void GetFileSystemInfo_WithExcludeAfterDirectoryFiltered_ShouldReturnAllInternalFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFiltered += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.Equal(GetSum(FirstLevelFilesAmount, SecondLevelFilesAmount), searchResult.Count);
        }

        private static void SetDefaultMockSetup(Mock<IDirectoryInfoService> directoryServiceMock)
        {
            directoryServiceMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(FirstLevelFiles.Concat(FirstLevelDirectories).ToList());
            directoryServiceMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(SecondLevelFiles.Concat(SecondLevelDirectories).ToList());
        }


        private static IEnumerable<FileSystemInfoModel> GetDefaultFiles(string path, int amount = 1)
        {
            var fileSystemInfoModels = Enumerable.Repeat(
                new FileSystemInfoModel(DefaultFileName, path, DefaultFileExtension,
                    SystemItemType.File), amount);
            return fileSystemInfoModels;
        }

        private static IEnumerable<FileSystemInfoModel> GetDefaultDirectories(string path, int amount = 1)
        {
            var fileSystemInfoModels = Enumerable.Repeat(
                new FileSystemInfoModel(DefaultDirectoryName, path, "",
                    SystemItemType.Directory), amount);
            return fileSystemInfoModels;
        }

        private static string getDirectoryPath(int nestingLevel = 0)
        {
            var path = $"{RootPath}{string.Join(string.Empty, Enumerable.Repeat($"/{DefaultDirectoryName}", nestingLevel))}";
            return path;
        }

        private static int GetSum(params int[] numbers)
        {
            return numbers.Sum();
        }
    }
}
