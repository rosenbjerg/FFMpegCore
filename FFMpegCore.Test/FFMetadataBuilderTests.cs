using System.Text.RegularExpressions;

namespace FFMpegCore.Test;

[TestClass]
public class FFMetadataBuilderTests
{
    private static string[] Lines(FFMetadataBuilder builder)
    {
        return builder.GetMetadataFileContent().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    }

    [TestMethod]
    public void NamedTags_SerializeToFFMetadataKeys()
    {
        var lines = Lines(FFMetadataBuilder.Empty()
            .WithMajorBrand("M4A")
            .WithMinorVersion("512")
            .WithCompatibleBrands("M4A isomiso2")
            .WithCopyright("(c) 2024")
            .WithTitle("Title")
            .WithArtists("Artist A", "Artist B")
            .WithComposers(new List<string> { "Composer" })
            .WithAlbumArtists("Album Artist")
            .WithAlbum("Album")
            .WithDate("2024")
            .WithGenres(new List<string> { "Genre" })
            .WithComments("Comment A", "Comment B")
            .WithEncoder("Lavf"));

        CollectionAssert.AreEqual(new[]
        {
            ";FFMETADATA1",
            "major_brand=M4A",
            "minor_version=512",
            "compatible_brands=M4A isomiso2",
            "copyright=(c) 2024",
            "title=Title",
            "artist=Artist A; Artist B",
            "composer=Composer",
            "album_artist=Album Artist",
            "album=Album",
            "date=2024",
            "genre=Genre",
            "comment=Comment A; Comment B",
            "encoder=Lavf"
        }, lines);
    }

    [TestMethod]
    public void WithTag_LastValueWins()
    {
        var lines = Lines(FFMetadataBuilder.Empty()
            .WithTag("title", "First")
            .WithTag("title", "Second"));

        CollectionAssert.AreEqual(new[] { ";FFMETADATA1", "title=Second" }, lines);
    }

    [TestMethod]
    public void WithTag_MultipleValues_AreJoined()
    {
        var lines = Lines(FFMetadataBuilder.Empty()
            .WithTag("artist", "A", "B")
            .WithTag("genre", new List<string> { "C", "D" }));

        CollectionAssert.AreEqual(new[] { ";FFMETADATA1", "artist=A; B", "genre=C; D" }, lines);
    }

    [TestMethod]
    public void SequentialChapters_StartWhereThePreviousEnded()
    {
        var lines = Lines(FFMetadataBuilder.Empty()
            .WithChapter("One", TimeSpan.FromSeconds(10))
            .WithChapter("Two", 5_000L)
            .WithChapter("Three", 1.5)
            .WithChapters(new[] { 2, 3 }, seconds => (TimeSpan.FromSeconds(seconds), $"Chapter of {seconds}s")));

        CollectionAssert.AreEqual(new[]
        {
            ";FFMETADATA1",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=0", "END=10000", "title=One",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=10000", "END=15000", "title=Two",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=15000", "END=16500", "title=Three",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=16500", "END=18500", "title=Chapter of 2s",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=18500", "END=21500", "title=Chapter of 3s"
        }, lines);
    }

    [TestMethod]
    public void ExplicitChapters_KeepTheirTimes_AndGetDefaultTitles()
    {
        var lines = Lines(FFMetadataBuilder.Empty()
            .WithChapter("", TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(110))
            .WithChapter(new ChapterData("From probe", TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3)))
            .WithChapter("After explicit", TimeSpan.FromSeconds(1)));

        CollectionAssert.AreEqual(new[]
        {
            ";FFMETADATA1",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=100000", "END=110000", "title=Chapter 1",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=1500", "END=3000", "title=From probe",
            "[CHAPTER]", "TIMEBASE=1/1000", "START=3000", "END=4000", "title=After explicit"
        }, lines);
    }

    [TestMethod]
    public void ChapterData_ExposesDuration()
    {
        var chapter = new ChapterData("One", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));

        Assert.AreEqual(TimeSpan.FromSeconds(3), chapter.Duration);
    }

    [TestMethod]
    public void TestMapMetadata()
    {
        //-i "whaterver0" // index: 0
        //-f concat -safe 0
        //-i "\AppData\Local\Temp\concat_b511f2bf-c4af-4f71-b9bd-24d706bf4861.txt"   // index: 1
        //-i "\AppData\Local\Temp\metadata_210d3259-3d5c-43c8-9786-54b5c414fa70.txt" // index: 2
        //-map_metadata 2

        var text0 = FFMpegArguments.FromFileInput("whaterver0")
            .AddMetaData("WhatEver3")
            .Text;

        var text1 = FFMpegArguments.FromFileInput("whaterver0")
            .AddDemuxConcatInput(new[] { "whaterver", "whaterver1" })
            .AddMetaData("WhatEver3")
            .Text;

        Assert.IsTrue(Regex.IsMatch(text0, "metadata_[0-9a-f-]+\\.txt\" -map_metadata 1"), "map_metadata index is calculated incorrectly.");
        Assert.IsTrue(Regex.IsMatch(text1, "metadata_[0-9a-f-]+\\.txt\" -map_metadata 2"), "map_metadata index is calculated incorrectly.");
    }
}
