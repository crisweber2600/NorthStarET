namespace NorthStarET.Foundation.Infrastructure.Attributes;

/// <summary>
/// Attribute to bypass tenant filtering in queries.
/// Usage of this attribute automatically triggers audit logging.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IgnoreTenantFilterAttribute : Attribute
{
}
