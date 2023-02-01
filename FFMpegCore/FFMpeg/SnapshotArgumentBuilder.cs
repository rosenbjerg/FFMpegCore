// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore;

public static class SnapshotArgumentBuilder
{
    public static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildSnapshotArguments(
        string input,
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
                .WithVideoCodec(VideoCodec.Png)
                .WithFrameOutputCount(1)
                .Resize(size));
    }

    private static Size? PrepareSnapshotSize(IMediaAnalysis source, Size? wantedSize)
    {
        if (wantedSize == null || (wantedSize.Value.Height <= 0 && wantedSize.Value.Width <= 0) || source.PrimaryVideoStream == null)
        {
            return null;
        }

        var currentSize = new Size(source.PrimaryVideoStream.Width, source.PrimaryVideoStream.Height);
        if (source.PrimaryVideoStream.Rotation == 90 || source.PrimaryVideoStream.Rotation == 180)
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
}
