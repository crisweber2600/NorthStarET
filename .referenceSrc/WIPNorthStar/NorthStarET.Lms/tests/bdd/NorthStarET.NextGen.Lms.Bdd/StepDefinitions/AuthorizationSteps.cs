using System.Threading.Tasks;
using Reqnroll;

namespace NorthStarET.NextGen.Lms.Bdd.StepDefinitions;

[Binding]
public sealed class AuthorizationSteps
{
    [Given("authorization caching is warmed for previously granted actions")]
    public Task GivenAuthorizationCachingIsWarmedAsync()
    {
        throw new PendingStepException();
    }

    [Given("a district admin has an active LMS session for tenant \"(.*)\"")]
    public Task GivenDistrictAdminHasActiveSessionAsync(string tenantName)
    {
        throw new PendingStepException();
    }

    [Given("a teacher has an active LMS session for tenant \"(.*)\"")]
    public Task GivenTeacherHasActiveSessionAsync(string tenantName)
    {
        throw new PendingStepException();
    }

    [When("the user requests to \"(.*)\"")]
    public Task WhenTheUserRequestsProtectedActionAsync(string action)
    {
        throw new PendingStepException();
    }

    [Then("the authorization decision should be allowed")]
    public Task ThenAuthorizationDecisionShouldBeAllowedAsync()
    {
        throw new PendingStepException();
    }

    [Then("the authorization decision should be denied")]
    public Task ThenAuthorizationDecisionShouldBeDeniedAsync()
    {
        throw new PendingStepException();
    }

    [Then("the response time should be below 50 milliseconds")]
    public Task ThenResponseTimeShouldBeBelowThresholdAsync()
    {
        throw new PendingStepException();
    }

    [Then("the authorization audit log records an allowed entry")]
    public Task ThenAuditLogRecordsAllowedEntryAsync()
    {
        throw new PendingStepException();
    }

    [Then("the authorization audit log records a denied entry with reason")]
    public Task ThenAuditLogRecordsDeniedEntryAsync()
    {
        throw new PendingStepException();
    }
}
