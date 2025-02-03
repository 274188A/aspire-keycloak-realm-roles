using System.Security.Claims;
using AspireKeycloak.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;

namespace AspireKeycloak.ApiService;

public class RoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        var realmAccessClaim = identity?.FindFirst("realm_access");
        identity?.AddRealmRoles(realmAccessClaim?.Value);

        return Task.FromResult(principal);
    }
}
