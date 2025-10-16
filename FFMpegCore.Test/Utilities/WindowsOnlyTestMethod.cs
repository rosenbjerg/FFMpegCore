using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test.Utilities;

public class WindowsOnlyTestMethod : TestMethodAttribute
{
    public WindowsOnlyTestMethod([CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1) 
        : base(callerFilePath, callerLineNumber)
    {
    }

    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var message = $"Test not executed on other platforms than Windows";
            {
                return
                [
                    new TestResult { Outcome = UnitTestOutcome.Inconclusive, TestFailureException = new AssertInconclusiveException(message) }
                ];
            }
        }

        return await base.ExecuteAsync(testMethod);
    }
}
