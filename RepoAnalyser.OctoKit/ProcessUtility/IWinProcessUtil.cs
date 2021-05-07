using System.Diagnostics;

namespace RepoAnalyser.Services.ProcessUtility
{
    public interface IWinProcessUtil
    {
        void KillAll();
        Process StartNew(ProcessStartInfo startInfo);
        (string output, string error, int exitCode) StartNewReadOutputAndError(ProcessStartInfo startInfo);
        Process StartNew(Process process);
    }
}