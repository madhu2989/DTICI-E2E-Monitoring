using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Authorization
{
    [ExcludeFromCodeCoverage]
    public class AdminRoleRequirement:  IAuthorizationRequirement
    {
        public string Role { get; set; }

        public AdminRoleRequirement()
        {
            Role = Roles.AdministratorRole;
        }
    }
}
