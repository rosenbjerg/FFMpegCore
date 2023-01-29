using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test.Utilities;

// https://matt.kotsenas.com/posts/ignoreif-mstest
/// <summary>
/// An extension to the [TestMethod] attribute. It walks the method and class hierarchy looking
/// for an [IgnoreIf] attribute. If one or more are found, they are each evaluated, if the attribute
/// returns `true`, evaluation is short-circuited, and the test method is skipped.
/// </summary>
public class TestMethodWithIgnoreIfSupportAttribute : TestMethodAttribute
{
    public override TestResult[] Execute(ITestMethod testMethod)
    {
        var ignoreResults = TestMethodUtils.GetIgnoreResults(testMethod);
        return ignoreResults.Any() 
            ? ignoreResults 
            : base.Execute(testMethod);
    }
}
public class DataTestMethodWithIgnoreIfSupportAttribute : DataTestMethodAttribute
{
    public override TestResult[] Execute(ITestMethod testMethod)
    {
        var ignoreResults = TestMethodUtils.GetIgnoreResults(testMethod);
        return ignoreResults.Any() 
            ? ignoreResults 
            : base.Execute(testMethod);
    }
}

internal class TestMethodUtils
{
    internal static TestResult[] GetIgnoreResults(ITestMethod testMethod)
    {
        var ignoreAttributes = FindAttributes(testMethod);

        // Evaluate each attribute, and skip if one returns `true`
        foreach (var ignoreAttribute in ignoreAttributes)
        {
            if (ignoreAttribute.ShouldIgnore(testMethod))
            {
                var message = $"Test not executed. Conditional ignore method '{ignoreAttribute.IgnoreCriteriaMethodName}' evaluated to 'true'.";
                {
                    return new[]
                    {
                        new TestResult { Outcome = UnitTestOutcome.Inconclusive, TestFailureException = new AssertInconclusiveException(message) }
                    };
                }
            }
        }

        return Array.Empty<TestResult>();
    }

    private static IEnumerable<IgnoreIfAttribute> FindAttributes(ITestMethod testMethod)
    {
        // Look for an [IgnoreIf] on the method, including any virtuals this method overrides
        var ignoreAttributes = new List<IgnoreIfAttribute>();
        ignoreAttributes.AddRange(testMethod.GetAttributes<IgnoreIfAttribute>(inherit: true));

        // Walk the class hierarchy looking for an [IgnoreIf] attribute
        var type = testMethod.MethodInfo.DeclaringType;
        while (type != null)
        {
            ignoreAttributes.AddRange(type.GetCustomAttributes<IgnoreIfAttribute>(inherit: true));
            type = type.DeclaringType;
        }
        return ignoreAttributes;
    }
}