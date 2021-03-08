using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;

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
            var repoDir = Path.Combine(_workDir, repoName);

            outputDir ??= Path.Combine(repoDir, "build");

            var pathToProjectFile = Directory.GetFiles(repoDir, "*.sln", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(pathToProjectFile))
            {
                throw new Exception("No project file found. Solution cannot be built");
            }

            var process = new Process
            {
               StartInfo = new ProcessStartInfo("dotnet")
               {
                   Arguments = $"build {pathToProjectFile} --output {outputDir} --configuration Release --nologo --verbosity minimal"
               }
            };

           process.Start();

           process.WaitForExit();

           return outputDir;
        }
    }

    public interface IMsBuildRunner
    {
        string Build(string repoName, string outputDir = null);
    }
}
