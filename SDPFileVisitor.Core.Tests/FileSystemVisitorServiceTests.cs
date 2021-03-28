using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SDPFileVisitor.Core.Comparers;
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

        private static IEnumerable<FileSystemInfoModel> AllElements => FirstLevelFiles.Concat(FirstLevelDirectories)
            .Concat(FirstLevelDirectories).Concat(SecondLevelDirectories);

        private static int AllElementsAmount => GetSum(FirstLevelFilesAmount, FirstLevelDirectoriesAmount,
            SecondLevelFilesAmount, SecondLevelDirectoriesAmount);

        [Fact]
        public void Search_FiltersNotGiven_ReturnsAllElements()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(AllElements, searchResult));
        }
        
        [Fact]
        public void Search_FiltersNotGiven_CallAppropriateEvents()
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
        public void Search_GivenRejectAllFilter_ReturnsEmptyResult()
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
        public void Search_GivenRejectByNameFilter_ReturnsFilteredResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            var firstLevelElements = FirstLevelFiles.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "skip", string.Empty, SystemItemType.File),
                    new FileSystemInfoModel(string.Empty, "notSkip", string.Empty, SystemItemType.File)
                }).Concat(FirstLevelDirectories.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "skip", string.Empty, SystemItemType.Directory),
                    new FileSystemInfoModel(string.Empty, "notSkip", string.Empty, SystemItemType.Directory)
                })).ToList();
            var secondLevelElements = SecondLevelFiles.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "skip", string.Empty, SystemItemType.File),
                    new FileSystemInfoModel(string.Empty, "notSkip", string.Empty, SystemItemType.File)
                }
                ).Concat(SecondLevelDirectories.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "skip", string.Empty, SystemItemType.Directory),
                    new FileSystemInfoModel(string.Empty, "notSkip", string.Empty, SystemItemType.Directory)
                })).ToList();
            var allElementsExceptSkipped = firstLevelElements.Concat(secondLevelElements).Where(el => el.FullName != "skip");
            
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(firstLevelElements);
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(secondLevelElements);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath,fInfo => fInfo.Name != "skip", directoryInfoMock.Object);
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(allElementsExceptSkipped, searchResult));
        }
        
        [Fact]
        public void Search_GivenStopSearchAfterStartSearch_ReturnsEmptyResult()
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
        public void Search_GivenStopSearchAfterSearchFinished_ReturnsAllElements()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.SearchFinished += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(AllElements, searchResult));
        }

        [Fact]
        public void Search_GivenStopSearchAfterFirstFileFound_ReturnsEmptyResult()
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
        public void Search_GivenStopSearchAfterPenultimateFirstLevelFileFound_ReturnsPreviousFirstLevelFiles()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(FirstLevelFiles.Concat(
                    new []
                    {
                        new FileSystemInfoModel(string.Empty, "forStop", string.Empty, SystemItemType.File),
                        new FileSystemInfoModel(string.Empty, "restFile",  string.Empty, SystemItemType.File)
                    }).Concat(FirstLevelDirectories).ToList());
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(SecondLevelFiles.Concat(SecondLevelDirectories).ToList());
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.StopSearch = args.Name == "forStop";
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles, searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeFileAfterFileFound_ReturnsAllElementsExceptExcluded()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            var allElements = FirstLevelFiles.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "forExclude", string.Empty, SystemItemType.File),
                    new FileSystemInfoModel(string.Empty, "restFile", string.Empty, SystemItemType.File)
                }).Concat(FirstLevelDirectories).ToList();
            var allElementsExceptExcluded = allElements.Where(el => el.FullName != "forExclude");
            
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(allElements);
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(SecondLevelFiles.Concat(SecondLevelDirectories).ToList());
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.Exclude = args.Name == "forExclude";
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(allElementsExceptExcluded, searchResult));
        }
        
        [Fact]
        public void Search_GivenStopSearchAfterFirstFileFiltered_ReturnsEmptyResult()
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
        public void Search_GivenStopSearchAfterFirstDirectoryFound_ReturnsFirstLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFound += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles, searchResult));
        }
        
        [Fact]
        public void Search_GivenStopSearchAfterPenultimateFirstLevelDirectoryFound_ReturnsPreviousFirstLevelFiles()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(FirstLevelFiles.Concat(FirstLevelDirectories.Concat(
                    new []
                    {
                        new FileSystemInfoModel(string.Empty, "forStop", string.Empty, SystemItemType.Directory),
                        new FileSystemInfoModel(string.Empty, "restDirectory",  string.Empty, SystemItemType.Directory)
                    })).ToList());
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(SecondLevelFiles.Concat(SecondLevelDirectories).ToList());
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.StopSearch = args.Name == "forStop";
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles, searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeDirectoryAfterDirectoryFound_ReturnsAllElementsExceptExcluded()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            var firstLevelElements = FirstLevelFiles.Concat(FirstLevelDirectories).ToList();
            var secondLevelElements = SecondLevelFiles.Concat(SecondLevelDirectories.Concat(
                new[]
                {
                    new FileSystemInfoModel(string.Empty, "forExclude", string.Empty, SystemItemType.Directory),
                    new FileSystemInfoModel(string.Empty, "restDirectory", string.Empty, SystemItemType.Directory)
                })).ToList();
            var allElementsExceptExcluded = firstLevelElements.Concat(secondLevelElements).Where(el => el.FullName != "forExclude");
            
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(FirstPath))
                .Returns(firstLevelElements);
            directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(SecondPath))
                .Returns(secondLevelElements);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.Exclude = args.Name == "forExclude";
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(allElementsExceptExcluded, searchResult));
        }
        
        [Fact]
        public void Search_GivenStopSearchAfterFirstDirectoryFiltered_ReturnsFirstLevelFilesAndSecondLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFiltered += (sender, args) => args.StopSearch = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles.Concat(SecondLevelFiles), searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeAfterFirstFileFound_ReturnsAllInternalDirectoriesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFound += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelDirectories.Concat(SecondLevelDirectories), searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeAfterFirstFileFiltered_ReturnsAllInternalDirectoriesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.FileFiltered += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelDirectories.Concat(SecondLevelDirectories), searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeAfterFirstDirectoryFound_ReturnsFirstLevelFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFound += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles, searchResult));
        }
        
        [Fact]
        public void Search_GivenExcludeAfterFirstDirectoryFiltered_ReturnsAllInternalFilesResult()
        {
            // Arrange
            var directoryInfoMock = new Mock<IDirectoryInfoService>();
            SetDefaultMockSetup(directoryInfoMock);
            
            // Act
            var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            visitor.DirectoryFiltered += (sender, args) => args.Exclude = true;
            var searchResult = visitor.Search().ToList();
            
            // Assert
            Assert.True(CollectionsHaveSameElements(FirstLevelFiles, searchResult));
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

        private static bool CollectionsHaveSameElements(IEnumerable<FileSystemInfoModel> first,
            IEnumerable<FileSystemInfoModel> second)
        {
            return first.All(fElement => second.Any(sElement => sElement.Equals(fElement)));
        }
    }
}
