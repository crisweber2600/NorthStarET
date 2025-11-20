using Microsoft.AspNetCore.Authentication;

namespace NorthStarET.NextGen.Lms.Api.Authentication;

public static class SessionAuthenticationExtensions
{
    public static AuthenticationBuilder AddLmsSession(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
            SessionAuthenticationDefaults.AuthenticationScheme,
            options => { });
    }
}
