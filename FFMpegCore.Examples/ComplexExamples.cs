using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore.Examples;

public static class ComplexExamples
{
    // One decode, three renditions: split hands the decoded video to three chains, and each output maps one of
    // them. OutputToMany would otherwise decode the input once per output.
    public static void EncodeRenditionLadder(string input, string outputFolder)
    {
        FFMpegArguments
            .FromFileInput(input)
            .OutputToMany(outputs => outputs
                .OutputToFile(Path.Combine(outputFolder, "1080.mp4"), options => options
                    .WithComplexFilter(graph => graph
                        .From(0, StreamType.Video)
                        .WithCustomFilter("split", "3")
                        .As("full", "half", "quarter")
                        .From("half")
                        .WithFilter(new ScaleArgument(1280, 720))
                        .As("v720")
                        .From("quarter")
                        .WithFilter(new ScaleArgument(854, 480))
                        .As("v480"))
                    .WithMap("full")
                    .WithMap(0, StreamType.Audio)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(20)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(AudioQuality.Good)
                    .WithFastStart())
                .OutputToFile(Path.Combine(outputFolder, "720.mp4"), options => options
                    .WithMap("v720")
                    .WithMap(0, StreamType.Audio)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(22)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(AudioQuality.Normal)
                    .WithFastStart())
                .OutputToFile(Path.Combine(outputFolder, "480.mp4"), options => options
                    .WithMap("v480")
                    .WithMap(0, StreamType.Audio)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(24)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(AudioQuality.Low)
                    .WithFastStart()))
            .ProcessSynchronously();
    }

    // A logo faded into the corner and a music bed mixed under the original audio.
    public static void BrandWithOverlayAndMusicBed(string video, string logo, string music, string output)
    {
        FFMpegArguments
            .FromFileInput(video)
            // a still image is one frame, and one frame cannot fade; looping it gives the fade something to run over
            .AddFileInput(logo, options => options
                .WithLoop(1)
                .WithFrameRate(25))
            .AddFileInput(music)
            .OutputToFile(output, options => options
                .WithComplexFilter(graph => graph
                    .From(1, StreamType.Video)
                    .WithFilter(new ScaleArgument(240, -1))
                    .WithFilter(new VideoFadeArgument(FadeDirection.In, TimeSpan.Zero, TimeSpan.FromSeconds(1)))
                    .As("logo")
                    .From(0, StreamType.Video)
                    .From("logo")
                    // the looped logo never ends, so the overlay has to stop with the video
                    .Overlay("W-w-24", "H-h-24", shortest: true)
                    .As("v")
                    .From(2, StreamType.Audio)
                    .WithCustomFilter("volume", "0.15")
                    .As("bed")
                    .From(0, StreamType.Audio)
                    .From("bed")
                    .AudioMix(2, "first")
                    .As("a"))
                .WithMap("v")
                .WithMap("a")
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(21)
                .WithAudioCodec(AudioCodec.Aac))
            .ProcessSynchronously();
    }

    // Two encodes of the same source, labelled and stacked, for A/B review.
    public static void BuildComparison(string before, string after, string fontFile, string output)
    {
        FFMpegArguments
            .FromFileInput(before)
            .AddFileInput(after)
            .OutputToFile(output, options => options
                .WithComplexFilter(graph => graph
                    .From(0, StreamType.Video)
                    // -2 rounds the height to something h264 can encode, whatever the source aspect ratio is
                    .WithFilter(new ScaleArgument(960, -2))
                    .WithFilter(new DrawTextArgument(Label("Before", fontFile)))
                    .As("left")
                    .From(1, StreamType.Video)
                    .WithFilter(new ScaleArgument(960, -2))
                    .WithFilter(new DrawTextArgument(Label("After", fontFile)))
                    .As("right")
                    .From("left")
                    .From("right")
                    .WithCustomFilter("hstack", "inputs=2")
                    .As("v"))
                .WithMap("v")
                .WithMap(0, StreamType.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(18))
            .ProcessSynchronously();

        static DrawTextOptions Label(string text, string fontFile)
        {
            return DrawTextOptions.Create(text, fontFile,
                ("x", "24"),
                ("y", "24"),
                ("fontsize", "42"),
                ("fontcolor", "white"),
                ("box", "1"),
                ("boxcolor", "black@0.6"));
        }
    }

    // An audio-only episode turned into the video file the platforms insist on: a still poster, loudness-normalised
    // audio, and chapters written into the container.
    public static async Task RenderPodcastEpisode(string poster, string audio, string output, CancellationToken cancellationToken)
    {
        var source = await FFProbe.AnalyseAsync(audio, cancellationToken: cancellationToken);

        var metadata = new FFMetadataBuilder()
            .WithTitle("Episode 42")
            .WithArtists("The Hosts")
            .WithGenres("Podcast")
            .WithChapter("Cold open", TimeSpan.FromMinutes(3))
            .WithChapter("Interview", TimeSpan.FromMinutes(34))
            .WithChapter("Outro", TimeSpan.FromMinutes(5));

        await FFMpegArguments
            .FromFileInput(poster, options => options
                .WithLoop(1)
                .WithFrameRate(2))
            .AddFileInput(audio)
            .AddMetadata(metadata)
            .OutputToFile(output, options => options
                .WithMap(0, StreamType.Video)
                .WithMap(1, StreamType.Audio)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(28)
                .WithPixelFormat("yuv420p")
                .WithAudioFilters(filters => filters
                    .HighPass(80)
                    .Loudnorm(-16, 7, -1.5))
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good)
                // the looped poster never ends either; the audio is what decides the length
                .WithShortest()
                .WithFastStart())
            .NotifyOnPercentageProgress(percent => Console.WriteLine($"{percent:0.#}%"), source.Duration)
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();
    }

    // A QC pass: decode once, write nothing, and read what the detection filters report on stderr.
    public static IReadOnlyList<string> ScanForBlackAndSilence(string input)
    {
        var findings = new List<string>();

        FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(OperatingSystem.IsWindows() ? "NUL" : "/dev/null", options => options
                .WithVideoFilters(filters => filters.BlackDetect(0.5))
                .WithAudioFilters(filters => filters.SilenceDetect("db", -45, 1))
                .ForceFormat("null"))
            // blackdetect and silencedetect report at info level; a quieter log level drops them silently
            .WithLogLevel(FFMpegLogLevel.Info)
            .NotifyOnError(line =>
            {
                if (line.Contains("black_start") || line.Contains("silence_start"))
                {
                    findings.Add(line.Trim());
                }
            })
            .ProcessSynchronously();

        return findings;
    }
}
