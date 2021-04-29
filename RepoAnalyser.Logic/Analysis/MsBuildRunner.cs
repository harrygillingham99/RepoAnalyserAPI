using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gendarme.Framework.Helpers;
using RepoAnalyser.Logic.Analysis.Interfaces;
using Log = Serilog.Log;

namespace RepoAnalyser.Logic.Analysis
{
    public class MsBuildRunner : IMsBuildRunner
    {
        public string Build(string repoDirectory, string outputDir)
        {
            var slnFilesLastWritten = new Dictionary<string, DateTime>();

            var repoDir = repoDirectory;

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
                        $"build {pathToProjectFile} --output {outputDir} --configuration Release --nologo",
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }

            };

            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Log.Error($"dotnet build error: {process.StandardError.ReadToEnd()}, build dir: {pathToProjectFile}, output dir: {outputDir}");
                throw new Exception($"Build Failed attempting to compile {pathToProjectFile.Split('\\').Last()}. Received a non 0 exit code.");
            }

            process.Kill();

            return outputDir;
        }
    }
}