using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Test
{
    [TestClass]
    public class ArgumentBuilderTest : BaseTest
    {
        private readonly string[] _concatFiles = { "1.mp4", "2.mp4", "3.mp4", "4.mp4"};


        [TestMethod]
        public void Builder_BuildString_IO_1()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Scale()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Scale(VideoSize.Hd).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -vf scale=-1:720 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_AudioCodec()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithAudioCodec(AudioCodec.Aac).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:a aac \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_AudioBitrate()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithAudioBitrate(AudioQuality.Normal).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -b:a 128k \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_Quiet()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Quiet().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -hide_banner -loglevel warning \"output.mp4\"", str);
        }


        [TestMethod]
        public void Builder_BuildString_AudioCodec_Fluent()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithAudioCodec(AudioCodec.Aac).WithAudioBitrate(128).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:a aac -b:a 128k \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_BitStream()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithBitStreamFilter(Channel.Audio, Filter.H264_Mp4ToAnnexB).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -bsf:a h264_mp4toannexb \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Concat()
        {
            var str = FFMpegArguments.FromConcatenation(_concatFiles).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Copy_Audio()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").CopyChannel(Channel.Audio).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:a copy \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Copy_Video()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").CopyChannel(Channel.Video).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:v copy \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Copy_Both()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").CopyChannel().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c copy \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_DisableChannel_Audio()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").DisableChannel(Channel.Audio).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -an \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_DisableChannel_Video()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").DisableChannel(Channel.Video).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -vn \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_DisableChannel_Both()
        {
            Assert.ThrowsException<FFMpegException>(() => FFMpegArguments.FromInputFiles(true, "input.mp4").DisableChannel(Channel.Both));
        }
        
        [TestMethod]
        public void Builder_BuildString_AudioSamplingRate_Default()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithAudioSamplingRate().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -ar 48000 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_AudioSamplingRate()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithAudioSamplingRate(44000).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -ar 44000 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_VariableBitrate()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithVariableBitrate(5).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -vbr 5 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_Faststart()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithFastStart().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -movflags faststart \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_Overwrite()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").OverwriteExisting().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -y \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_RemoveMetadata()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithoutMetadata().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -map_metadata -1 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_Transpose()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Transpose(Transposition.CounterClockwise90).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -vf \"transpose=2\" \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_CpuSpeed()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithCpuSpeed(10).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -quality good -cpu-used 10 -deadline realtime \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_ForceFormat()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").ForceFormat(VideoCodec.LibX264).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -f libx264 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_FrameOutputCount()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithFrameOutputCount(50).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -vframes 50 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_FrameRate()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithFramerate(50).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -r 50 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Loop()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Loop(50).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -loop 50 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Seek()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Seek(TimeSpan.FromSeconds(10)).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -ss 00:00:10 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Shortest()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").UsingShortest().OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -shortest \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Size()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").Resize(1920, 1080).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -s 1920x1080 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Speed()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithSpeedPreset(Speed.Fast).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -preset fast \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_DrawtextFilter()
        {
            var str = FFMpegArguments
                .FromInputFiles(true, "input.mp4")
                .DrawText(DrawTextOptions
                    .Create("Stack Overflow", "/path/to/font.ttf")
                    .WithParameter("fontcolor", "white")
                    .WithParameter("fontsize", "24")
                    .WithParameter("box", "1")
                    .WithParameter("boxcolor", "black@0.5")
                    .WithParameter("boxborderw", "5")
                    .WithParameter("x", "(w-text_w)/2")
                    .WithParameter("y", "(h-text_h)/2"))
                .OutputToFile("output.mp4").Arguments;

            Assert.AreEqual("-i \"input.mp4\" -vf drawtext=\"text='Stack Overflow':fontfile=/path/to/font.ttf:fontcolor=white:fontsize=24:box=1:boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=(h-text_h)/2\" \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_DrawtextFilter_Alt()
        {
            var str = FFMpegArguments
                .FromInputFiles(true, "input.mp4")
                .DrawText(DrawTextOptions
                    .Create("Stack Overflow", "/path/to/font.ttf", ("fontcolor", "white"), ("fontsize", "24")))
                .OutputToFile("output.mp4").Arguments;

            Assert.AreEqual("-i \"input.mp4\" -vf drawtext=\"text='Stack Overflow':fontfile=/path/to/font.ttf:fontcolor=white:fontsize=24\" \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_StartNumber()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithStartNumber(50).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -start_number 50 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Threads_1()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").UsingThreads(50).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -threads 50 \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Threads_2()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").UsingMultithreading(true).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual($"-i \"input.mp4\" -threads {Environment.ProcessorCount} \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Codec()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithVideoCodec(VideoCodec.LibX264).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:v libx264 -pix_fmt yuv420p \"output.mp4\"", str);
        }

        [TestMethod]
        public void Builder_BuildString_Codec_Override()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithVideoCodec(VideoCodec.LibX264).OutputToFile("output.mp4", true).Arguments;
            Assert.AreEqual("-i \"input.mp4\" -c:v libx264 -pix_fmt yuv420p \"output.mp4\" -y", str);
        }


        [TestMethod]
        public void Builder_BuildString_Duration()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithDuration(TimeSpan.FromSeconds(20)).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -t 00:00:20 \"output.mp4\"", str);
        }
        
        [TestMethod]
        public void Builder_BuildString_Raw()
        {
            var str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithCustomArgument(null).OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\"  \"output.mp4\"", str);

            str = FFMpegArguments.FromInputFiles(true, "input.mp4").WithCustomArgument("-acodec copy").OutputToFile("output.mp4").Arguments;
            Assert.AreEqual("-i \"input.mp4\" -acodec copy \"output.mp4\"", str);
        }
    }
}