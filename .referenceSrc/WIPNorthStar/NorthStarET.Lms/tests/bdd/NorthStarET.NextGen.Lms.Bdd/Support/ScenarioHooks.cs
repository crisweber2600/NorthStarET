using Reqnroll;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

[Binding]
public sealed class ScenarioHooks
{
    private readonly ScenarioContext _scenarioContext;

    public ScenarioHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void InitializeScenario()
    {
        var districtContext = new DistrictScenarioContext();
        _scenarioContext.Set(districtContext);

        var schoolContext = new SchoolCatalogScenarioContext();
        _scenarioContext.Set(schoolContext);
    }

    [AfterScenario]
    public void CleanupScenario()
    {
        var key = typeof(DistrictScenarioContext).FullName!;

        if (_scenarioContext.ContainsKey(key))
        {
            var context = _scenarioContext.Get<DistrictScenarioContext>();
            context.Repository.Clear();
            context.AuditSink.Clear();
            context.Dispose();
        }

        var schoolKey = typeof(SchoolCatalogScenarioContext).FullName!;

        if (_scenarioContext.ContainsKey(schoolKey))
        {
            var context = _scenarioContext.Get<SchoolCatalogScenarioContext>();
            context.Repository.Clear();
            context.Dispose();
        }
    }
}
