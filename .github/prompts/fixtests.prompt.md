```prompt
# Fix Tests - .NET focused test-fixing prompt

Purpose
- Provide a concise, actionable checklist for diagnosing and fixing a single failing .NET test in this repository.

Preconditions
- Work on a feature branch. Do not run the whole solution tests by default.
- Have local environment values for `PostgreSQL:ConnectionString`, `Redis:ConnectionString`, and `EntraId` if running integration tests.

Instructions (one-failing-test flow)
1. Capture the failing test output. Prefer asking the user for the terminal output. If not available, run:
   dotnet test <project.csproj> --filter "FullyQualifiedName~<TestMethodName>" --verbosity minimal

2. Classify the test type and required resources (unit, integration, bdd, ui). Note missing infra like DB/Redis.

3. For unit tests: run the single test, read stack trace, edit the smallest amount of code (or test) to fix. Re-run the test.

4. For integration/bdd tests: start AppHost in background with `dotnet run --project src/NorthStarET.NextGen.Lms.AppHost` and wait for health, or prefer in-memory replacements if feasible. Re-run the single test.

5. For UI tests: run the single Playwright test. Collect console logs and screenshots when failing. Prefer selector stabilization and prerender fixes.

6. Verification: re-run the single test until it passes (exit code 0). Do not run `dotnet test` across the entire solution unless asked.

7. Commit and push following `AGENTS.md` rules. Use an explicit commit message referencing the test name and why the change was safe.

Output format (short)
```
## Fix Summary for <TestName>

### Issue
[one-line failing message]

### Root cause
[one-line root cause]

### Fix
[one-line code/test change summary]

### Verification
✅ Test now passes (local)

### Next steps
[what to run next / any infra notes]
```

Constraints
- Make minimal, safe changes. If an infra dependency can't be satisfied, document the limitation and propose a code-side workaround.
- Do not modify CI/CD settings or secrets.

```
# Fix Tests - Systematic Test Debugging Prompt

## Objective
Systematically identify, diagnose, and fix failing tests one at a time using targeted testing and browser automation tools.

## Prerequisites
- Tests have been run and failures identified
- Aspire app infrastructure is available
- Browser automation tools are accessible

## Process Flow

### 1. Capture First Failing Test
```
Use #get_terminal_output to capture the most recent test run output.
Identify the FIRST failing test by name and error message.
Do NOT run full test suite - we already have the output.
```

**Example Output to Parse:**
```
Failed TestName [30s]
  Error Message: [error details]
  Stack Trace: [trace]
```

### 2. Analyze Test Type and Failure

**Categorize the test:**
- Unit Test (xUnit in `tests/unit/`) - No external dependencies
- Integration Test (Aspire in `tests/aspire/`) - Requires running Aspire app
- BDD Test (Reqnroll in `tests/bdd/`) - Requires running Aspire app  
- UI Test (Playwright in `tests/ui/`) - Requires browser automation

**Common Failure Patterns:**
- **Timeout**: Component not rendering, API not responding, Aspire not starting
- **Assertion Failure**: Expected vs actual mismatch
- **Setup Failure**: Dependencies not initialized, data not seeded
- **Connection Error**: Database/Redis not available, HTTP client can't connect

### 3. Run Targeted Test Only

**For Unit Tests:**
```bash
dotnet test tests/unit/[ProjectName]/[ProjectName].csproj \
  --configuration Debug \
  --verbosity minimal \
  --filter "FullyQualifiedName~[TestMethodName]"
```

**For Integration/BDD/UI Tests:**
```bash
# Start Aspire app in background first
dotnet run --project src/NorthStarET.NextGen.Lms.AppHost/NorthStarET.NextGen.Lms.AppHost.csproj &

# Wait for startup (check dashboard at https://localhost:17182)
sleep 15

# Then run targeted test
dotnet test tests/[type]/[ProjectName]/[ProjectName].csproj \
  --configuration Debug \
  --verbosity minimal \
  --filter "FullyQualifiedName~[TestMethodName]"
```

### 4. Diagnose Using Available Tools

#### For UI/Playwright Tests:
1. **Use #chromedevtools/chrome-devtools-mcp:**
   - `mcp_chromedevtool_list_pages` - Check what pages are open
   - `mcp_chromedevtool_navigate_page` - Navigate to the failing route
   - `mcp_chromedevtool_take_snapshot` - Get page structure with element UIDs
   - `mcp_chromedevtool_take_screenshot` - Visual verification
   - `mcp_chromedevtool_list_console_messages` - Check for JS errors
   - `mcp_chromedevtool_list_network_requests` - Verify API calls

2. **Use #mcp_playwright_browser tools:**
   - `mcp_playwright_browser_navigate` - Go to test URL
   - `mcp_playwright_browser_snapshot` - Get accessibility tree
   - `mcp_playwright_browser_take_screenshot` - Visual verification
   - `mcp_playwright_browser_console_messages` - Check console errors
   - `mcp_playwright_browser_network_requests` - Verify network activity

#### For All Test Types:
3. **Use #microsoft.docs.mcp for documentation:**
   - `microsoft_docs_search` - Search for .NET/Blazor/EF Core solutions
   - `microsoft_code_sample_search` - Find official code examples
   - `microsoft_docs_fetch` - Get complete documentation pages

### 5. Common Fix Patterns

#### Blazor Prerendering Issues
**Symptom:** Playwright can't find elements, timeouts
**Diagnosis:** Check if page uses `prerender: false`
**Fix:** Enable prerendering (default behavior)
```razor
<!-- BEFORE (causes issues) -->
@rendermode @(new InteractiveServerRenderMode(prerender: false))

<!-- AFTER (fixes Playwright tests) -->
@rendermode InteractiveServer
```

#### Email/String Normalization Issues
**Symptom:** Assertion failures on string comparisons
**Diagnosis:** Check if domain normalizes values (ToLowerInvariant, Trim)
**Fix:** Update test expectations to match normalized values

#### EF Core Entity Configuration Issues
**Symptom:** Database migration errors, "requires a primary key"
**Diagnosis:** Abstract/base classes being mapped as entities
**Fix:** Add `modelBuilder.Ignore<BaseClass>()` in OnModelCreating

#### Aspire Startup Timeout Issues
**Symptom:** Tests timeout waiting for resources
**Diagnosis:** Containers (Postgres, Redis) take time to start
**Fix:** Increase timeout values in test fixtures
```csharp
private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120); // was 45
```

#### HTTP Client Connection Issues
**Symptom:** Polly timeout, can't connect to API
**Diagnosis:** Aspire app not fully started or health checks failing
**Fix:** 
1. Check Aspire dashboard for resource health
2. Verify migrations complete successfully
3. Check for Entity configuration errors
4. Increase resilience handler timeouts if needed

### 6. Implement Fix

**Steps:**
1. Make the minimal change to fix the specific test
2. Document WHY the change fixes the issue (add code comments)
3. Verify the fix doesn't break other functionality
4. Run the specific test again to confirm it passes

### 7. Verify Fix

```bash
# Re-run the specific failing test
dotnet test [project-path] --filter "FullyQualifiedName~[TestMethodName]" --verbosity minimal

# Verify it passes (exit code 0)
echo $?
```

### 8. Commit and Push

```bash
git add .
git commit -m "fix: [brief description of what was fixed]

- [Detailed explanation of the problem]
- [Detailed explanation of the solution]
- [Impact/result]

Fixes: [TestName]"

git pull origin [branch-name]
# Resolve any conflicts
git push origin [branch-name]
```

### 9. Move to Next Failure

```
After successful commit:
1. Update documentation (if new pattern discovered)
2. Return to Step 1 with the NEXT failing test
3. Repeat until all tests pass
```

## Best Practices

### DO:
✅ Fix one test at a time
✅ Run only the targeted failing test
✅ Use browser tools to diagnose UI issues
✅ Search Microsoft docs for official solutions
✅ Document the fix pattern for future reference
✅ Commit after each successful fix
✅ Update test summary documentation

### DON'T:
❌ Run the entire test suite repeatedly
❌ Make multiple unrelated changes at once
❌ Skip diagnosis and guess at fixes
❌ Commit without verifying the fix
❌ Ignore patterns that might affect other tests
❌ Leave test code with hardcoded values that should be dynamic

## Output Format

After each fix, provide:

```markdown
## Fix Summary for [TestName]

### Issue
[Brief description of the failing test and error]

### Root Cause
[Explanation of why it was failing]

### Solution
[What was changed and why]

### Verification
✅ Test now passes
✅ No regressions introduced
✅ Changes committed to branch

### Next Steps
[Identify next failing test to fix]
```

## Example Session

```
Step 1: Captured terminal output
Found first failure: DistrictList_DisplaysExistingDistricts
Error: Timeout waiting for element "District Management"

Step 2: Analyzed - Playwright UI test, timeout issue

Step 3: Started Aspire app, ran targeted test:
dotnet test tests/ui/.../Playwright.csproj --filter "FullyQualifiedName~DistrictList_DisplaysExistingDistricts"

Step 4: Used Chrome DevTools to navigate to /districts
Found: Page renders blank initially, no prerendering
Diagnosis: prerender: false prevents initial HTML render

Step 5: Fixed Districts.razor to enable prerendering

Step 6: Re-ran test - PASSED ✅

Step 7: Committed with detailed message

Step 8: Moving to next failure...
```

## Tools Quick Reference

### Terminal Output
- `#get_terminal_output` - Get last test run results

### Browser Automation
- `#chromedevtools/chrome-devtools-mcp` - Chrome DevTools Protocol
- `#mcp_playwright_browser_*` - Playwright browser automation

### Documentation
- `#microsoft.docs.mcp` - Official Microsoft documentation search

## Troubleshooting

### "Can't capture terminal output"
- Terminal may be idle or cleared
- Re-run tests to generate fresh output
- Use `dotnet test ... > testresults.txt` to save output

### "Aspire app won't start"
- Check port conflicts (17182 dashboard)
- Verify Docker/Podman is running for containers
- Check for previous instances still running
- Review AppHost Program.cs for configuration errors

### "Browser tools can't connect"
- Ensure browser instance is running
- Check if debugging port is accessible
- Verify browser automation is enabled
- Try restarting browser instance

### "Tests pass locally but fail in CI"
- Check environment variables
- Verify container images are pulled
- Review timing/timeout differences
- Check for test isolation issues

## Success Criteria

- ✅ All unit tests passing (21/21)
- ✅ All Aspire tests passing
- ✅ All BDD tests passing  
- ✅ All Playwright tests passing
- ✅ Build succeeds with warnings as errors
- ✅ All changes committed and pushed
- ✅ Documentation updated

---

**Remember:** Systematic, one-test-at-a-time fixes with proper diagnosis lead to sustainable solutions!
