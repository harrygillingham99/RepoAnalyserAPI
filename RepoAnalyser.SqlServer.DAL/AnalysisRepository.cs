using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.SqlServer.DAL.BaseRepository;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using RepoAnalyser.SqlServer.DAL.SQL;

namespace RepoAnalyser.SqlServer.DAL
{
    public class AnalysisRepository : Repository, IAnalysisRepository
    {
        public AnalysisRepository(IOptions<AppSettings> options) : base(options.Value.DatabaseConnectionString)
        {
        }

        public Task UpsertAnalysisResults(AnalysisResults results, IDictionary<string, string> codeOwners = null)
        {
            return Invoke(connection =>
            {
                if (codeOwners != null)
                    connection.ExecuteAsync(Sql.UpsertCodeOwnerAnalysis,
                        new {results.RepoId, Result = JsonConvert.SerializeObject(codeOwners)});

                return connection.ExecuteAsync(Sql.UpsertAnalysisResultsInfo,
                    new {results.RepoId, results.RepoName, results.CodeOwnersLastRunDate});
            });
        }

        public Task<(AnalysisResults, IDictionary<string, string>)> GetAnalysisResult(long repoId)
        {
            return InvokeMultiQuery(async (connection, multiReader) =>
            {
                var analysisResults = multiReader.ReadSingleOrDefaultAsync<AnalysisResults>();
                var codeOwners = multiReader.ReadSingleOrDefaultAsync<string>();

                return (await analysisResults,
                    JsonConvert.DeserializeObject<IDictionary<string, string>>(await codeOwners));
            }, Sql.GetRepoAnalysisRunInfo, new {repoId});
        }
    }
}