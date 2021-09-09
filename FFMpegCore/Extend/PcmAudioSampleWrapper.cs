using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;

namespace FFMpegCore.Extend
{
    public class PcmAudioSampleWrapper : IAudioSample
    {
        //This could actually be short or int, but copies would be inefficient.
        //Handling bytes lets the user decide on the conversion, and abstract the library
        //from handling shorts, unsigned shorts, integers, unsigned integers and floats.
        private readonly byte[] _sample;

        public PcmAudioSampleWrapper(byte[] sample)
        {
            _sample = sample;
        }

        public void Serialize(Stream stream)
        {
            stream.Write(_sample, 0, _sample.Length);
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token)
        {
            await stream.WriteAsync(_sample, 0, _sample.Length, token);
        }
    }
}
