using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using RepoAnalyser.Logic.Analysis.Interfaces;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.Constants;

namespace RepoAnalyser.Logic.Analysis
{
    public class MsBuildRunner : IMsBuildRunner
    {
        private readonly string _workDir;

        public MsBuildRunner(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
        }

        public string Build(string repoName, string outputDir = null)
        {
            var slnFilesLastWritten = new Dictionary<string, DateTime>();

            var repoDir = Path.Combine(_workDir, repoName);

            outputDir ??= Path.Combine(repoDir, AnalysisConstants.DefaultBuildPath);

            var slnPaths = Directory.GetFiles(repoDir, "*.sln", SearchOption.AllDirectories);

            foreach (var sln in slnPaths) slnFilesLastWritten.Add(sln, File.GetLastWriteTime(sln));

            var pathToProjectFile = slnFilesLastWritten.OrderByDescending(x => x.Value).First().Key;

            if (string.IsNullOrWhiteSpace(pathToProjectFile))
                throw new Exception("No project file found. Solution cannot be built");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet")
                {
                    Arguments =
                        $"build {pathToProjectFile} --output {outputDir} --configuration Release --nologo --verbosity minimal"
                }
            };

            process.Start();

            process.WaitForExit();

            return outputDir;
        }
    }
}