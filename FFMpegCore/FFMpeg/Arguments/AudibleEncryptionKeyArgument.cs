namespace FFMpegCore.Arguments
{
    public class AudibleEncryptionKeyArgument : IArgument
    {
        private readonly string _key;
        private readonly string _iv;

        public AudibleEncryptionKeyArgument(string key, string iv)
        {
            _key = key;
            _iv = iv;
        }

        public string Text => $"-audible_key {_key} -audible_iv {_iv}";
    }
}
