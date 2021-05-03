using System.Diagnostics;

namespace RepoAnalyser.Services.ProcessUtility
{
    public interface IWinProcessUtil
    {
        void KillAll();
        Process StartNew(ProcessStartInfo startInfo);
    }
}