﻿using Xunit;
using FluentAssertions;
using System;
using System.Linq;
using FilmCRUD.CustomExceptions;
using MovieAPIClients;
using FilmCRUD;
using FilmDomain.Entities;
using System.Runtime.CompilerServices;

namespace DepotTests.CRUDTests
{
    /// <summary>
    /// To test method <c>RipToMovieLinker.SearchMovieAndPickFromResultsAsync</c>.
    /// </summary>
    public class RipToMovieLinkerMovieSearchTests : RipToMovieLinkerTestsBase
    {

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithNoSearchResults_ShouldThrowNoSearchResultsError()
        //{
        //    // arrange
        //    string titleWithNoResults = "Some movie";

        //    // act
        //    // nothing to do

        //    // assert
        //    Action methodCall = () => RipToMovieLinker.SearchMovieAndPickFromResultsAsync(Enumerable.Empty<MovieSearchResult>(), titleWithNoResults);
        //    methodCall.Should().Throw<NoSearchResultsError>();
        //}


        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResultsAndWithoutProvidedReleaseDate_ShouldThrowMultipleSearchResultsError()
        //{
        //    // arrange
        //    string movieTitleToSearch = "the fly";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "The Fly", ReleaseDate = 1986 },
        //        new MovieSearchResult()  { Title = "The Fly", ReleaseDate = 1958 },
        //        };

        //    // act
        //    // nothing to do

        //    // assert
        //    Action methodCall = () => RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch);
        //    methodCall.Should().Throw<MultipleSearchResultsError>();
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResultsAndProvidedReleaseDate_ShouldReturnMatchingMovieRip()
        //{
        //    // arrange
        //    string movieTitleToSearch = "the fly";
        //    string movieReleaseDateToSearch = "1986";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "The Fly", ReleaseDate = 1986 },
        //        new MovieSearchResult()  { Title = "The Fly", ReleaseDate = 1958 },
        //        };

        //    // act
        //    Movie movieFound = RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch, movieReleaseDateToSearch);

        //    // assert
        //    movieFound.Should().BeEquivalentTo(new { Title = "The Fly", ReleaseDate = 1986 });
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithProvidedReleaseDate_WithoutDateMatch_ShouldReturnResultWithinDateTolerance()
        //{
        //    // arrange
        //    string movieTitleToSearch = "The Death of Dick Long";
        //    string movieReleaseDateToSearch = "2019";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "The Death of Dick Long", ReleaseDate = 2013 },
        //        new MovieSearchResult()  { Title = "The Death of Dick Long", ReleaseDate = 2020 },
        //        };

        //    // act
        //    Movie movieFound = RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch, movieReleaseDateToSearch);

        //    // assert
        //    movieFound.Should().BeEquivalentTo(new { Title = "The Death of Dick Long", ReleaseDate = 2020 });
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResultsAndProvidedReleaseDateWithoutMatch_ShouldThrowNoSearchResultsError()
        //{
        //    // arrange
        //    string movieTitleToSearch = "the fly";
        //    string movieReleaseDateToSearch = "1900";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "The Fly", ReleaseDate = 1986, ExternalId = 1 },
        //        new MovieSearchResult()  { Title = "The Fly", ReleaseDate = 1958, ExternalId = 2 },
        //        };

        //    // act
        //    // nothing to do

        //    // assert
        //    Action methodCall = () => RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch, movieReleaseDateToSearch);
        //    methodCall.Should().Throw<NoSearchResultsError>();
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResultsWithSameReleaseDate_ShouldThrowMultipleSearchResultsError()
        //{
        //    // arrange
        //    string movieTitleToSearch = "the fly";
        //    string movieReleaseDateToSearch = "1986";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "The Fly", ReleaseDate = 1986, ExternalId = 1 },
        //        new MovieSearchResult()  { Title = "The Fly", ReleaseDate = 1986, ExternalId = 2 },
        //        };

        //    // act
        //    // nothing to do

        //    // assert
        //    Action methodCall = () => RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch, movieReleaseDateToSearch);
        //    methodCall.Should().Throw<MultipleSearchResultsError>();
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResults_ShouldReturnTheResultWithTitleExactMatch()
        //{
        //    // arrange
        //    string movieTitleToSearch = "sorcerer";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { Title = "Sorcerer", ReleaseDate = 1977},
        //        new MovieSearchResult()  { Title = "Highlander III: The Sorcerer", ReleaseDate = 1994},
        //        };

        //    // act
        //    Movie movieFound = RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch);

        //    // assert
        //    movieFound.Should().BeEquivalentTo(new { Title = "Sorcerer", ReleaseDate = 1977 });
        //}

        //[Fact]
        //public void SearchMovieAndPickFromResultsAsync_WithSeveralSearchResults_ShouldReturnTheResultWithOriginalTitleExactMatch()
        //{
        //    // arrange
        //    string movieTitleToSearch = "the fly";
        //    MovieSearchResult[] searchResults = {
        //        new MovieSearchResult() { OriginalTitle = "The Fly", ReleaseDate = 1986 },
        //        new MovieSearchResult()  { Title = "Curse of the Fly", ReleaseDate = 1965 },
        //        };

        //    // act
        //    Movie movieFound = RipToMovieLinker.SearchMovieAndPickFromResultsAsync(searchResults, movieTitleToSearch);

        //    // assert
        //    movieFound.Should().BeEquivalentTo(new { OriginalTitle = "The Fly", ReleaseDate = 1986 });
        //}
    }
}