using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FuzzySharp;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Config;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.ProcessUtility;
using Serilog;
using Exception = System.Exception;
using Repository = LibGit2Sharp.Repository;

namespace RepoAnalyser.Services.libgit2sharp.Adapter
{
    /*
     * A class to handle interfacing with libgit2sharp and native git commands via the git cli in order interact with native git
     * functions and manage repo directories
     */
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;
        private readonly IWinProcessUtil _processUtil;

        public GitAdapter(IOptions<AppSettings> options, IWinProcessUtil processUtil)
        {
            _processUtil = processUtil;
            _workDir = options.Value.WorkingDirectory;
        }

        public IEnumerable<string> GetRelativeFilePathsForRepository(GitActionRequest request,
            bool ignoreGitFiles = true)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, (repoDirectory) =>
            {
                var files = Directory.GetFiles(repoDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(path => !path.Contains(GetRepoBuildPath(request.RepoName, request.BranchName)))
                    .Select(path => path
                        .Replace(repoDirectory, string.Empty)
                        .Replace("\\", "/"));


                return ignoreGitFiles ? files.Where(x => !x.StartsWith("/.git")) : files;
            });
        }

        public bool IsDotNetProject(GitActionRequest request)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, repoDirectory => Directory.GetFiles(GetRepoBranchDirectory(request.RepoName, request.BranchName), "*.sln",
                SearchOption.AllDirectories).Any());

        }

        public string GetSlnName(string repoName, string branchName = null)
        {
            return Directory.GetFiles(GetRepoBranchDirectory(repoName), "*.sln",
                SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTime).First().Replace(".sln", string.Empty).Split('\\').Last();
        }

        public RepoDirectoryResult GetAllDirectoriesForRepo(GitActionRequest request)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, (repoDirectory) =>
            {
                var branches = Directory.EnumerateFiles(GetRepoRootDirectory(request.RepoName)).ToList();
                var defaultBranchDir =
                    branches.FirstOrDefault(dir => dir.Contains($"{request.RepoName}-DefaultRemote"));

                if (string.IsNullOrWhiteSpace(defaultBranchDir))
                    throw new NullReferenceException(
                        "Repository not found");

                return new RepoDirectoryResult
                {
                    DefaultRemoteDir = new RepoDirectoryResult.RepoDirectory
                    {
                        Directory = defaultBranchDir,
                        DotNetBuildDirectory = GetRepoBuildPath(defaultBranchDir)
                    },
                    BranchNameDirMap = branches.Where(dir => dir != defaultBranchDir)
                        .ToDictionary(key => key.Replace($"{request.RepoName}-", string.Empty), value =>
                            new RepoDirectoryResult.RepoDirectory
                            {
                                Directory = value,
                                DotNetBuildDirectory =
                                    GetRepoBuildPath(request.RepoName,
                                        value.Replace($"{request.RepoName}-", string.Empty))
                            })
                };
            });
        }

        public RepoDirectoryResult.RepoDirectory GetRepoDirectory(GitActionRequest request)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, repoDirectory => new RepoDirectoryResult.RepoDirectory
            {
                Directory = repoDirectory,
                DotNetBuildDirectory = GetRepoBuildPath(request.RepoName, request.BranchName)
            });

        }

        public (RepoDirectoryResult.RepoDirectory repoDirectory, IEnumerable<string> assemblyNames) GetBuiltAssembliesForRepo(string repoName,
            string branchName = null)
        {
            var repoDir = GetRepoBuildPath(repoName, branchName);
            var slnName = GetSlnName(repoName, branchName);
            var results = Directory.EnumerateFiles(repoDir, "*.dll", SearchOption.AllDirectories).Select(dir => dir.Replace(repoDir, string.Empty).Replace("\\", string.Empty));

            return (new RepoDirectoryResult.RepoDirectory{Directory = GetRepoBranchDirectory(repoName, branchName), DotNetBuildDirectory = repoDir}, 
                results.Where(dir =>
                    Fuzz.PartialRatio(dir.Replace(".dll",string.Empty).ToLower(), slnName.ToLower()) > 85 ||
                    Fuzz.PartialRatio(repoName.ToLower(), dir.Replace(".dll", string.Empty).ToLower()) > 85));

        }

        public IDictionary<string, AddedRemoved> GetFileLocMetrics(GitActionRequest request)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, repoDir =>
            {
                //This is by far the fastest way to get these metrics for a repository
                var (output, error, exitCode) = _processUtil.StartNewReadOutputAndError(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C git --no-pager log --author={request.Username} --pretty=tformat: --numstat",
                    WorkingDirectory = repoDir,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
             

                static Dictionary<string, AddedRemoved> ParseGitLogResult(string logResult)
                {
                    var results = new Dictionary<string, AddedRemoved>();
                    var split = Regex.Split(logResult, "\r\n|\r|\n");
                    foreach (var line in split)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var splitLine = Regex.Split(line, "\\t");
                        var fileName = splitLine[2];
                        var added = int.TryParse(splitLine[0], out var linesAdded);
                        var removed = int.TryParse(splitLine[1], out var linesRemoved);
                        if (added && removed)
                        {
                            //some rows will contain diffs where file names have changed so we will take the new one and add the metrics
                            if (fileName.Contains("=>"))
                                fileName = fileName.Split('>').Last().Replace("}",
                                    string.Empty).Trim();
                            if (!results.ContainsKey(fileName))
                            {
                                results.Add(fileName, new AddedRemoved(linesAdded, linesRemoved));
                            }
                            else
                            {
                                var changes = results[fileName];
                                changes.Added += linesAdded;
                                changes.Removed += linesRemoved;
                                results[fileName] = changes;
                            }
                        }
                        else
                        {
                            Log.Error("Failed to parse Git Log line " + line);
                        }
                    }

                    return results;
                }

                if (exitCode == 0) return ParseGitLogResult(output.ToString());

                Log.Error(
                    $"git log error: {error}");
                throw new Exception(
                    $"Failed to retrieve git log information for {request.RepoName}. Received a non 0 exit code.");
                ;
            });
        }

        private T CloneOrPullLatestRepositoryThenInvoke<T>(GitActionRequest request, Func<string, T> gitActionRequest)
        {
            var repoDirectory = GetRepoBranchDirectory(request.RepoName, request.BranchName);
            if (!Directory.Exists(repoDirectory))
            {
                _ = Repository.Clone(request.RepoUrl, repoDirectory, new CloneOptions
                {
                    CredentialsProvider = (url, fromUrl, types) => BuildCredentials(request.Token),
                    BranchName = request.BranchName
                });
            }
            else
            {
                using var repo = new Repository(repoDirectory);
                var options = new FetchOptions
                {
                    CredentialsProvider = (url, fromUrl, types) => BuildCredentials(request.Token)
                };
                var result = Commands.Pull(repo, new Signature(request.Username, request.Email, DateTimeOffset.Now),
                    new PullOptions {FetchOptions = options});
                if (result.Status == MergeStatus.Conflicts)
                    throw new Exception(
                        $"Error in {nameof(CloneOrPullLatestRepositoryThenInvoke)}, merge conflicts for {repoDirectory}");
            }

            return gitActionRequest.Invoke(repoDirectory);
        }

        private Credentials BuildCredentials(string token)
        {
            return new UsernamePasswordCredentials
            {
                Username = token,
                Password = ""
            };
        }

        private string GetRepoBranchDirectory(string repoName, string branchName = null)
        {
            return Path.Combine(GetRepoRootDirectory(repoName),
                !string.IsNullOrWhiteSpace(branchName) ? $"{repoName}-{branchName}" : $"{repoName}-DefaultRemote");
        }

        private string GetRepoRootDirectory(string repoName)
        {
            return Path.Combine(_workDir, repoName);
        }

        private string GetRepoBuildPath(string repoName, string branchName = null)
        {
            return Path.Combine(GetRepoBranchDirectory(repoName, branchName), AnalysisConstants.DefaultBuildPath);
        }
    }
}