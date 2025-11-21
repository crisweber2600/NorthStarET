namespace NorthStarET.Foundation.Infrastructure.Attributes;

/// <summary>
/// DEPRECATED: Use ITenantContext.BypassFilter() instead.
/// This attribute is kept for backward compatibility but will be removed in a future version.
/// 
/// Example usage with ITenantContext:
/// <code>
/// using (tenantContext.BypassFilter("Cross-tenant report generation"))
/// {
///     var allStudents = await _context.Students.ToListAsync();
/// }
/// </code>
/// </summary>
[Obsolete("Use ITenantContext.BypassFilter() instead. This attribute will be removed in a future version.")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IgnoreTenantFilterAttribute : Attribute
{
}
