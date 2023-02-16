using System.Diagnostics.CodeAnalysis;

namespace Daimler.Providence.Service.Authorization
{
    [ExcludeFromCodeCoverage]
    public class Roles
    {
        public const string AdministratorRole = "Monitoring_admin";
        public const string ContributorRole = "Monitoring_contributor";
        public const string AdminOrContributorRole = "Monitoring_admin,Monitoring_contributor";
    }
}
