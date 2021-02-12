using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.SqlServer.DAL.Interfaces
{
    public interface IRepoAnalyserAuditRepository
    {
        Task InsertRequestAudit(ClientMetadata requester, long executionTime, string requestedEndpoint);
    }
}