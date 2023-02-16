using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Daimler.Providence.Service.Utilities;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;

namespace Daimler.Providence.Service.Authorization
{
    [ExcludeFromCodeCoverage]
    public class ContributorAuthorizationHandler:  AuthorizationHandler<ContributorRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContributorRoleRequirement requirement)
        {
            var isInRole = context.User.IsInRole(requirement.Role);
            if (isInRole)
            {
                context.Succeed(requirement);
            }
            else
            {
                var user = context.User;
                var userName = user.Identity.Name;
                var action = context.Resource.ToString();
                var claims = user.Claims;
                var userRoles = claims?.FirstOrDefault(c => c.Type.Contains("claims/scope"))?.Value;
                userRoles = !string.IsNullOrEmpty(userRoles) ? userRoles : "No (Providence Service) roles assigned to this user.";
                AILogger.Log(SeverityLevel.Information, $"Http statusCode: 403 Forbidden \n User: '{userName}' \n Action: '{action}' \n Roles: '{userRoles}'.", string.Empty, typeof(AdministratorAuthorizationHandler).Name);
                context.Fail();
            }
            return Task.FromResult(0);
        }
    }
}
