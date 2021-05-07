using System.Diagnostics;

namespace RepoAnalyser.Services.ProcessUtility
{
    public interface IWinProcessUtil
    {
        void KillAll();
        (string error, int exitCode) StartNewReadError(ProcessStartInfo startInfo);
        (string output, string error, int exitCode) StartNewReadOutputAndError(ProcessStartInfo startInfo);
        (string error, int exitCode) StartNewReadError(Process process);
    }
}