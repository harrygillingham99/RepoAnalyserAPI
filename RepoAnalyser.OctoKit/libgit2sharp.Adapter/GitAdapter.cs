using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;

namespace RepoAnalyser.Services.libgit2sharp.Adapter
{
    /*
     * A class to handle interfacing with libgit2sharp in order interact with native git
     * functions and manage repo directories
     */
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;

        public GitAdapter(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
        }

        public IEnumerable<string> GetRelativeFilePathsForRepository(GitActionRequest request,
            bool ignoreGitFiles = true)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, () =>
            {
                var repoDirectory = GetRepoBranchDirectory(request.RepoName, request.BranchName);

                var files = Directory.GetFiles(repoDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(path => !path.Contains(GetRepoBuildPath(request.RepoName, request.BranchName)))
                    .Select(path => path
                        .Replace(repoDirectory, string.Empty)
                        .Replace("\\", "/"));


                return ignoreGitFiles ? files.Where(x => !x.StartsWith("/.git")) : files;
            });
        }

        public bool IsDotNetProject(string repoName)
        {
            return Directory.GetFiles(GetRepoBranchDirectory(repoName), "*.sln",
                SearchOption.AllDirectories).Any();
        }

        public RepoDirectoryResult GetAllDirectoriesForRepo(GitActionRequest request)
        {
            return CloneOrPullLatestRepositoryThenInvoke(request, () =>
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

        public RepoDirectoryResult.RepoDirectory GetRepoDirectory(string repoName, string branchName = null)
        {
            var directory = GetRepoBranchDirectory(repoName, branchName);
            return new RepoDirectoryResult.RepoDirectory
            {
                Directory = directory,
                DotNetBuildDirectory = GetRepoBuildPath(repoName, branchName)
            };
        }

        private T CloneOrPullLatestRepositoryThenInvoke<T>(GitActionRequest request, Func<T> gitActionRequest)
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

            return gitActionRequest.Invoke();
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