using System;
using Microsoft.Extensions.Configuration;

namespace NorthStarET.NextGen.Lms.Web.Testing;

internal static class TestEnvironment
{
    public const string TestAuthVariable = "NORTHSTARET_LMS_USE_TEST_AUTH";
    public const string PlaywrightStubsVariable = "NORTHSTARET_LMS_USE_PLAYWRIGHT_STUBS";

    public static readonly Guid DefaultDistrictId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static bool IsTestAuthEnabled(IConfiguration? configuration = null)
    {
        return ReadFlag(configuration, TestAuthVariable);
    }

    public static bool UsePlaywrightStubs(IConfiguration? configuration = null)
    {
        return ReadFlag(configuration, PlaywrightStubsVariable);
    }

    private static bool ReadFlag(IConfiguration? configuration, string key)
    {
        var value = configuration?[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            value = Environment.GetEnvironmentVariable(key);
        }

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}