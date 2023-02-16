using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace Daimler.Providence.Database
{
    [ExcludeFromCodeCoverage]
    public partial class MonitoringDBFactory : IDbContextFactory<MonitoringDB>
    {
        private IConfiguration _configuration;

        public MonitoringDBFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MonitoringDB CreateDbContext()
        {
            //var db = new MonitoringDB();
            var db = new MonitoringDB(_configuration);
            db.Database.SetCommandTimeout(6000); // 1h default timeout
            return db;
        }
    }
}