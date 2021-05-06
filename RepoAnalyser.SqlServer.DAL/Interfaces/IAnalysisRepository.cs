using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.SqlServer.DAL.Interfaces
{
    public interface IAnalysisRepository
    {
        Task<AnalysisResult> GetAnalysisResult(long repoId);
        Task UpsertAnalysisResults(AnalysisResults results, IDictionary<string, string> codeOwners = null,
            IDictionary<string, int> cyclomaticComplexities = null);
    }
}