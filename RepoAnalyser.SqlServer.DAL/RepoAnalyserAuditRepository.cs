using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects.Config;
using RepoAnalyser.Objects.Misc;
using RepoAnalyser.SqlServer.DAL.BaseRepository;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using RepoAnalyser.SqlServer.DAL.SQL;

namespace RepoAnalyser.SqlServer.DAL
{
    public class RepoAnalyserAuditRepository : Repository, IRepoAnalyserAuditRepository
    {
        public RepoAnalyserAuditRepository(IOptions<AppSettings> options) : base(options.Value.DatabaseConnectionString)
        {

        }

        public Task InsertRequestAudit(RequestAudit audit)
        {
            return Invoke(connection => 
                connection.ExecuteAsync(Sql.InsertAuditItemSql,
                 new
                 {
                     audit.Metadata!.BrowserEngine,
                     audit.Metadata!.BrowserLanguage,
                     audit.Metadata!.BrowserName,
                     audit.Metadata!.CookiesEnabled,
                     audit.Metadata!.Page,
                     audit.Metadata!.Referrer,
                     RequestTime = audit.ExecutionTime,
                     EndpointRequested = audit.RequestedEndpoint
                 }));
        }
    }
}
