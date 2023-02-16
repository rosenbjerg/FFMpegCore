using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test.Utilities;

public class WindowsOnlyDataTestMethod : DataTestMethodAttribute
{
    public override TestResult[] Execute(ITestMethod testMethod)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var message = $"Test not executed on other platforms than Windows";
            {
                return new[]
                {
                    new TestResult { Outcome = UnitTestOutcome.Inconclusive, TestFailureException = new AssertInconclusiveException(message) }
                };
            }
        }

        return base.Execute(testMethod);
    }
}
