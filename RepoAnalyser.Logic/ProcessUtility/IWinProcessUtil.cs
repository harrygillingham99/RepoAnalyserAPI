using System.Diagnostics;

namespace RepoAnalyser.Logic.ProcessUtility
{
    public interface IWinProcessUtil
    {
        void KillAll();
        Process StartNew(ProcessStartInfo startInfo);
    }
}