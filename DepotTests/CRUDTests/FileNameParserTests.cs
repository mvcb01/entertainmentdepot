using Xunit;
using FluentAssertions;
using FilmDomain.Entities;

using FilmCRUD.Helpers;

namespace DepotTests.CRUDTests
{
    public class FileNameParserTests
    {
        [Theory]
        [InlineData(
            "The.Deer.Hunter.1978.REMASTERED.1080p.BluRay.x264.DTS-HD.MA.5.1-FGT",
            "The Deer Hunter",
            "1978",
            "1080p",
            "BluRay.x264.DTS-HD.MA.5.1",
            "FGT"
            )]
        [InlineData(
            "Khrustalyov.My.Car.1998.720p.BluRay.x264-GHOULS[rarbg]",
            "khrustalyov my car",
            "1998",
            "720p",
            "BluRay.x264",
            "GHOULS[rarbg]"
            )]
        [InlineData(
            "Sicario 2015 1080p BluRay x264 AC3-JYK",
            "sicario",
            "2015",
            "1080p",
            "BluRay x264 AC3",
            "JYK"
            )]
        [InlineData(
            "The.Lives.of.Others.2006.GERMAN.REMASTERED.1080p.BluRay.x264.DTS-NOGRP",
            "the lives of others",
            "2006",
            "1080p",
            "BluRay.x264.DTS",
            "NOGRP"
            )]
        [InlineData(
            "Terminator.2.Judgement.Day.1991.Extended.REMASTERED.1080p.BluRay.H264.AAC.READ.NFO-RARBG",
            "terminator 2 judgement day",
            "1991",
            "1080p",
            "BluRay.H264.AAC.READ.NFO",
            "RARBG"
            )]
        [InlineData(
            "A.Hero.2021.1080p.AMZN.WEBRip.DDP5.1.x264-TEPES",
            "A Hero",
            "2021",
            "1080p",
            "AMZN.WEBRip.DDP5.1.x264",
            "TEPES"
            )]
        [InlineData(
            "Better.Things.2008.FESTiVAL.DVDRip.XviD-NODLABS",
            "Better things",
            "2008",
            "DVDRip",
            "XviD",
            "NODLABS"
            )]
        [InlineData(
            "Ex Drummer (2007)",
            "ex drummer",
            "2007",
            null,
            null,
            null
            )]
        [InlineData(
            "Idiocracy.2006.WEB-DL.1080p.x264.anoXmous",
            "idiocracy",
            "2006",
            "1080p",
            "x264",
            "anoXmous"
            )]
        public void ParseFileNameIntoMovieRip_ShouldReturnCorrectComponents(
            string fileName,
            string title,
            string releasedDate,
            string ripQuality,
            string ripInfo,
            string ripGroup)
        {
            MovieRip actual = FileNameParser.ParseFileNameIntoMovieRip(fileName);
            title.Should().BeEquivalentTo(actual.ParsedTitle);
            releasedDate.Should().BeEquivalentTo(actual.ParsedReleaseDate);
            ripQuality.Should().BeEquivalentTo(actual.ParsedRipQuality);
            ripInfo.Should().BeEquivalentTo(actual.ParsedRipInfo);
            ripGroup.Should().BeEquivalentTo(actual.ParsedRipGroup);
        }

        [Theory]
        [InlineData("The Tragedy Of Macbeth (2021)", "the tragedy of macbeth", "2021")]
        [InlineData("Cop Car 2015 ", "Cop Car", "2015")]
        [InlineData("  Khrustalyov.My.Car.1998", "khrustalyov my car", "1998")]
        public void SplitTitleAndReleaseDate_ShouldReturnCorrectTitleAndReleaseDate(
            string titleAndRelaseDate,
            string expectedTitle,
            string expectedReleaseDate)
        {
            var result = FileNameParser.SplitTitleAndReleaseDate(titleAndRelaseDate);

            var actualTitle = result[0];
            var actualReleaseDate = result[1];

            // BeEquivalentTo - ignora lowercase VS uppercase
            actualTitle.Should().BeEquivalentTo(expectedTitle);
            actualReleaseDate.Should().BeEquivalentTo(expectedReleaseDate);
        }

        [Theory]
        [InlineData("BluRay.x264-GECKOS", "BluRay.x264", "GECKOS")]
        [InlineData("BluRay.H264.AAC-VXT", "BluRay.H264.AAC", "VXT")]
        [InlineData("BluRay x264 DTS-JYK", "BluRay x264 DTS", "JYK")]
        [InlineData("BluRay.x264.DTS-HD.MA.5.1-FGT", "BluRay.x264.DTS-HD.MA.5.1", "FGT")]
        [InlineData("BluRay x264 Mayan AAC - Ozlem", "BluRay x264 Mayan AAC", "Ozlem")]
        [InlineData("BluRay.x264.anoXmous", "BluRay.x264", "anoXmous")]
        [InlineData("BDRip.XviD-Larceny", "BDRip.XviD", "Larceny")]
        [InlineData("[DvdRip] [Xvid] {1337x}-Noir", "[DvdRip] [Xvid] {1337x}", "Noir")]
        [InlineData("BluRay 5.1 Ch x265 HEVC SUJAIDR", "BluRay 5.1 Ch x265 HEVC", "SUJAIDR")]
        public void SplitRipInfoAndGroup_ShouldReturnCorrectInfoAndGroup(
            string ripInfoAndGroup,
            string expectedRipInfo,
            string expectedRipGroup)
        {
            var result = FileNameParser.SplitRipInfoAndGroup(ripInfoAndGroup);
            var actualRipInfo = result[0];
            var actualRipGroup = result[1];

            actualRipInfo.Should().BeEquivalentTo(expectedRipInfo);
            actualRipGroup.Should().BeEquivalentTo(expectedRipGroup);
        }

    }
}