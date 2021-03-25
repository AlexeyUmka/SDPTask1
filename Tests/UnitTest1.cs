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
        private Mock<IDirectoryInfoService> _directoryInfoMock;
        private const string RootPath = "root";
        private const string DefaultFileName = "file";
        private const string DefaultFileExtension = "exe";
        private const string DefaultDirectoryName = "child";

        [Fact]
        public void GetFileSystemInfo_WithoutFilteringFlags_ShouldReturnProperAmountOfTheElements()
        {
            // Arrange
            const int firstLevelFilesAmount = 20;
            const int firstLevelDirectoriesAmount = 1;
            const int secondLevelFilesAmount = 10;
            const int secondLevelDirectoriesAmount = 1;

            var firstPath = getDirectoryPath(0);
            var secondPath = getDirectoryPath(1);
            var thirdPath = getDirectoryPath(2);

            var firstLevelFiles = GetDefaultFiles(firstPath, firstLevelFilesAmount).ToList();
            var firstLevelDirectories = GetDefaultDirectories(firstPath).ToList();
            var secondLevelFiles = GetDefaultFiles(secondPath, secondLevelFilesAmount).ToList();
            var secondLevelDirectories = GetDefaultDirectories(secondPath).ToList();
            var allElementsAmount = GetSum(firstLevelFilesAmount, firstLevelDirectoriesAmount, secondLevelFilesAmount,
                secondLevelDirectoriesAmount);
            _directoryInfoMock = new Mock<IDirectoryInfoService>();
            _directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(firstPath))
                .Returns(firstLevelFiles.Concat(firstLevelDirectories).ToList());
            _directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(secondPath))
                .Returns(secondLevelFiles.Concat(secondLevelDirectories).ToList());
            // Act
            var visitor = new FileSystemVisitorService(RootPath, _directoryInfoMock.Object);
            var searchResult = visitor.Search().ToList();
            // Assert
            Assert.Equal(firstLevelFilesAmount, firstLevelFiles.Count());
            Assert.Equal(firstLevelDirectoriesAmount, firstLevelDirectories.Count());
            Assert.Equal(secondLevelFilesAmount, secondLevelFiles.Count());
            Assert.Equal(secondLevelDirectoriesAmount, secondLevelDirectories.Count());
            Assert.Equal(allElementsAmount, searchResult.Count());
        }


        private IEnumerable<FileSystemInfoModel> GetDefaultFiles(string path, int amount = 1)
        {
            var fileSystemInfoModels = Enumerable.Repeat(
                new FileSystemInfoModel(DefaultFileName, path, DefaultFileExtension,
                    SystemItemType.File), amount);
            return fileSystemInfoModels;
        }

        private IEnumerable<FileSystemInfoModel> GetDefaultDirectories(string path, int amount = 1)
        {
            var fileSystemInfoModels = Enumerable.Repeat(
                new FileSystemInfoModel(DefaultDirectoryName, path, "",
                    SystemItemType.Directory), amount);
            return fileSystemInfoModels;
        }

        private string getDirectoryPath(int nestingLevel = 0)
        {
            var path = $"{RootPath}{Enumerable.Repeat($"/{DefaultDirectoryName}", nestingLevel)}";
            return path;
        }

        private int GetSum(params int[] numbers)
        {
            return numbers.Sum();
        }
    }
}
