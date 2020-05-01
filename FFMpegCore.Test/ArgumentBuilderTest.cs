using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FFMpegCore.Test
{
    [TestClass]
    public class ArgumentBuilderTest : BaseTest
    {
        List<string> concatFiles = new List<string>
        { "1.mp4", "2.mp4", "3.mp4", "4.mp4"};

        FFArgumentBuilder builder;

        public ArgumentBuilderTest() : base()
        {
            builder = new FFArgumentBuilder();
        }

        private string GetArgumentsString(params Argument[] args)
        {
            var container = new ArgumentContainer {new InputArgument("input.mp4")};
            foreach (var a in args)
            {
                container.Add(a);
            }
            container.Add(new OutputArgument("output.mp4"));

            return builder.BuildArguments(container);
        }


        [TestMethod]
        public void Builder_BuildString_IO_1()
        {
            var str = GetArgumentsString();

            Assert.AreEqual(str, "-i \"input.mp4\" \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Scale()
        {
            var str = GetArgumentsString(new ScaleArgument(VideoSize.Hd));

            Assert.AreEqual(str, "-i \"input.mp4\" -vf scale=-1:720 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_AudioCodec()
        {
            var str = GetArgumentsString(new AudioCodecArgument(AudioCodec.Aac));
            Assert.AreEqual(str, "-i \"input.mp4\" -c:a aac \"output.mp4\"");
        }
        
        [TestMethod]
        public void Builder_BuildString_AudioBitrate()
        {
            var str = GetArgumentsString(new AudioBitrateArgument(AudioQuality.Normal));
            Assert.AreEqual(str, "-i \"input.mp4\" -b:a 128k \"output.mp4\"");
        }
        
        [TestMethod]
        public void Builder_BuildString_Quiet()
        {
            var str = GetArgumentsString(new QuietArgument());
            Assert.AreEqual(str, "-i \"input.mp4\" -hide_banner -loglevel warning \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_BitStream()
        {
            var str = GetArgumentsString(new BitStreamFilterArgument(Channel.Audio, Filter.H264_Mp4ToAnnexB));

            Assert.AreEqual(str, "-i \"input.mp4\" -bsf:a h264_mp4toannexb \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Concat()
        {
            var container = new ArgumentContainer {new ConcatArgument(concatFiles), new OutputArgument("output.mp4")};


            var str = builder.BuildArguments(container);

            Assert.AreEqual(str, "-i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Copy_Audio()
        {
            var str = GetArgumentsString(new CopyArgument(Channel.Audio));

            Assert.AreEqual(str, "-i \"input.mp4\" -c:a copy \"output.mp4\"");
        }


        [TestMethod]
        public void Builder_BuildString_Copy_Video()
        {
            var str = GetArgumentsString(new CopyArgument(Channel.Video));

            Assert.AreEqual(str, "-i \"input.mp4\" -c:v copy \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Copy_Both()
        {
            var str = GetArgumentsString(new CopyArgument(Channel.Both));

            Assert.AreEqual(str, "-i \"input.mp4\" -c copy \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_CpuSpeed()
        {
            var str = GetArgumentsString(new CpuSpeedArgument(10));

            Assert.AreEqual(str, "-i \"input.mp4\" -quality good -cpu-used 10 -deadline realtime \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_ForceFormat()
        {
            var str = GetArgumentsString(new ForceFormatArgument(VideoCodec.LibX264));

            Assert.AreEqual(str, "-i \"input.mp4\" -f libx264 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_FrameOutputCount()
        {
            var str = GetArgumentsString(new FrameOutputCountArgument(50));

            Assert.AreEqual(str, "-i \"input.mp4\" -vframes 50 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_FrameRate()
        {
            var str = GetArgumentsString(new FrameRateArgument(50));

            Assert.AreEqual(str, "-i \"input.mp4\" -r 50 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Loop()
        {
            var str = GetArgumentsString(new LoopArgument(50));

            Assert.AreEqual(str, "-i \"input.mp4\" -loop 50 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Seek()
        {
            var str = GetArgumentsString(new SeekArgument(TimeSpan.FromSeconds(10)));

            Assert.AreEqual(str, "-i \"input.mp4\" -ss 00:00:10 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Shortest()
        {
            var str = GetArgumentsString(new ShortestArgument(true));

            Assert.AreEqual(str, "-i \"input.mp4\" -shortest \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Size()
        {
            var str = GetArgumentsString(new SizeArgument(1920, 1080));

            Assert.AreEqual(str, "-i \"input.mp4\" -s 1920x1080 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Speed()
        {
            var str = GetArgumentsString(new SpeedArgument(Speed.Fast));

            Assert.AreEqual(str, "-i \"input.mp4\" -preset fast \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_DrawtextFilter()
        {
            var str = GetArgumentsString(new DrawTextArgument("Stack Overflow", "/path/to/font.ttf", 
                ("fontcolor", "white"),
                ("fontsize", "24"),
                ("box", "1"),
                ("boxcolor", "black@0.5"),
                ("boxborderw", "5"),
                ("x", "(w-text_w)/2"),
                ("y", "(h-text_h)/2")));

            Assert.AreEqual("-i \"input.mp4\" -vf drawtext=\"text='Stack Overflow': fontfile=/path/to/font.ttf: fontcolor=white: fontsize=24: box=1: boxcolor=black@0.5: boxborderw=5: x=(w-text_w)/2: y=(h-text_h)/2\" \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_StartNumber()
        {
            var str = GetArgumentsString(new StartNumberArgument(50));

            Assert.AreEqual(str, "-i \"input.mp4\" -start_number 50 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Threads_1()
        {
            var str = GetArgumentsString(new ThreadsArgument(50));
            
            Assert.AreEqual(str, "-i \"input.mp4\" -threads 50 \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Threads_2()
        {
            var str = GetArgumentsString(new ThreadsArgument(true));

            Assert.AreEqual(str, $"-i \"input.mp4\" -threads {Environment.ProcessorCount} \"output.mp4\"");
        }


        [TestMethod]
        public void Builder_BuildString_Codec()
        {
            var str = GetArgumentsString(new VideoCodecArgument(VideoCodec.LibX264));

            Assert.AreEqual(str, "-i \"input.mp4\" -c:v libx264 -pix_fmt yuv420p \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Codec_Override()
        {
            var str = GetArgumentsString(new VideoCodecArgument(VideoCodec.LibX264), new OverrideArgument());

            Assert.AreEqual(str, "-i \"input.mp4\" -c:v libx264 -pix_fmt yuv420p -y \"output.mp4\"");
        }

        [TestMethod]
        public void Builder_BuildString_Duration() {
            var str = GetArgumentsString(new DurationArgument(TimeSpan.FromSeconds(20)));

            Assert.AreEqual(str, "-i \"input.mp4\" -t 00:00:20 \"output.mp4\"");
        }
    }
}
