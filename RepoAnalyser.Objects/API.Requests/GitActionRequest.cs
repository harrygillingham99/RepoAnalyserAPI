﻿using Octokit;

namespace RepoAnalyser.Objects.API.Requests
{
    public class GitActionRequest
    {
        public GitActionRequest(string repoUrl, string token, string username, string email)
        {
            RepoUrl = repoUrl;
            Token = token;
            Username = username;
            Email = email;
        }

        public GitActionRequest(string repoUrl, string token, Account user)
        {
            RepoUrl = repoUrl;
            Token = token;
            Email = user.Email;
            Username = user.Login;
        }

        public GitActionRequest(RepositoryCommitsRequest request, string token)
        {
            RepoUrl = request.RepositoryUrl;
            Token = token;
            Username = request.Username;
            Email = request.Email;
        }

        public string RepoUrl { get; }
        public string Token { get; }
        public string Username { get; }
        public string Email { get; }
    }
}