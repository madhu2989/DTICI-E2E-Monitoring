using Daimler.Providence.Service.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Daimler.Providence.Service.Utilities
{
    /// <summary>
    /// Utility class for accessing context of the current thread
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ThreadContext
    {
        /// <summary>
        /// Method for retrieving the name of the principal whose context the current thread runs in, i.e. the current logged in user.
        /// </summary>
        public static string GetCurrentUserName()
        {
            var httpContext  = new HttpContextAccessor().HttpContext;
            string username = "Unknown"; 
            if (httpContext?.User?.Identity?.Name != null)
            {
                username = httpContext.User.Identity.Name;
            }
            return username;
        }

        /// <summary>
        /// Method for retrieving the the roles of the  logged in user.
        /// </summary>
        public static string[] GetCurrentUserRoles()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            ClaimsPrincipal user = null;
            if (httpContext != null && httpContext.User != null)
            {
                user = httpContext.User;
            }            
            if (user == null)
                return Array.Empty<string>();
            else
            {
                var roles = new List<string>();
                if (user.IsInRole(Roles.AdministratorRole))
                {
                    roles.Add(Roles.AdministratorRole);
                }
                if (user.IsInRole(Roles.ContributorRole))
                {
                    roles.Add(Roles.ContributorRole);
                }
                return roles.ToArray();
            }
        }
    }
}