using System.IO;
using Moq;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

using FilmCRUD.Helpers;
using FilmCRUD.Interfaces;
using FilmCRUD.CustomExceptions;


namespace DepotTests.CRUDTests
{
    public class DirectoryFileListerTests
    {
        private readonly Mock<IFileSystemIOWrapper> _fileSystemIOWrapper;

        private readonly DirectoryFileLister _directoryFileLister;

        public DirectoryFileListerTests()
        {
            this._fileSystemIOWrapper = new Mock<IFileSystemIOWrapper>();
            this._directoryFileLister = new DirectoryFileLister(this._fileSystemIOWrapper.Object);
        }

        [Fact]
        public void ListMoviesAndPersistToTextFile_WithInexistantMovieWarehousePath_ShouldThrowDirectoryNotFoundException()
        {
            // arrange
            string inexistentMovieWarehousePath = "Z:\\SomeDir";
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(inexistentMovieWarehousePath)).Returns(false);

            // act
            // nada a fazer...

            // assert
            _directoryFileLister
                .Invoking(d => d.ListMoviesAndPersistToTextFile(inexistentMovieWarehousePath, "D:\\DoesNotMatter"))
                .Should()
                .Throw<DirectoryNotFoundException>()
                .WithMessage(inexistentMovieWarehousePath);
        }

        [Fact]
        public void ListMoviesAndPersistToTextFile_WithInexistentDestinationDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // arrange
            string existentMovieWarehousePath = "Z:\\WarehouseDir";
            string inexistentDestinationDirectory = "S:\\SomeDstDir";
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(existentMovieWarehousePath)).Returns(true);
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(inexistentDestinationDirectory)).Returns(false);

            // act
            // nada a fazer...

            // assert
            _directoryFileLister
                .Invoking(d => d.ListMoviesAndPersistToTextFile(existentMovieWarehousePath, inexistentDestinationDirectory))
                .Should()
                .Throw<DirectoryNotFoundException>()
                .WithMessage(inexistentDestinationDirectory);
        }

        [Fact]
        public void ListMoviesAndPersistToTextFile_WithExistingFileName_ShouldThrowFileExistsError()
        {
            // arrange
            string existentMovieWarehousePath = "Z:\\WarehouseDir";
            string existentDestinationDirectory = "S:\\SomeDstDir";
            string existentFileName = $"movies_{DateTime.Now.ToString("yyyyMMdd")}.txt";
            string existentFilePath = Path.Combine(existentDestinationDirectory, existentFileName);
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(existentMovieWarehousePath)).Returns(true);
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(existentDestinationDirectory)).Returns(true);
            _fileSystemIOWrapper
                .Setup(f => f.GetFiles(existentDestinationDirectory))
                .Returns(new string[] { existentFilePath });

            // act
            // nada a fazer...

            // assert
            _directoryFileLister
                .Invoking(d => d.ListMoviesAndPersistToTextFile(existentMovieWarehousePath, existentDestinationDirectory))
                .Should()
                .Throw<FileExistsError>()
                .WithMessage(existentFilePath);
        }

        [Fact]
        public void GetMovieFileNames_WithExistingMovieWarehousePath_ShouldReturnCorrectDirectoryContents()
        {
            // arrange
            string existentMovieWarehousePath = "Z:\\WarehouseDir";
            string[] movieFileNames = {
                "The.Lives.of.Others.2006.GERMAN.REMASTERED.1080p.BluRay.x264.DTS-NOGRP",
                "Sicario 2015 1080p BluRay x264 AC3-JYK"
            };
            IEnumerable<string> warehouseContents = movieFileNames.Select(s => Path.Combine(existentMovieWarehousePath, s));
            _fileSystemIOWrapper.Setup(f => f.DirectoryExists(existentMovieWarehousePath)).Returns(true);
            _fileSystemIOWrapper.Setup(f => f.GetSubdirectories(existentMovieWarehousePath)).Returns(warehouseContents);

            // act
            List<string> result = _directoryFileLister.GetMovieFileNames(existentMovieWarehousePath);

            // assert
            // da documentação:
            //     The two collections are equivalent when they both contain the same strings in any order.
            result.Should().BeEquivalentTo(movieFileNames);
        }
    }
}