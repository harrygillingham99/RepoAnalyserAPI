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

        public Task UpsertAnalysisResults(AnalysisResults results, IDictionary<string, string> codeOwners = null, IDictionary<string, int> cyclomaticComplexities = null, string staticAnalysisReportDir = null)
        {
            return Invoke(connection =>
            {
                if (codeOwners != null)
                    connection.ExecuteAsync(Sql.UpsertCodeOwnerAnalysis,
                        new {results.RepoId, Result = JsonConvert.SerializeObject(codeOwners), LastUpdated = results.CodeOwnersLastRunDate});

                if (cyclomaticComplexities != null)
                    connection.ExecuteAsync(Sql.UpsertCyclomaticComplexityAnalysis,
                        new {results.RepoId, Result = JsonConvert.SerializeObject(cyclomaticComplexities), LastUpdated = results.CyclomaticComplexitiesLastUpdated });

                if (staticAnalysisReportDir != null)
                    connection.ExecuteAsync(Sql.UpsertStaticAnalysis,
                        new
                        {
                            results.RepoId, Result = staticAnalysisReportDir,
                            LastUpdated = results.StaticAnalysisLastUpdated
                        });

                return connection.ExecuteAsync(Sql.UpsertAnalysisResultsInfo,
                    new {results.RepoId, results.RepoName});
            });
        }

        public Task<AnalysisResult> GetAnalysisResult(long repoId)
        {
            return InvokeMultiQuery(async (connection, multiReader) =>
            {
                var analysisResults = multiReader.ReadSingleOrDefaultAsync<AnalysisResults>();
                var codeOwnersJson = await multiReader.ReadSingleOrDefaultAsync<string>();
                var cyclomaticComplexitiesJson = await multiReader.ReadSingleOrDefaultAsync<string>();
                var staticAnalysisReportDir = await multiReader.ReadSingleOrDefaultAsync<string>();

                IDictionary<string, string> GetCodeOwners()
                {
                    return codeOwnersJson != null
                        ? JsonConvert.DeserializeObject<IDictionary<string, string>>(codeOwnersJson)
                        : new Dictionary<string, string>();

                }

                IDictionary<string, int> GetCyclomaticComplexity()
                {
                    return cyclomaticComplexitiesJson != null
                        ? JsonConvert.DeserializeObject<IDictionary<string, int>>(cyclomaticComplexitiesJson)
                        : new Dictionary<string, int>();
                }

                string GetReportDir()
                {
                    return staticAnalysisReportDir;
                }

                return new AnalysisResult
                {
                    Result = await analysisResults,
                    CodeOwners = GetCodeOwners(),
                    Complexities = GetCyclomaticComplexity(),
                    GendarmeReportDirectory = GetReportDir()
                };
            }, Sql.GetRepoAnalysisRunInfo, new {repoId});
        }
    }
}