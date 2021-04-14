using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.SqlServer.DAL.Interfaces
{
    public interface IAnalysisRepository
    {
        Task<(AnalysisResults, IDictionary<string, string>)> GetAnalysisResult(long repoId);
        Task UpsertAnalysisResults(AnalysisResults results, IDictionary<string, string> codeOwners = null);
    }
}