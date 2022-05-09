using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;

using FilmCRUD;
using FilmDomain.Interfaces;
using System;
using FilmDomain.Entities;

namespace DepotTests.CRUDTests
{
    public class ScanManagerTests
    {
        private readonly Mock<IMovieRipRepository> _movieRipRepositoryMock;
        private readonly  Mock<IMovieWarehouseVisitRepository> _movieWarehouseVisitRepositoryMock;
        private readonly  Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly ScanManager _scanManager;
        public ScanManagerTests()
        {
            this._movieRipRepositoryMock = new Mock<IMovieRipRepository>();
            this._movieWarehouseVisitRepositoryMock = new Mock<IMovieWarehouseVisitRepository>();

            this._unitOfWorkMock = new Mock<IUnitOfWork>();
            this._unitOfWorkMock
                .SetupGet(u => u.MovieWarehouseVisits)
                .Returns(this._movieWarehouseVisitRepositoryMock.Object);

            this._unitOfWorkMock
                .SetupGet(u => u.MovieRips)
                .Returns(this._movieRipRepositoryMock.Object);

            this._scanManager = new ScanManager(this._unitOfWorkMock.Object);
        }

        [Fact]
        public void GetRipCountByReleaseDate_ReturnsCorrectCount()
        {
            // arrange
            var latestVisit = new MovieWarehouseVisit() {
                MovieRips = new List<MovieRip>() {
                    new MovieRip() { ParsedReleaseDate = "1999" },
                    new MovieRip() { ParsedReleaseDate = "1999" },
                    new MovieRip() { ParsedReleaseDate = "2000" }
                }
            };
            this._movieWarehouseVisitRepositoryMock
                .Setup(m => m.GetClosestMovieWarehouseVisit())
                .Returns(latestVisit);


            // act
            var countByReleaseDate = this._scanManager.GetRipCountByReleaseDate();

            // assert
            var expected = new Dictionary<string, int>() {
                ["1999"] = 2,
                ["2000"] = 1
            };
            countByReleaseDate.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAllRipsWithReleaseDate_ReturnsExpectedFileNames()
        {
            // arrange
            string releaseDate = "1997";
            var movieRips = new List<MovieRip>() {
                    new MovieRip() {
                        FileName = "Face.Off.1997.iNTERNAL.1080p.BluRay.x264-MARS[rarbg]",
                        ParsedReleaseDate = "1997"
                        },
                     new MovieRip() {
                        FileName = "Gummo.1997.DVDRip.XviD-DiSSOLVE",
                        ParsedReleaseDate = "1997"
                        }
                };

            this._movieRipRepositoryMock
                .Setup(m => m.Find(It.IsAny<Expression<Func<MovieRip, bool>>>()))
                .Returns(movieRips);


            // act
            var ripsWithReleaseDate = this._scanManager.GetAllRipsWithReleaseDate(releaseDate);

            // assert
            var expected = new List<string>() {
                "Face.Off.1997.iNTERNAL.1080p.BluRay.x264-MARS[rarbg]",
                "Gummo.1997.DVDRip.XviD-DiSSOLVE"
            };
            // da documentação:
            //     The two collections are equivalent when they both contain the same strings in any order.
            ripsWithReleaseDate.Should().BeEquivalentTo(expected);

        }

        [Fact]
        public void GetRipCountByVisit_ReturnsCorrectCount()
        {
            // arrange
            var visit_0 = new MovieWarehouseVisit() {
                VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null),
                MovieRips = new List<MovieRip>() { new MovieRip(), new MovieRip() }
                };
            var visit_1 = new MovieWarehouseVisit() {
                VisitDateTime = DateTime.ParseExact("20220102", "yyyyMMdd", null),
                MovieRips = new List<MovieRip>() { new MovieRip(), new MovieRip(), new MovieRip(), new MovieRip() }
                };
            this._movieWarehouseVisitRepositoryMock
                .Setup(m => m.GetAll())
                .Returns(new MovieWarehouseVisit[] { visit_0, visit_1 });

            // act
            Dictionary<DateTime, int> ripCountByVisit = this._scanManager.GetRipCountByVisit();

            // assert
            var expected = new Dictionary<DateTime, int>() {
                [DateTime.ParseExact("20220101", "yyyyMMdd", null)] = 2,
                [DateTime.ParseExact("20220102", "yyyyMMdd", null)] = 4,
            };
            ripCountByVisit.Should().BeEquivalentTo(expected);
        }
    }
}