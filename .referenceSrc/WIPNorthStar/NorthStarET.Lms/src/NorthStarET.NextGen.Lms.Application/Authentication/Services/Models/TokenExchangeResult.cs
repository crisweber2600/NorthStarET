using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

public sealed record TokenExchangeResult(
    Guid SessionId,
    DateTimeOffset ExpiresAt,
    string LmsAccessToken,
    UserContextModel User,
    IReadOnlyCollection<TenantSummaryModel> AvailableTenants);
