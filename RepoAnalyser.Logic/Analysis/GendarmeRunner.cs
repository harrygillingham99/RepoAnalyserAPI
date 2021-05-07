using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Services.ProcessUtility;
using Log = Serilog.Log;

namespace RepoAnalyser.Logic.Analysis
{
    public class GendarmeRunner : IGendarmeRunner
    {
        private readonly IWinProcessUtil _processUtil;
        private readonly string _workDir;
        private readonly int[] _acceptableExitCodes = {0,1 };

        public GendarmeRunner(IWinProcessUtil processUtil, IOptions<AppSettings> options)
        {
            _processUtil = processUtil;
            _workDir = options.Value.WorkingDirectory;
        }

        public (string reportFileDir, string htmlResult) Run(GendarmeAnalyisRequest request)
        {
            var reportDir = $"{_workDir}\\Reports\\{request.RepoName}";

            var reportFileDir = Path.Join(reportDir, "report.html");

            if (!Directory.Exists(reportDir)) Directory.CreateDirectory(reportDir);

            var (error, exitCode) = _processUtil.StartNewReadError(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = @$"/C gendarme --html {reportFileDir} --quiet {string.Join(' ', request.PathToAssemblies)}",
                WorkingDirectory = request.RepoBuildPath,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (_acceptableExitCodes.Contains(exitCode) && File.Exists(reportFileDir))
                return (reportFileDir, File.ReadAllText(reportFileDir));

            Log.Error("Error running Gendarme: " + error);
            throw new Exception($"Error running Gendarme: {error}");
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
