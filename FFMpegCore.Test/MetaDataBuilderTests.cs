using System.Text.RegularExpressions;
using FFMpegCore.Builders.MetaData;

namespace FFMpegCore.Test;

[TestClass]
public class MetaDataBuilderTests
{
    [TestMethod]
    public void TestMetaDataBuilderIntegrity()
    {
        var source = new
        {
            Album = "Kanon und Gigue",
            Artist = "Pachelbel",
            Title = "Kanon und Gigue in D-Dur",
            Copyright = "Copyright Lol",
            Composer = "Pachelbel",
            Genres = new[] { "Synthwave", "Classics" },
            Tracks = new[]
            {
                new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 01" }, new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 02" },
                new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 03" }, new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 04" }
            }
        };

        var builder = new MetaDataBuilder()
            .WithTitle(source.Title)
            .WithArtists(source.Artist)
            .WithComposers(source.Composer)
            .WithAlbumArtists(source.Artist)
            .WithGenres(source.Genres)
            .WithCopyright(source.Copyright)
            .AddChapters(source.Tracks, x => (x.Duration, x.Title));

        var metadata = builder.Build();
        var serialized = MetaDataSerializer.Instance.Serialize(metadata);

        Assert.IsTrue(serialized.StartsWith(";FFMETADATA1", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(serialized.Contains("genre=Synthwave; Classics", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(serialized.Contains("title=Chapter 01", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(serialized.Contains("album_artist=Pachelbel", StringComparison.OrdinalIgnoreCase));
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

    [TestMethod]
    public void Builder_NamedEntries_SerializeToFFMetadataKeys()
    {
        var serialized = MetaDataSerializer.Instance.Serialize(new MetaDataBuilder()
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
            .WithEncoder("Lavf")
            .Build());

        var lines = serialized.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
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
    public void Builder_WithEntry_AppendsToExistingKey()
    {
        var metaData = new MetaDataBuilder()
            .WithEntry("artist", "First")
            .WithEntry("artist", "Second")
            .WithEntry("artist", new List<string> { "Third", "Fourth" })
            .Build();

        Assert.AreEqual("First; Second; Third; Fourth", metaData.Entries["artist"]);
    }

    [TestMethod]
    public void Builder_WithEntry_ReplacesBlankExistingValue()
    {
        var metaData = new MetaDataBuilder()
            .WithEntry("title", " ")
            .WithEntry("title", "Real title")
            .Build();

        Assert.AreEqual("Real title", metaData.Entries["title"]);
    }

    [TestMethod]
    public void Builder_AddChapter_ByDuration_ChainsFromPreviousEnd()
    {
        var metaData = new MetaDataBuilder()
            .AddChapter(TimeSpan.FromSeconds(10), "One")
            .AddChapter(TimeSpan.FromSeconds(5))
            .AddChapter(new ChapterData("Explicit", TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(110)))
            .Build();

        Assert.HasCount(3, metaData.Chapters);
        Assert.AreEqual(("One", TimeSpan.Zero, TimeSpan.FromSeconds(10)), (metaData.Chapters[0].Title, metaData.Chapters[0].Start, metaData.Chapters[0].End));
        Assert.AreEqual(("Chapter 2", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15)), (metaData.Chapters[1].Title, metaData.Chapters[1].Start, metaData.Chapters[1].End));
        Assert.AreEqual(("Explicit", TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(110)), (metaData.Chapters[2].Title, metaData.Chapters[2].Start, metaData.Chapters[2].End));
        Assert.AreEqual(TimeSpan.FromSeconds(10), metaData.Chapters[2].Duration);
    }

    [TestMethod]
    public void Serializer_Chapters_UseMillisecondTimebase_AndDefaultTitles()
    {
        var serialized = MetaDataSerializer.Instance.Serialize(new MetaDataBuilder()
            .AddChapter(new ChapterData("", TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3)))
            .Build());

        var lines = serialized.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        CollectionAssert.AreEqual(new[]
        {
            ";FFMETADATA1",
            "[CHAPTER]",
            "TIMEBASE=1/1000",
            "START=1500",
            "END=3000",
            "title=Chapter 1"
        }, lines);
    }

    [TestMethod]
    public void Build_ReturnsSnapshot_DetachedFromBuilder()
    {
        var builder = new MetaDataBuilder().WithTitle("Before").AddChapter(TimeSpan.FromSeconds(1));
        var snapshot = builder.Build();

        builder.WithTitle("After").AddChapter(TimeSpan.FromSeconds(1));

        Assert.AreEqual("Before", snapshot.Entries["title"]);
        Assert.HasCount(1, snapshot.Chapters);
    }

    [TestMethod]
    public void MetaData_CloneConstructor_CopiesEntriesAndChapters()
    {
        var original = new MetaData();
        original.Entries["title"] = "Title";
        original.Chapters.Add(new ChapterData("One", TimeSpan.Zero, TimeSpan.FromSeconds(1)));

        var clone = new MetaData(original);
        original.Entries["title"] = "Changed";
        original.Chapters.Clear();

        Assert.AreEqual("Title", clone.Entries["title"]);
        Assert.HasCount(1, clone.Chapters);
        Assert.AreEqual("One", ((IReadOnlyMetaData)clone).Chapters[0].Title);
        Assert.AreEqual("Title", ((IReadOnlyMetaData)clone).Entries["title"]);
    }

    [TestMethod]
    public void FFMetadataBuilder_And_MetaDataBuilder_ProduceEquivalentOutput()
    {
        var legacy = MetaDataSerializer.Instance.Serialize(new MetaDataBuilder()
            .WithTitle("Title")
            .WithArtists("Artist")
            .AddChapter(TimeSpan.FromSeconds(10), "One")
            .AddChapter(TimeSpan.FromSeconds(5), "Two")
            .Build());

        var current = FFMetadataBuilder.Empty()
            .WithTag("title", "Title")
            .WithTag("artist", "Artist")
            .WithChapter("One", 10_000L)
            .WithChapter("Two", 5.0)
            .GetMetadataFileContent();

        Assert.AreEqual(legacy, current);
    }
}
