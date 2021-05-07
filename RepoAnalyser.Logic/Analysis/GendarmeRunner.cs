using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Services.ProcessUtility;

namespace RepoAnalyser.Logic.Analysis
{
    public class GendarmeRunner : IGendarmeRunner
    {
        private readonly IWinProcessUtil _processUtil;
        private readonly string _workDir;

        public GendarmeRunner(IWinProcessUtil processUtil, IOptions<AppSettings> options)
        {
            _processUtil = processUtil;
            _workDir = options.Value.WorkingDirectory;
        }

        public (string reportFileDir, string htmlResult) Run(GendarmeAnalyisRequest request)
        {
            var reportDir = $"{_workDir}\\Reports\\{request.RepoName}";

            var reportFileDir = Path.Join(reportDir, "report.html");

            if (!Directory.Exists(reportDir))
            {
                Directory.CreateDirectory(reportDir);
            }

            using var process = _processUtil.StartNew(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @$"/C gendarme --html {reportFileDir} --quiet {string.Join(' ', request.PathToAssemblies)}",
                WorkingDirectory = request.RepoBuildPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            process.WaitForExit();

            if ((process.ExitCode == 0 || process.ExitCode == 1) && File.Exists(reportFileDir )) return (reportFileDir, File.ReadAllText(reportFileDir));

            throw new Exception($"Error running Gendarme, encountered a non 0 exit code. {reportFileDir} {request.RepoBuildPath}");
        }
    }

    public class GendarmeAnalyisRequest
    {
        public string RepoName { get; set; }
        public IEnumerable<string> PathToAssemblies { get; set; }
        public string RepoBuildPath { get; set; }
    }

    public interface IGendarmeRunner
    {
        (string reportFileDir, string htmlResult) Run(GendarmeAnalyisRequest request);
    }
}
