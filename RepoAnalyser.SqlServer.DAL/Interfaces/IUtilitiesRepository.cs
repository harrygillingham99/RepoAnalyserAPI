using System.Threading.Tasks;

namespace RepoAnalyser.SqlServer.DAL.Interfaces
{
    public interface IUtilitiesRepository
    {
        public Task TruncateRequestAuditReseedId();
    }
}