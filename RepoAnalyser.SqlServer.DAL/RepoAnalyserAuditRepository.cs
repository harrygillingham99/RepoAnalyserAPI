using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
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

        public Task InsertRequestAudit(ClientMetadata requester, long executionTime, string requestedEndpoint)
        {
            return Invoke(connection => connection.ExecuteAsync(Sql.InsertAuditItemSql,
                 new
                 {
                     requester.BrowserEngine,
                     requester.BrowserLanguage,
                     requester.BrowserName,
                     requester.CookiesEnabled,
                     requester.Page,
                     requester.Referrer,
                     RequestTime = executionTime,
                     EndpointRequested = requestedEndpoint
                 }));
        }
    }
}
