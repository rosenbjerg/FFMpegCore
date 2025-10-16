﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore;

public static class SnapshotArgumentBuilder
{
    public static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildSnapshotArguments(
        string input,
        string output,
        IMediaAnalysis source,
        Size? size = null,
        TimeSpan? captureTime = null,
        int? streamIndex = null,
        int inputFileIndex = 0)
    {
        return BuildSnapshotArguments(input, VideoCodec.Image.GetByExtension(output), source, size, captureTime, streamIndex, inputFileIndex);
    }

    public static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildSnapshotArguments(
        string input,
        IMediaAnalysis source,
        Size? size = null,
        TimeSpan? captureTime = null,
        int? streamIndex = null,
        int inputFileIndex = 0)
    {
        return BuildSnapshotArguments(input, VideoCodec.Image.Png, source, size, captureTime, streamIndex, inputFileIndex);
    }

    private static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildSnapshotArguments(
        string input,
        Codec codec,
        IMediaAnalysis source,
        Size? size = null,
        TimeSpan? captureTime = null,
        int? streamIndex = null,
        int inputFileIndex = 0)
    {
        captureTime ??= TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);
        size = PrepareSnapshotSize(source, size);
        streamIndex ??= source.PrimaryVideoStream?.Index
                        ?? source.VideoStreams.FirstOrDefault()?.Index
                        ?? 0;

        return (FFMpegArguments
                .FromFileInput(input, false, options => options
                    .Seek(captureTime)),
            options => options
                .SelectStream((int)streamIndex, inputFileIndex)
                .WithVideoCodec(codec)
                .WithFrameOutputCount(1)
                .Resize(size));
    }

    public static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildGifSnapshotArguments(
        string input,
        IMediaAnalysis source,
        Size? size = null,
        TimeSpan? captureTime = null,
        TimeSpan? duration = null,
        int? streamIndex = null,
        int fps = 12)
    {
        var defaultGifOutputSize = new Size(480, -1);

        captureTime ??= TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);
        size = PrepareSnapshotSize(source, size) ?? defaultGifOutputSize;
        streamIndex ??= source.PrimaryVideoStream?.Index
                        ?? source.VideoStreams.FirstOrDefault()?.Index
                        ?? 0;

        return (FFMpegArguments
                .FromFileInput(input, false, options => options
                    .Seek(captureTime)
                    .WithDuration(duration)),
            options => options
                .WithGifPaletteArgument((int)streamIndex, size, fps));
    }

    private static Size? PrepareSnapshotSize(IMediaAnalysis source, Size? wantedSize)
    {
        if (wantedSize == null || (wantedSize.Value.Height <= 0 && wantedSize.Value.Width <= 0) || source.PrimaryVideoStream == null)
        {
            return null;
        }

        var currentSize = new Size(source.PrimaryVideoStream.Width, source.PrimaryVideoStream.Height);
        if (IsRotated(source.PrimaryVideoStream.Rotation))
        {
            currentSize = new Size(source.PrimaryVideoStream.Height, source.PrimaryVideoStream.Width);
        }

        if (wantedSize.Value.Width != currentSize.Width || wantedSize.Value.Height != currentSize.Height)
        {
            if (wantedSize.Value.Width <= 0 && wantedSize.Value.Height > 0)
            {
                var ratio = (double)wantedSize.Value.Height / currentSize.Height;
                return new Size((int)(currentSize.Width * ratio), (int)(currentSize.Height * ratio));
            }

            if (wantedSize.Value.Height <= 0 && wantedSize.Value.Width > 0)
            {
                var ratio = (double)wantedSize.Value.Width / currentSize.Width;
                return new Size((int)(currentSize.Width * ratio), (int)(currentSize.Height * ratio));
            }

            return wantedSize;
        }

        return null;
    }

    private static bool IsRotated(int rotation)
    {
        var absRotation = Math.Abs(rotation);
        return absRotation == 90 || absRotation == 180;
    }
}
