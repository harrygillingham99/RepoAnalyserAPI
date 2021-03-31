using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.SqlServer.DAL.BaseRepository;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using RepoAnalyser.SqlServer.DAL.SQL;

namespace RepoAnalyser.SqlServer.DAL
{
    public class UtilitiesRepository : Repository, IUtilitiesRepository
    {
        public UtilitiesRepository(IOptions<AppSettings> options) : base(options.Value.DatabaseConnectionString)
        {
        }

        public Task TruncateRequestAuditReseedId()
        {
            return Invoke(connection => connection.ExecuteAsync(Sql.TruncateRequestAudit));
        }
    }
}
