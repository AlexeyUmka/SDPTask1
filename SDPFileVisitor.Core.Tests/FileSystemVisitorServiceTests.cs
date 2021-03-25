using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private Mock<IDirectoryInfoService> _directoryInfoMock;
        private const string RootPath = "root";
        private const string DefaultFileName = "file";
        private const string DefaultFileExtension = "exe";
        private const string DefaultDirectoryName = "child";

        [Fact]
        public void GetFileSystemInfo_WithoutFilteringFlags_ShouldReturnProperAmountOfTheElements()
        {
            // Arrange
            var visitor = new FileSystemVisitorService(RootPath);
            const int firstLevelFilesAmount = 20;
            const int firstLevelDirectoriesAmount = 1;
            const int secondLevelFilesAmount = 10;
            const int secondLevelDirectoriesAmount = 1;
            var firstPath = getDirectoryPath(0);
            var secondPath = getDirectoryPath(1);
            var thirdPath = getDirectoryPath(2);

            var firstLevelFiles = GetDefaultFiles(firstPath, firstLevelFilesAmount);
            var firstLevelDirectories = GetDefaultDirectories(firstPath);
            var secondLevelFiles = GetDefaultFiles(secondPath, secondLevelFilesAmount);
            var secondLevelDirectories = GetDefaultDirectories(secondPath);
            var allElementsAmount = GetSum(firstLevelFilesAmount, firstLevelDirectoriesAmount, secondLevelFilesAmount,
                secondLevelDirectoriesAmount);
            _directoryInfoMock = new Mock<IDirectoryInfoService>();
            _directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(firstPath))
                .Returns(firstLevelFiles.Concat(firstLevelDirectories));
            _directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(secondPath))
                .Returns(secondLevelFiles.Concat(secondLevelDirectories));
            _directoryInfoMock
                .Setup(x => x.GetFileSystemInfos(thirdPath))
                .Returns(new List<FileSystemInfoModel>());
            // Act
            var searchResult = visitor.Search();
            // Assert
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
