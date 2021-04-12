using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RepoAnalyser.Objects;
using RepoAnalyser.SqlServer.DAL.BaseRepository;
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
            return Invoke(async connection =>
            {
                using var multiReader = await connection.QueryMultipleAsync(Sql.GetRepoAnalysisRunInfo, new {repoId});

                var analysisResults = await multiReader.ReadSingleOrDefaultAsync<AnalysisResults>();
                var codeOwners = await multiReader.ReadSingleOrDefaultAsync<string>();

                return (analysisResults ?? new AnalysisResults(),
                    codeOwners != null
                        ? JsonConvert.DeserializeObject<IDictionary<string, string>>(codeOwners)
                        : new Dictionary<string, string>());
            });
        }
    }

    public class AnalysisResults
    {
        public long RepoId { get; set; }
        public string RepoName { get; set; }
        public DateTime? CodeOwnersLastRunDate { get; set; }
    }

    public interface IAnalysisRepository
    {
        Task<(AnalysisResults, IDictionary<string, string>)> GetAnalysisResult(long repoId);
        Task UpsertAnalysisResults(AnalysisResults results, IDictionary<string, string> codeOwners = null);
    }
}