namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeSource"/> for a raw audio stream that is gathered from <see cref="IEnumerator{IAudioFrame}"/>.
    /// It is the user's responbility to make sure the enumerated samples match the configuration provided to this pipe.
    /// </summary>
    public class RawAudioPipeSource : IPipeSource
    {
        private readonly IEnumerator<IAudioSample> _sampleEnumerator;

        public string Format { get; set; } = "s16le";
        public uint SampleRate { get; set; } = 8000;
        public uint Channels { get; set; } = 1;

        public RawAudioPipeSource(IEnumerator<IAudioSample> sampleEnumerator)
        {
            _sampleEnumerator = sampleEnumerator;
        }

        public RawAudioPipeSource(IEnumerable<IAudioSample> sampleEnumerator)
            : this(sampleEnumerator.GetEnumerator()) { }

        public string GetStreamArguments()
        {
            return $"-f {Format} -ar {SampleRate} -ac {Channels}";
        }

        public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            if (_sampleEnumerator.Current != null)
            {
                await _sampleEnumerator.Current.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }

            while (_sampleEnumerator.MoveNext())
            {
                await _sampleEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
