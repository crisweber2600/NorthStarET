using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Web.Models;

namespace NorthStarET.NextGen.Lms.Web.Controllers;

[Authorize]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Claims()
    {
        var model = new ClaimsDiagnosticViewModel
        {
            UserName = User.Identity?.Name,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = User.Claims
                .Select(claim => new ClaimSummary
                {
                    Type = claim.Type,
                    Value = claim.Value,
                    ValueType = claim.ValueType,
                    Issuer = claim.Issuer
                })
                .OrderBy(summary => summary.Type)
                .ToList()
        };

        return View(model);
    }
}
