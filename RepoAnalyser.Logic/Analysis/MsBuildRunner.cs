using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using RepoAnalyser.Logic.Analysis.Interfaces;
using RepoAnalyser.Services.ProcessUtility;
using Serilog;

namespace RepoAnalyser.Logic.Analysis
{
    public class MsBuildRunner : IMsBuildRunner
    {
        private readonly IWinProcessUtil _processUtil;

        public MsBuildRunner(IWinProcessUtil processUtil)
        {
            _processUtil = processUtil;
        }

        public string Build(string repoDirectory, string outputDir)
        {
            var slnFilesLastWritten = new Dictionary<string, DateTime>();

            var repoDir = repoDirectory;

            var slnPaths = Directory.GetFiles(repoDir, "*.sln", SearchOption.AllDirectories);

            foreach (var sln in slnPaths) slnFilesLastWritten.Add(sln, File.GetLastWriteTime(sln));

            var pathToProjectFile = slnFilesLastWritten.OrderByDescending(x => x.Value).First().Key;

            if (string.IsNullOrWhiteSpace(pathToProjectFile))
                throw new Exception("No project file found. Solution cannot be built");

            using var process = _processUtil.StartNew(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    $"build {pathToProjectFile} --output {outputDir} --configuration Release --nologo",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,

            });

            var processError = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0) return outputDir;

            Log.Error(
                $"dotnet build error: {processError}, build dir: {pathToProjectFile}, output dir: {outputDir}");
            throw new Exception(
                $"Build Failed attempting to compile {pathToProjectFile.Split('\\').Last()}. Received a non 0 exit code.");

        }
    }
}