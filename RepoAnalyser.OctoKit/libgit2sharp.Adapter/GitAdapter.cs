using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;

namespace RepoAnalyser.Services.libgit2sharp.Adapter
{
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;
        private readonly string _defaultBuildPathTemplate;

        public GitAdapter(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
            _defaultBuildPathTemplate = Path.Combine(_workDir, "{0}", AnalysisConstants.DefaultBuildPath);
        }

        public string CloneOrPullLatestRepository(GitActionRequest request)
        {
            var repoDirectory = Path.Combine(_workDir, request.RepoName);
            if (!Directory.Exists(repoDirectory))
            {
                _ = Repository.Clone(request.RepoUrl, repoDirectory, new CloneOptions
                {
                    CredentialsProvider = (url, fromUrl, types) => BuildCredentials(request.Token)
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

        public IEnumerable<string> GetRelativeFilePathsForRepository(string repoDirectory, string repoName,
            bool ignoreGitFiles = true)
        {
            var files = Directory.GetFiles(repoDirectory, "*.*", SearchOption.AllDirectories)
                .Where(path => !path.Contains(FormatBuildPath(repoName)))
                .Select(path => path
                    .Replace(_workDir, "")
                    .Replace("\\", "/")
                    .Replace($"/{repoName}", ""));

            return ignoreGitFiles ? files.Where(x => !x.StartsWith("/.git")) : files;
        }

        public bool IsDotNetProject(string repoName)
        {
            return Directory.GetFiles(Path.Combine(_workDir, repoName), "*.sln", SearchOption.AllDirectories).Any();
        }

        private Credentials BuildCredentials(string token)
        {
            return new UsernamePasswordCredentials
            {
                Username = token,
                Password = ""
            };
        }

        private string FormatBuildPath(string repoName) => string.Format(_defaultBuildPathTemplate, repoName);
    }
}