using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RepoAnalyser.Objects.Attributes;
using Serilog;

namespace RepoAnalyser.Services.ProcessUtility
{
    [ScrutorIgnore]
    public class WinProcessUtil : IWinProcessUtil
    {
        private readonly List<Process> _processes;

        public WinProcessUtil()
        {
            _processes = new List<Process>();
        }

        public (string output, string error, int exitCode) StartNewReadOutputAndError(ProcessStartInfo startInfo)
        {
            return StartNewReadOutputAndError(new Process {StartInfo = startInfo});
        }

        public (string error, int exitCode) StartNewReadError(Process process)
        {
            var error = new StringBuilder();
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null) error.AppendLine(args.Data);
            };

            process.Start();
            _processes.Add(process);
            process.BeginErrorReadLine();
            process.WaitForExit();

            return (error.ToString(), process.ExitCode);
        }

        public (string error, int exitCode) StartNewReadError(ProcessStartInfo startInfo)
        {
            return StartNewReadError(new Process
            {
                StartInfo = startInfo
            });
        }

        //in a 'prod' environment this will be called before a graceful shutdown
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

        public (string output, string error, int exitCode) StartNewReadOutputAndError(Process process)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null) error.AppendLine(e.Data);
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) output.AppendLine(e.Data);
            };
            process.Start();
            _processes.Add(process);
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return (output.ToString(), error.ToString(), process.ExitCode);
        }
    }
}