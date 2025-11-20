using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Api.Authentication;

public static class SessionAuthenticationDefaults
{
    public const string AuthenticationScheme = "LmsSession";

    public static string SessionHeaderName => AuthenticationHeaders.SessionId;
}
