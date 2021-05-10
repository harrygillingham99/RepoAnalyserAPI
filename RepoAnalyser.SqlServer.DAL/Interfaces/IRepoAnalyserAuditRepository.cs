using System.Threading.Tasks;
using RepoAnalyser.Objects.Misc;

namespace RepoAnalyser.SqlServer.DAL.Interfaces
{
    public interface IRepoAnalyserAuditRepository
    {
        Task InsertRequestAudit(RequestAudit audit);
    }
}