using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFMpegCore.Extensions.Downloader.Enums;

namespace FFMpegCore.Test.Utilities;

[Flags]
internal enum OsPlatforms : ushort
{
    Windows,
    Linux,
    MacOS
}

internal class OsSpecificTestMethod : TestMethodAttribute
{
    private readonly IEnumerable<OSPlatform> _supportedOsPlatforms;

    public OsSpecificTestMethod(OsPlatforms supportedOsPlatforms, [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1) : base(callerFilePath, callerLineNumber)
    {
        _supportedOsPlatforms = supportedOsPlatforms.GetFlags()
            .Select(flag => OSPlatform.Create(flag.ToString().ToUpperInvariant()))
            .ToArray();
    }

    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        if (_supportedOsPlatforms.Any(RuntimeInformation.IsOSPlatform))
        {
            return await base.ExecuteAsync(testMethod);
        }

        var message = $"Test only executed on specific platforms: {string.Join(", ", _supportedOsPlatforms.Select(platform => platform.ToString()))}";
        {
            return
            [
                new TestResult { Outcome = UnitTestOutcome.Inconclusive, TestFailureException = new AssertInconclusiveException(message) }
            ];
        }
    }
}
