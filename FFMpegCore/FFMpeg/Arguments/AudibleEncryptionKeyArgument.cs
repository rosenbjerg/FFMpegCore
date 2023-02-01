namespace FFMpegCore.Arguments
{
    public class AudibleEncryptionKeyArgument : IArgument
    {
        private readonly bool _aaxcMode;

        private readonly string? _key;
        private readonly string? _iv;

        private readonly string? _activationBytes;

        public AudibleEncryptionKeyArgument(string activationBytes)
        {
            _activationBytes = activationBytes;
        }

        public AudibleEncryptionKeyArgument(string key, string iv)
        {
            _aaxcMode = true;

            _key = key;
            _iv = iv;
        }

        public string Text => _aaxcMode ? $"-audible_key {_key} -audible_iv {_iv}" : $"-activation_bytes {_activationBytes}";
    }
}
