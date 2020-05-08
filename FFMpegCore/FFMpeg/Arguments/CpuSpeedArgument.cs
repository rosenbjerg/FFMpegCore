namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class CpuSpeedArgument : IArgument
    {
        public readonly int CpuSpeed;
        public CpuSpeedArgument(int cpuSpeed)
        {
            CpuSpeed = cpuSpeed;
        }
        
        public string Text => $"-quality good -cpu-used {CpuSpeed} -deadline realtime";
    }
}
