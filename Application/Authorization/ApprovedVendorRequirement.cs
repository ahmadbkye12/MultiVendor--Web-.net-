using Microsoft.AspNetCore.Authorization;

namespace Application.Authorization;

/// <summary>
/// Requires an authenticated user whose vendor profile exists and is approved by an admin.
/// </summary>
public sealed class ApprovedVendorRequirement : IAuthorizationRequirement;
