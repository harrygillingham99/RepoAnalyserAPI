using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RepoAnalyser.Objects.Attributes;
using Log = Serilog.Log;

namespace RepoAnalyser.Logic.ProcessUtility
{
    [ScrutorIgnore]
    public class WinProcessUtil : IWinProcessUtil
    {
        private readonly List<Process> _processes;

        public WinProcessUtil()
        {
            _processes = new List<Process>();
        }

        public Process StartNew(ProcessStartInfo startInfo)
        { 
            var process = Process.Start(startInfo);
            _processes.Add(process);
            return process;
        }

        public void KillAll()
        {
            _processes.ForEach(process =>
            {
                if (process.HasExited) return;

                try
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Process encountered an error");
                }
            });
        }
    }
}
