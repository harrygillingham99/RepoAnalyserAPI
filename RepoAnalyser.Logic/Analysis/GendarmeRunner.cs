using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gendarme.Framework.Helpers;
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

        public GendarmeRunner(IWinProcessUtil processUtil, IOptions<AppSettings> options)
        {
            _processUtil = processUtil;
            _workDir = options.Value.WorkingDirectory;
        }

        public string Run(GendarmeAnalyisRequest request)
        {
            var reportDir = $"{_workDir}/Reports/{request.RepoName}/report.html";
            using var process = _processUtil.StartNew(new ProcessStartInfo
            {
                FileName = "gendarme",
                Arguments =
                    $"--html {reportDir} --quiet {string.Join(' ', request.PathToAssemblies)}",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,

            });

            var processError = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0) return File.ReadAllText(reportDir);

            Log.Error($"Gendarme error: {processError}");
            throw new Exception("Error running Gendarme, encountered a non 0 exit code.");
        }
    }

    public class GendarmeAnalyisRequest
    {
        public string RepoName { get; set; }
        public IEnumerable<string> PathToAssemblies { get; set; }

    }

    public interface IGendarmeRunner
    {
        string Run(GendarmeAnalyisRequest request);
    }
}
