﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Options;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Services.libgit2csharp.Adapter.Interfaces;
using Commit = LibGit2Sharp.Commit;
using Credentials = LibGit2Sharp.Credentials;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace RepoAnalyser.Services.libgit2csharp.Adapter
{
    public class GitAdapter : IGitAdapter
    {
        private readonly string _workDir;

        public GitAdapter(IOptions<AppSettings> options)
        {
            _workDir = options.Value.WorkingDirectory;
        }


        public IEnumerable<Commit> GetCommits(GitActionRequest request)
        {
            var repoLocation = GetOrCloneRepository(request);
            using var repo = new Repository(repoLocation);
            foreach (var commit in repo.Commits) yield return commit;
        }

        public string GetOrCloneRepository(GitActionRequest request)
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
                        $"Error in {nameof(GetOrCloneRepository)}, merge conflicts for {GetRepoNameFromUrl(request.RepoUrl)}");
            }

            return repoDirectory;
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