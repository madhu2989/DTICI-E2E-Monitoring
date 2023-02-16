using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Authorization
{
    [ExcludeFromCodeCoverage]
    public class AdminOrContributorRoleRequirement:  IAuthorizationRequirement
    {        
        public string Role { get; set; }

        public AdminOrContributorRoleRequirement()
        {
            Role = Roles.AdminOrContributorRole;
        }
    }
}
