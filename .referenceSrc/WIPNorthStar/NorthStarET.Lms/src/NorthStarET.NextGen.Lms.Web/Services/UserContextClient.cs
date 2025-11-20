using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Web.Services;

public sealed class UserContextClient
{
    private static readonly Uri DefaultRelativeUri = new("auth/current-user", UriKind.Relative);
    private readonly HttpClient httpClient;

    public UserContextClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<UserContextDto?> GetCurrentUserAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, DefaultRelativeUri);
        request.Headers.Add(AuthenticationHeaders.SessionId, sessionId.ToString());

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<UserContextDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
