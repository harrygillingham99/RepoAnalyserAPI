using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.SqlServer.DAL.SQL;

namespace RepoAnalyser.SqlServer.DAL
{
    public class RepoAnalyserRepository : Repository, IRepoAnalyserRepository
    {
        public RepoAnalyserRepository(IOptions<AppSettings> options) : base(options.Value.DatabaseConnectionString)
        {

        }

        public async Task InsertRequestAudit(ClientMetadata requester, long executionTime, string requestedEndpoint)
        {
            await Invoke(connection => connection.ExecuteAsync(Sql.InsertAuditItemSql,
                new
                {
                    requester.BrowserEngine, requester.BrowserLanguage, requester.BrowserName, requester.CookiesEnabled,
                    requester.Page, requester.Referrer, RequestTime = executionTime,
                    EndpointRequested = requestedEndpoint
                }));
        }
    }

    public interface IRepoAnalyserRepository
    {
        Task InsertRequestAudit(ClientMetadata requester, long executionTime, string requestedEndpoint);
    }
}
