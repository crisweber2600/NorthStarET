using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace NorthStarET.NextGen.Lms.Identity.IntegrationTests.Authentication;

public class AuthenticationFlowTests : IClassFixture<AspireIdentityFixture>
{
    private readonly AspireIdentityFixture _fixture;

    public AuthenticationFlowTests(AspireIdentityFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExchangeToken_WithValidEntraToken_ShouldCreateSession()
    {
        await Task.CompletedTask;
        Assert.True(false, "Integration token exchange flow not implemented.");
    }
}
