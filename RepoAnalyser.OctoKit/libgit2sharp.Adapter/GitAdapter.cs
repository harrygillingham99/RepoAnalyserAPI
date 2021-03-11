using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using Octokit.GraphQL.Core.Builders;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using Credentials = LibGit2Sharp.Credentials;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace RepoAnalyser.Services.libgit2sharp.Adapter
{
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;

        public GitAdapter(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
        }

        public string CloneOrPullLatestRepository(GitActionRequest request)
        {
            var repoDirectory = Path.Combine(_workDir, GetRepoNameFromUrl(request.RepoUrl));
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
                        $"Error in {nameof(CloneOrPullLatestRepository)}, merge conflicts for {GetRepoNameFromUrl(request.RepoUrl)}");
            }

            return repoDirectory;
        }

        public IEnumerable<string> GetRelativeFilePathsForRepository(string repoDirectory, string repoName)
        {
            return Directory.GetFiles(repoDirectory, "*.*", SearchOption.AllDirectories).Select(path => path.Replace(_workDir,"").Replace("\\", "/").Replace($"/{repoName}", ""));
        }

        private string GetRepoNameFromUrl(string repoUrl)
        {
            return repoUrl.Split('/').Last().Replace(".git", string.Empty);
        }

        private Credentials BuildCredentials(string token)
        {
            return new UsernamePasswordCredentials
            {
                Username = token,
                Password = ""
            };
        }
    }
}