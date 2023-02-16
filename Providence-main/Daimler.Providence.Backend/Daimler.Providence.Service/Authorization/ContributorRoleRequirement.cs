using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Authorization
{
    [ExcludeFromCodeCoverage]
    public class ContributorRoleRequirement:  IAuthorizationRequirement
    {
        public string Role { get; set; }
        
        public ContributorRoleRequirement()
        {
            Role = Roles.ContributorRole;
        }
    }
}
