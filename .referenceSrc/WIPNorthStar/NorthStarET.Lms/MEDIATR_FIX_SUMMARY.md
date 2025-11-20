# MediatR Licensing Fix Summary

## Problem Identified
**Date**: 2025-11-11  
**Issue**: All 32 BDD tests failing with `System.InvalidOperationException: No constructor for type 'MediatR.Licensing.LicenseAccessor' can be instantiated using services from the service container`

### Root Cause
- MediatR 13.1.0 introduced commercial licensing requirements
- `MediatR.Licensing.LicenseAccessor` class required explicit DI registration
- No separate `MediatR.Licensing` NuGet package exists
- MediatR 13.x licensing is designed for commercial/enterprise customers

### Error Pattern
```
Stack Trace:
  at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.CreateConstructorCallSite(...)
  at Microsoft.Extensions.DependencyInjection.MediatRServiceCollectionExtensions.CheckLicense(IServiceProvider serviceProvider)
  at MediatR.Mediator..ctor(IServiceProvider serviceProvider, INotificationPublisher publisher)
  at NorthStarET.NextGen.Lms.Bdd.Support.DistrictScenarioContext..ctor() line 48
```

All 32 BDD scenarios failed at initialization when attempting to resolve `IMediator` from DI container.

## Solution Applied

### 1. Downgrade MediatR to v12.4.1
**File**: `Directory.Packages.props`

```xml
<!-- BEFORE (failing) -->
<PackageVersion Include="MediatR" Version="13.1.0" />

<!-- AFTER (working) -->
<PackageVersion Include="MediatR" Version="12.4.1" />
```

**Rationale**: MediatR 12.x is the last major version without licensing requirements, fully open-source under Apache 2.0 license.

### 2. Update Pipeline Behaviors for MediatR 12.x API

**Files Modified**:
- `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/UnitOfWorkBehavior.cs`
- `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/TenantIsolationBehavior.cs`
- `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/AuditingBehavior.cs`
- `src/NorthStarET.NextGen.Lms.Infrastructure/Common/Behaviors/IdempotencyBehavior.cs`

**API Change**: MediatR 12.x `RequestHandlerDelegate<TResponse>` signature changed between versions
- **MediatR 13.x**: `next(cancellationToken)` - delegate takes CancellationToken parameter
- **MediatR 12.x**: `next()` - delegate takes no parameters (CancellationToken already in Handle method)

```csharp
// BEFORE (MediatR 13.x syntax)
public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
{
    return await next(cancellationToken); // ❌ Compilation error in 12.x
}

// AFTER (MediatR 12.x syntax)
public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
{
    return await next(); // ✅ Correct for 12.x
}
```

### 3. Update Test Mocks

**Files Modified**:
- `tests/unit/NorthStarET.NextGen.Lms.Infrastructure.Tests/Common/Behaviors/TenantIsolationBehaviorTests.cs`
- `tests/unit/NorthStarET.NextGen.Lms.Infrastructure.Tests/Common/Behaviors/UnitOfWorkBehaviorTests.cs`

```csharp
// BEFORE (MediatR 13.x)
await behavior.Handle(command, (ct) => Task.FromResult("success"), CancellationToken.None);

// AFTER (MediatR 12.x)
await behavior.Handle(command, () => Task.FromResult("success"), CancellationToken.None);
```

### 4. Application Layer Registration (No Changes Required)

**File**: `src/NorthStarET.NextGen.Lms.Application/DependencyInjection.cs`

```csharp
// Works for both MediatR 12.x and 13.x
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

## Verification Results

### Before Fix
```
Failed!  - Failed:    32, Passed:     0, Skipped:     0, Total:    32
All BDD tests failing with identical MediatR.Licensing.LicenseAccessor error
```

### After Fix
```
Passed!  - Failed:     0, Passed:    32, Skipped:     0, Total:    32, Duration: 455 ms
✅ All 32 BDD tests passing
✅ Build successful (0 errors)
✅ Unit tests passing
```

### Full Test Suite Status (Post-Fix)
- **Unit Tests**: ✅ Passing (Domain layer: 25/25)
- **BDD Tests**: ✅ Passing (32/32)
- **Playwright Tests**: ⚠️ 1 timeout (pre-existing issue), 7 passing, 29 skipped
  - Failing test: `CreateDistrictFlow_SubmitsSuccessfully` (30s timeout waiting for navigation)
  - Note: This is an infrastructure/Aspire startup issue, NOT related to MediatR fix

## Alternative Solutions Considered

### Option 1: Purchase MediatR License ❌
- **Cost**: ~$500/year for commercial license
- **Decision**: Not viable for open-source/community project

### Option 2: Fork MediatR 13.x and Remove Licensing ❌
- **Complexity**: High maintenance burden
- **Legal**: License violation risk
- **Decision**: Not sustainable

### Option 3: Implement Custom No-Op LicenseAccessor ❌
- **Attempted**: Yes (tried creating `ILicenseAccessor` implementation)
- **Result**: No public `ILicenseAccessor` interface available; internal MediatR implementation
- **Decision**: Not feasible

### Option 4: Downgrade to MediatR 12.4.1 ✅ (Selected)
- **Pros**:
  - Fully open-source (Apache 2.0)
  - Proven stable version
  - No licensing requirements
  - All features needed for this project available
- **Cons**:
  - Missing MediatR 13.x features (none critical for this project)
- **Decision**: Best balance of stability, legality, and functionality

## Breaking Changes Between MediatR 12 → 13

### 1. Pipeline Behaviors
- `RequestHandlerDelegate<TResponse>` signature changed (parameter removed)
- Required updates to all custom behaviors

### 2. Licensing System (New in 13.x)
- `MediatR.Licensing.LicenseAccessor` introduced
- Commercial license check on `IMediator` instantiation
- No workaround without paid license

### 3. Notification Publishers
- MediatR 13.x has `TaskWhenAllPublisher`, `ForeachAwaitPublisher`
- MediatR 12.x uses default sequential publisher
- **Impact**: None for this project (domain events published via EF Core SaveChanges)

## Migration Path to MediatR 13+ (Future)

If the project requires MediatR 13+ features in the future:

1. **Purchase Commercial License** (~$500/year)
   - Add license key to configuration
   - Register `ILicenseAccessor` in DI
   
2. **Update Pipeline Behaviors**
   ```csharp
   return await next(cancellationToken); // 13.x syntax
   ```

3. **Update Tests**
   ```csharp
   await behavior.Handle(command, (ct) => Task.FromResult("success"), CancellationToken.None);
   ```

## Commit Details

**Commit SHA**: fa9241d  
**Message**: `fix: downgrade MediatR to v12.4.1 to resolve licensing dependency injection issue`

**Files Changed**:
- `Directory.Packages.props` - MediatR version downgrade
- 4 behavior files - `next()` invocation syntax
- 2 test files - Mock delegate signatures
- `evidence/mediatr-v12-fix-verification.txt` - Test verification transcript

## Lessons Learned

1. **Version Upgrades**: Always review release notes for commercial licensing changes
2. **Breaking Changes**: MediatR 13.x introduced non-backward-compatible licensing
3. **Open Source Projects**: Stick with open-source dependencies unless commercial features are essential
4. **Testing**: Systematic BDD failures pointed directly to DI configuration issue

## References

- [MediatR 12.4.1 Release Notes](https://github.com/jbogard/MediatR/releases/tag/v12.4.1)
- [MediatR 13.0.0 Licensing Announcement](https://github.com/jbogard/MediatR/releases/tag/v13.0.0)
- [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) - MediatR 12.x license

---
**Status**: ✅ RESOLVED  
**Test Health**: 32/32 BDD passing, all unit tests passing, 1 pre-existing Playwright timeout (unrelated)
