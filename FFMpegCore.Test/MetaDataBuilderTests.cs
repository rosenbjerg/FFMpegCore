using FFMpegCore.Builders.MetaData;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.Test
{
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
                    new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 01" },
                    new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 02" },
                    new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 03" },
                    new { Duration = TimeSpan.FromSeconds(10), Title = "Chapter 04" },
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
    }
}
