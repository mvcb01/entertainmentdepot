using Xunit;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;

using FilmDomain.Entities;
using FilmDomain.Interfaces;
using FilmCRUD;
using FluentAssertions.Execution;

namespace DepotTests.CRUDTests
{
    public class ScanMoviesManagerTests
    {
        private readonly Mock<IActorRepository> _actorRepositoryMock;

        private readonly Mock<IGenreRepository> _genreRepositoryMock;

        private readonly Mock<IDirectorRepository> _directorRepositoryMock;

        private readonly Mock<IMovieRepository> _movieRepositoryMock;

        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly ScanMoviesManager _scanMoviesManager;

        public ScanMoviesManagerTests()
        {
            this._actorRepositoryMock = new Mock<IActorRepository>(MockBehavior.Strict);
            this._genreRepositoryMock = new Mock<IGenreRepository>(MockBehavior.Strict);
            this._directorRepositoryMock = new Mock<IDirectorRepository>(MockBehavior.Strict);
            this._movieRepositoryMock = new Mock<IMovieRepository>(MockBehavior.Strict);

            this._unitOfWorkMock = new Mock<IUnitOfWork>();
            this._unitOfWorkMock
                .SetupGet(u => u.Actors)
                .Returns(this._actorRepositoryMock.Object);
            this._unitOfWorkMock
                .SetupGet(u => u.Genres)
                .Returns(this._genreRepositoryMock.Object);
            this._unitOfWorkMock
                .SetupGet(u => u.Directors)
                .Returns(this._directorRepositoryMock.Object);
            this._unitOfWorkMock
                .SetupGet(u => u.Movies)
                .Returns(this._movieRepositoryMock.Object);

            this._scanMoviesManager = new ScanMoviesManager(this._unitOfWorkMock.Object);
        }

        [Fact]
        public void GetMoviesWithGenres_WithProvidedGenres_ShouldReturnCorrectMovies()
        {
            // arrange
            var dramaGenre = new Genre() { Name = "drama" };
            var horrorGenre = new Genre() { Name = "horror" };
            var comedyGenre = new Genre() { Name = "comedy" };

            var firstMovie = new Movie() {
                Title = "the fly", ReleaseDate = 1986, Genres = new Genre[] { dramaGenre, horrorGenre }
            };
            var secondMovie = new Movie() {
                Title = "gummo", ReleaseDate = 1997, Genres = new Genre[] { dramaGenre }
            };
            var thirdMovie = new Movie() {
                Title = "dumb and dumber", ReleaseDate = 1994, Genres = new Genre[] { comedyGenre }
            };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._unitOfWorkMock
                .Setup(u => u.Movies.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<Movie> actual = this._scanMoviesManager.GetMoviesWithGenres(visit, dramaGenre, horrorGenre);

            // assert
            actual.Should().BeEquivalentTo(new Movie[] { firstMovie, secondMovie });
        }

        [Fact]
        public void GetMoviesWithActors_WithProvidedActors_ShouldReturnCorrectMovies()
        {
            // arrange
            var firstActor = new Actor() { Name = "jeff goldblum" };
            var secondActor = new Actor() { Name = "bill pullman" };
            var thirdActor = new Actor() { Name = "jim carrey" };

            var firstMovie = new Movie() {
                Title = "the fly", ReleaseDate = 1986, Actors = new Actor[] { firstActor }
            };
            var secondMovie = new Movie() {
                Title = "independence day", ReleaseDate = 1996, Actors = new Actor[] { firstActor, secondActor }
            };
            var thirdMovie = new Movie() {
                Title = "dumb and dumber", ReleaseDate = 1994, Actors = new Actor[] { thirdActor }
            };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._unitOfWorkMock
                .Setup(u => u.Movies.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<Movie> actual = this._scanMoviesManager.GetMoviesWithActors(visit, firstActor, secondActor);

            // assert
            actual.Should().BeEquivalentTo(new Movie[] { firstMovie, secondMovie });
        }

        [Fact]
        public void GetMoviesWithDirectors_WithProvidedActors_ShouldReturnCorrectMovies()
        {
            // arrange
            var firstDirector = new Director() { Name = "benny safdie" };
            var secondDirector = new Director() { Name = "josh safdie" };
            var thirdDirector = new Director() { Name = "paul thomas anderson" };

            var firstMovie = new Movie() {
                Title = "uncut gems", ReleaseDate = 2019, Directors = new Director[] { firstDirector, secondDirector }
            };
            var secondMovie = new Movie() {
                Title = "there will be blood", ReleaseDate = 2007, Directors = new Director[] { thirdDirector }
            };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._unitOfWorkMock
                .Setup(u => u.Movies.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie });

            // act
            IEnumerable<Movie> actual = this._scanMoviesManager.GetMoviesWithDirectors(visit, firstDirector, secondDirector);

            // assert
            actual.Should().BeEquivalentTo(new Movie[] { firstMovie });

        }

        [Fact]
        public void GetMoviesWithReleaseDates_WithProvidedDates_ShouldReturnCorrectMovies()
        {
            // arrange
            var firstMovie = new Movie() { Title = "the fly", ReleaseDate = 1986 };
            var secondMovie = new Movie() {Title = "gummo", ReleaseDate = 1997 };
            var thirdMovie = new Movie() { Title = "dumb and dumber", ReleaseDate = 1994 };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<Movie> actual = this._scanMoviesManager.GetMoviesWithReleaseDates(visit, new int[] { 1994, 1995, 1996, 1997 });

            // assert
            var expected = new Movie[] { secondMovie, thirdMovie };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCountByGenre_ShouldReturnCorrectCount()
        {
            // arrange
            var dramaGenre = new Genre() { Name = "drama" };
            var horrorGenre = new Genre() { Name = "horror" };
            var comedyGenre = new Genre() { Name = "comedy" };

            var firstMovie = new Movie() { Title = "the fly", ReleaseDate = 1986, Genres = new Genre[] { dramaGenre, horrorGenre } };
            var secondMovie = new Movie() {Title = "gummo", ReleaseDate = 1997, Genres = new Genre[] { dramaGenre } };
            var thirdMovie = new Movie() { Title = "dumb and dumber", ReleaseDate = 1994, Genres = new Genre[] { comedyGenre } };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<KeyValuePair<Genre, int>> actual = this._scanMoviesManager.GetCountByGenre(visit);

            // assert
            var expected = new List<KeyValuePair<Genre, int>>() {
                new KeyValuePair<Genre, int>(dramaGenre, 2),
                new KeyValuePair<Genre, int>(horrorGenre, 1),
                new KeyValuePair<Genre, int>(comedyGenre, 1),
            };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCountByActor_ShouldReturnCorrectCount()
        {
            // arrange
            var firstActor = new Actor() { Name = "jeff goldblum" };
            var secondActor = new Actor() { Name = "bill pullman" };
            var thirdActor = new Actor() { Name = "jim carrey" };

            var firstMovie = new Movie() { Title = "the fly", ReleaseDate = 1986, Actors = new Actor[] { firstActor, secondActor } };
            var secondMovie = new Movie() { Title = "independence day", ReleaseDate = 1996, Actors = new Actor[] { firstActor } };
            var thirdMovie = new Movie() { Title = "dumb and dumber", ReleaseDate = 1994, Actors = new Actor[] { thirdActor } };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<KeyValuePair<Actor, int>> actual = this._scanMoviesManager.GetCountByActor(visit);

            // assert
            var expected = new List<KeyValuePair<Actor, int>>() {
                new KeyValuePair<Actor, int>(firstActor, 2),
                new KeyValuePair<Actor, int>(secondActor, 1),
                new KeyValuePair<Actor, int>(thirdActor, 1)
            };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetCountByDirector_ShouldReturnCorrectCount()
        {
            // arrange
            var firstDirector = new Director() { Name = "benny safdie" };
            var secondDirector = new Director() { Name = "josh safdie" };
            var thirdDirector = new Director() { Name = "paul thomas anderson" };

            var firstMovie = new Movie() { Title = "uncut gems", ReleaseDate = 2019, Directors = new Director[] { firstDirector, secondDirector } };
            var secondMovie = new Movie() { Title = "there will be blood", ReleaseDate = 2007, Directors = new Director[] { thirdDirector } };
            var thirdMovie = new Movie() { Title = "Licorice Pizza", ReleaseDate = 2021, Directors = new Director[] { thirdDirector } };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<KeyValuePair<Director, int>> actual = this._scanMoviesManager.GetCountByDirector(visit);

            // assert
            var expected = new List<KeyValuePair<Director, int>>() {
                new KeyValuePair<Director, int>(thirdDirector, 2),
                new KeyValuePair<Director, int>(secondDirector, 1),
                new KeyValuePair<Director, int>(firstDirector, 1)
            };
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("Licorice Pizza")]
        [InlineData("Licorice Pizza 2021")]
        [InlineData("Licorice Pizza (2021)")]
        [InlineData("licorice pizza")]
        [InlineData("licorice pizza 2021")]
        [InlineData("licorice pizza (2021)")]
        [InlineData(" licorice   piZZa")]
        [InlineData(" licorice ! piZZa 2021 -->")]
        [InlineData("??? licorice ==> piZZa (2021)%%$$##")]
        public void SearchMovieEntitiesByTitle_ShouldReturnCorrectMatches(string title)
        {
            // arrange
            var firstMovie = new Movie() { Title = "uncut gems", ReleaseDate = 2019 };
            var secondMovie = new Movie() { Title = "there will be blood", ReleaseDate = 2007 };
            var thirdMovie = new Movie() { Title = "Licorice Pizza", ReleaseDate = 2021 };

            var visit = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visit))
                .Returns(new Movie[] { firstMovie, secondMovie, thirdMovie });

            // act
            IEnumerable<Movie> actual = this._scanMoviesManager.SearchMovieEntitiesByTitle(visit, title);

            // assert
            var expected = new Movie[] { thirdMovie };
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetVisitDiff_WithTwoNullVisits_ShouldThrowArgumentNullException()
        {
            // arrange
            // nothing to do...

            // act
            // nothing to do...

            // assert
            this._scanMoviesManager.Invoking(s => s.GetVisitDiff(null, null)).Should().Throw<ArgumentNullException>();

        }

        [Fact]
        public void GetVisitDiff_WithNullVisitRight_ShouldThrowArgumentNullException()
        {
            // arrange
            var visitLeft = new MovieWarehouseVisit();

            // act
            // nothing to do...

            // assert
            this._scanMoviesManager.Invoking(s => s.GetVisitDiff(visitLeft, null)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetVisitDiff_WithVisitLeftMoreRecentThanVisitRigh_ShouldThrowArgumentException()
        {
            // arrange
            var visitLeft = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220102", "yyyyMMdd", null) };
            var visitRight = new MovieWarehouseVisit() { VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null) };

            // act
            // nothing to to...

            //assert
            this._scanMoviesManager
                .Invoking(r => r.GetVisitDiff(visitLeft, visitRight))
                .Should()
                .Throw<ArgumentException>();
        }

        [Fact]
        public void GetVisitDiff_WithOnlyVisitRight_ShouldReturnEmptyEnumerableInRemovedKeyAndAllRipsInAddedKey()
        {
            // arrange
            var firstRip = new MovieRip() {
                FileName = "Face.Off.1997.iNTERNAL.1080p.BluRay.x264-MARS[rarbg]",
                Movie = new Movie() { Title = "Face Off", ReleaseDate = 1997 }
            };
            var secondRip = new MovieRip() {
                FileName = "Gummo.1997.DVDRip.XviD-DiSSOLVE",
                Movie = new Movie() { Title = "Gummo", ReleaseDate = 1997 }
            };
            var thirdRip = new MovieRip() {
                FileName = "Papillon.1973.1080p.BluRay.X264-AMIABLE",
                Movie = new Movie() { Title = "Papillon", ReleaseDate = 1973 }
            };
            var visitRight = new MovieWarehouseVisit() {
                MovieRips = new List<MovieRip>() { firstRip, secondRip, thirdRip },
                VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null)
                };
            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visitRight))
                .Returns(visitRight.MovieRips.Select(mr => mr.Movie));

            // act
            Dictionary<string, IEnumerable<string>> visitDiff = this._scanMoviesManager.GetVisitDiff(null, visitRight);

            // assert
            using (new AssertionScope())
            {
                visitDiff["removed"].Should().BeEmpty();
                visitDiff["added"].Should().BeEquivalentTo(new string[] {
                    firstRip.Movie.ToString(),
                    secondRip.Movie.ToString(),
                    thirdRip.Movie.ToString() });
            }
        }

        [Fact]
        public void GetVisitDiff_WithTwoVisits_ShouldReturnCorrectDifference()
        {
            // arrange
            var movieWithTwoRips = new Movie() { Title = "Wake In Fright", ReleaseDate = 1971 };

            var firstRip = new MovieRip() {
                FileName = "Face.Off.1997.iNTERNAL.1080p.BluRay.x264-MARS[rarbg]",
                Movie = new Movie() { Title = "Face Off", ReleaseDate = 1997 }
            };
            var secondRip = new MovieRip() {
                FileName = "Wake.In.Fright.1971.1080p.BluRay.H264.AAC-RARBG",
                Movie = movieWithTwoRips
            };
            var thirdRip = new MovieRip() {
                FileName = "Gummo.1997.DVDRip.XviD-DiSSOLVE",
                Movie = new Movie() { Title = "Gummo", ReleaseDate = 1997 }
            };
            var fourthRip = new MovieRip() {
                FileName = "Wake.In.Fright.1971.1080p.BluRay.x264.DD2.0-FGT",
                Movie = movieWithTwoRips
            };

            var visitLeft = new MovieWarehouseVisit() {
                MovieRips = new List<MovieRip>() { firstRip, secondRip },
                VisitDateTime = DateTime.ParseExact("20220101", "yyyyMMdd", null)
            };
            var visitRight = new MovieWarehouseVisit() {
                MovieRips = new List<MovieRip>() { thirdRip, fourthRip },
                VisitDateTime = DateTime.ParseExact("20220102", "yyyyMMdd", null)
            };

            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visitLeft))
                .Returns(visitLeft.MovieRips.Select(mr => mr.Movie));
            this._movieRepositoryMock
                .Setup(m => m.GetAllMoviesInVisit(visitRight))
                .Returns(visitRight.MovieRips.Select(mr => mr.Movie));

            // act
            Dictionary<string, IEnumerable<string>> visitDiff = this._scanMoviesManager.GetVisitDiff(visitLeft, visitRight);

            // assert
            IEnumerable<string> removedExpected = new string[] { firstRip.Movie.ToString() };
            IEnumerable<string> addedExpected = new string[] { thirdRip.Movie.ToString() };
            using (new AssertionScope())
            {
                visitDiff["removed"].Should().BeEquivalentTo(removedExpected);
                visitDiff["added"].Should().BeEquivalentTo(addedExpected);
            }
        }

    }
}
