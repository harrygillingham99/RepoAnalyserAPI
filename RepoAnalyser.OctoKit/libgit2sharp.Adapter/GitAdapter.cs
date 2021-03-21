using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Xsl;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Octokit.GraphQL.Model;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using Repository = LibGit2Sharp.Repository;

namespace RepoAnalyser.Services.libgit2sharp.Adapter
{
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;
        private readonly string _defaultBuildPathTemplate;

        public GitAdapter(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
            _defaultBuildPathTemplate = Path.Combine(_workDir, "{0}", "{1}", AnalysisConstants.DefaultBuildPath);
        }

        public string CloneOrPullLatestRepository(GitActionRequest request)
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
                        $"Error in {nameof(CloneOrPullLatestRepository)}, merge conflicts for {request.RepoName}");
            }

            return repoDirectory;
        }

        public IEnumerable<string> GetRelativeFilePathsForRepository(string repoName, string branchName = null, bool ignoreGitFiles = true)
        {
            var repoDirectory = GetRepoBranchDirectory(repoName, branchName);

            var files = Directory.GetFiles(repoDirectory, "*.*", SearchOption.AllDirectories)
                .Where(path => !path.Contains(FormatBuildPath(repoName)))
                .Select(path => path
                    .Replace(GetRepoBranchDirectory(repoName, branchName), string.Empty)
                    .Replace("\\", "/"));
                    

            return ignoreGitFiles ? files.Where(x => !x.StartsWith("/.git")) : files;
        }

        public bool IsDotNetProject(string repoName)
        {
            return Directory.GetFiles(BuildRepoDirectories(repoName).DefaultRemoteDir.Directory, "*.sln", SearchOption.AllDirectories).Any();
        }

        public RepoDirectoryResult BuildRepoDirectories(string repoName)
        {
            var branches = Directory.EnumerateFiles(GetRepoRootDirectory(repoName)).ToList();
            var defaultBranchDir = branches.FirstOrDefault(dir => dir.Contains($"{repoName}-DefaultRemote"));
            return new RepoDirectoryResult
            {
                DefaultRemoteDir = new RepoDirectoryResult.RepoDirectory
                {
                    Directory = defaultBranchDir,
                    DotNetBuildDirectory = FormatBuildPath(defaultBranchDir)
                },
                BranchNameDirMap = branches.Where(dir => dir != $"{repoName}-DefaultRemote")
                    .ToDictionary(key => key.Replace($"{repoName}-", string.Empty), value => new RepoDirectoryResult.RepoDirectory
                    {
                        Directory = value,
                        DotNetBuildDirectory = FormatBuildPath(repoName, value.Replace($"{repoName}-", string.Empty))
                    })
            };
        }

        public RepoDirectoryResult.RepoDirectory BuildRepoDirectory(string repoName, string branchName = null)
        {
            var directory = GetRepoBranchDirectory(repoName, branchName);
            return new RepoDirectoryResult.RepoDirectory
            {
                Directory = directory,
                DotNetBuildDirectory = FormatBuildPath(repoName, branchName)
            };
        }

        private Credentials BuildCredentials(string token)
        {
            return new UsernamePasswordCredentials
            {
                Username = token,
                Password = ""
            };
        }

        private string GetRepoBranchDirectory(string repoName, string branchName = null) => Path.Combine(GetRepoRootDirectory(repoName),!string.IsNullOrWhiteSpace(branchName) ? $"{repoName}-{branchName}" : $"{repoName}-DefaultRemote");

        private string GetRepoRootDirectory(string repoName) => Path.Combine(_workDir, repoName);

        private string FormatBuildPath(string repoName, string branchName = null) => string.Format(_defaultBuildPathTemplate, repoName, !string.IsNullOrWhiteSpace(branchName) ? $"{repoName}-{branchName}" : $"{repoName}-DefaultRemote");
    }
}