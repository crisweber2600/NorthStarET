using System;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;

public sealed record TenantSummaryModel(Guid TenantId, string Name, string Type);
