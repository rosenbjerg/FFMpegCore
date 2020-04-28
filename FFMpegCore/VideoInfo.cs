using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Pipes;
using System;
using System.IO;

namespace FFMpegCore
{
    public class VideoInfo
    {
        private FileInfo _file;

        internal VideoInfo()
        {

        }
        /// <summary>
        /// Create a video information object from a file information object.
        /// </summary>
        /// <param name="fileInfo">Video file information.</param>
        public VideoInfo(FileInfo fileInfo, int outputCapacity = int.MaxValue)
        {
            fileInfo.Refresh();

            if (!fileInfo.Exists)
                throw new ArgumentException($"Input file {fileInfo.FullName} does not exist!");

            _file = fileInfo;

            new FFProbe(outputCapacity).ParseVideoInfo(this);
        }

        /// <summary>
        /// Create a video information object from a target path.
        /// </summary>
        /// <param name="path">Path to video.</param>
        /// <param name="outputCapacity">Max amount of outputlines</param>
        public VideoInfo(string path, int outputCapacity = int.MaxValue) : this(new FileInfo(path), outputCapacity) { }

        /// <summary>
        /// Duration of the video file.
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Audio format of the video file.
        /// </summary>
        public string AudioFormat { get; internal set; }

        /// <summary>
        /// Video format of the video file.
        /// </summary>
        public string VideoFormat { get; internal set; }

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public string Ratio { get; internal set; }

        /// <summary>
        /// Video frame rate.
        /// </summary>
        public double FrameRate { get; internal set; }

        /// <summary>
        /// Height of the video file.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Width of the video file.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Video file size in MegaBytes (MB).
        /// </summary>
        public double Size { get; internal set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name => _file != null ? _file.Name : throw new FileNotFoundException();

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string FullName => _file != null ? _file.FullName : throw new FileNotFoundException();

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension => _file != null ? _file.Extension : throw new FileNotFoundException();

        /// <summary>
        /// Gets a flag indicating if the file is read-only.
        /// </summary>
        public bool IsReadOnly => _file != null ? _file.IsReadOnly : throw new FileNotFoundException();

        /// <summary>
        /// Gets a flag indicating if the file exists (no cache, per call verification).
        /// </summary>
        public bool Exists => _file != null ? File.Exists(FullName) : false;

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationTime => _file != null ? _file.CreationTime : throw new FileNotFoundException();

        /// <summary>
        /// Gets the parent directory information.
        /// </summary>
        public DirectoryInfo Directory => _file != null ? _file.Directory : throw new FileNotFoundException();

        /// <summary>
        /// Create a video information object from a file information object.
        /// </summary>
        /// <param name="fileInfo">Video file information.</param>
        /// <returns></returns>
        public static VideoInfo FromFileInfo(FileInfo fileInfo)
        {
            return FromPath(fileInfo.FullName);
        }

        /// <summary>
        ///  Create a video information object from a target path.
        /// </summary>
        /// <param name="path">Path to video.</param>
        /// <returns></returns>
        public static VideoInfo FromPath(string path)
        {
            return new VideoInfo(path);
        }

        /// <summary>
        ///  Create a video information object from a encoded stream.
        /// </summary>
        /// <param name="stream">Encoded video stream.</param>
        /// <returns></returns>
        public static VideoInfo FromStream(System.IO.Stream stream)
        {
            return new FFProbe().ParseVideoInfo(stream);
        }

        /// <summary>
        ///  Pretty prints the video information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Video Path : " + FullName + Environment.NewLine +
                   "Video Root : " + Directory.FullName + Environment.NewLine +
                   "Video Name: " + Name + Environment.NewLine +
                   "Video Extension : " + Extension + Environment.NewLine +
                   "Video Duration : " + Duration + Environment.NewLine +
                   "Audio Format : " + AudioFormat + Environment.NewLine +
                   "Video Format : " + VideoFormat + Environment.NewLine +
                   "Aspect Ratio : " + Ratio + Environment.NewLine +
                   "Framerate : " + FrameRate + "fps" + Environment.NewLine +
                   "Resolution : " + Width + "x" + Height + Environment.NewLine +
                   "Size : " + Size + " MB";
        }

        /// <summary>
        /// Open a file stream.
        /// </summary>
        /// <param name="mode">Opens a file in a specified mode.</param>
        /// <returns>File stream of the video file.</returns>
        public FileStream FileOpen(FileMode mode)
        {
            return _file.Open(mode);
        }

        /// <summary>
        /// Move file to a specific directory.
        /// </summary>
        /// <param name="destination"></param>
        public void MoveTo(DirectoryInfo destination)
        {
            var newLocation = $"{destination.FullName}{Path.DirectorySeparatorChar}{Name}{Extension}";
            _file.MoveTo(newLocation);
            _file = new FileInfo(newLocation);
        }

        /// <summary>
        ///  Delete the file.
        /// </summary>
        public void Delete()
        {
            _file.Delete();
        }

        /// <summary>
        /// Converts video info to file info.
        /// </summary>
        /// <returns>FileInfo</returns>
        public FileInfo ToFileInfo()
        {
            return new FileInfo(_file.FullName);
        }
    }
}
