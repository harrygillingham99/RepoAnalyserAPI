﻿using System.Collections.Generic;
using System.Threading.Tasks;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Logic.Interfaces
{
    public interface IPullRequestFacade
    {
        Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption filterOption);
        Task<DetailedPullRequest> GetDetailedPullRequest(string token, long repoId, int pullNumber);
        Task<PullFileInfo> GetPullFileInformation (long repoId, int pullNumber, string fileName, string token);
        Task<IDictionary<string, int>> GetPullCyclomaticComplexity (long repoId, int pullNumber, string token, string connectionId);
    }
}