using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Models;
using SDPFileVisitor.Core.Services;
using Xunit;

namespace Tests
{
    public class UnitTest1
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
        private static List<FileSystemInfoModel> FirstLevelFiles => GetDefaultFiles(FirstPath, FirstLevelFilesAmount).ToList();
        private static List<FileSystemInfoModel> FirstLevelDirectories => GetDefaultDirectories(SecondPath).ToList();
        private static List<FileSystemInfoModel> SecondLevelFiles => GetDefaultFiles(ThirdPath, SecondLevelFilesAmount).ToList();
        private static List<FileSystemInfoModel> SecondLevelDirectories => GetDefaultDirectories(ThirdPath).ToList();

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
            Assert.Equal(FirstLevelFilesAmount, FirstLevelFiles.Count());
            Assert.Equal(FirstLevelDirectoriesAmount, FirstLevelDirectories.Count());
            Assert.Equal(SecondLevelFilesAmount, SecondLevelFiles.Count());
            Assert.Equal(SecondLevelDirectoriesAmount, SecondLevelDirectories.Count());
            Assert.Equal(AllElementsAmount, searchResult.Count());
        }
        
        [Fact]
        public void GetFileSystemInfo_WithoutFilteringFlags_ShouldCallAppropriateEvents()
        {
            // // Arrange
            // var directoryInfoMock = new Mock<IDirectoryInfoService>();
            // SetDefaultMockSetup(directoryInfoMock);
            //
            // // Act
            // var visitor = new FileSystemVisitorService(FirstPath, directoryInfoMock.Object);
            // var searchResult = visitor.Search().ToList();
            // // Assert
            // Assert.Equal(FirstLevelFilesAmount, FirstLevelFiles.Count());
            // Assert.Equal(FirstLevelDirectoriesAmount, FirstLevelDirectories.Count());
            // Assert.Equal(SecondLevelFilesAmount, SecondLevelFiles.Count());
            // Assert.Equal(SecondLevelDirectoriesAmount, SecondLevelDirectories.Count());
            // Assert.Equal(AllElementsAmount, searchResult.Count());
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
