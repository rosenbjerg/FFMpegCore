using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using System;
using System.Drawing;
using System.IO;

namespace FFMpegCore
{
    public class ImageInfo
    {
        private FileInfo _file;

        /// <summary>
        ///  Create a image information object from a target path.
        /// </summary>
        /// <param name="fileInfo">Image file information.</param>
        public ImageInfo(FileInfo fileInfo)
        {
            if (!fileInfo.Extension.ToLower().EndsWith(FileExtension.Png))
            {
                throw new Exception("Image joining currently suppors only .png file types");
            }

            fileInfo.Refresh();

            Size = fileInfo.Length / (1024 * 1024);

            using (var image = Image.FromFile(fileInfo.FullName))
            {
                Width = image.Width;
                Height = image.Height;
                var cd = FFProbeHelper.Gcd(Width, Height);
                Ratio = $"{Width / cd}:{Height / cd}";
            }


            if (!fileInfo.Exists)
                throw new ArgumentException($"Input file {fileInfo.FullName} does not exist!");

            _file = fileInfo;


        }

        /// <summary>
        /// Create a image information object from a target path.
        /// </summary>
        /// <param name="path">Path to image.</param>
        public ImageInfo(string path) : this(new FileInfo(path)) { }

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public string Ratio { get; internal set; }

        /// <summary>
        /// Height of the image file.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Width of the image file.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Image file size in MegaBytes (MB).
        /// </summary>
        public double Size { get; internal set; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name => _file.Name;

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string FullName => _file.FullName;

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension => _file.Extension;

        /// <summary>
        /// Gets a flag indicating if the file is read-only.
        /// </summary>
        public bool IsReadOnly => _file.IsReadOnly;

        /// <summary>
        /// Gets a flag indicating if the file exists (no cache, per call verification).
        /// </summary>
        public bool Exists => File.Exists(FullName);

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationTime => _file.CreationTime;

        /// <summary>
        /// Gets the parent directory information.
        /// </summary>
        public DirectoryInfo Directory => _file.Directory;

        /// <summary>
        /// Create a image information object from a file information object.
        /// </summary>
        /// <param name="fileInfo">Image file information.</param>
        /// <returns></returns>
        public static ImageInfo FromFileInfo(FileInfo fileInfo)
        {
            return FromPath(fileInfo.FullName);
        }

        /// <summary>
        /// Create a image information object from a target path.
        /// </summary>
        /// <param name="path">Path to image.</param>
        /// <returns></returns>
        public static ImageInfo FromPath(string path)
        {
            return new ImageInfo(path);
        }

        /// <summary>
        /// Pretty prints the image information.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Image Path : " + FullName + Environment.NewLine +
                   "Image Root : " + Directory.FullName + Environment.NewLine +
                   "Image Name: " + Name + Environment.NewLine +
                   "Image Extension : " + Extension + Environment.NewLine +
                   "Aspect Ratio : " + Ratio + Environment.NewLine +
                   "Resolution : " + Width + "x" + Height + Environment.NewLine +
                   "Size : " + Size + " MB";
        }

        /// <summary>
        /// Open a file stream.
        /// </summary>
        /// <param name="mode">Opens a file in a specified mode.</param>
        /// <returns>File stream of the image file.</returns>
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
        /// Delete the file.
        /// </summary>
        public void Delete()
        {
            _file.Delete();
        }

        /// <summary>
        /// Converts image info to file info.
        /// </summary>
        /// <returns>A new FileInfo instance.</returns>
        public FileInfo ToFileInfo()
        {
            return new FileInfo(_file.FullName);
        }
    }
}
