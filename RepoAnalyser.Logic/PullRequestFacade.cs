﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.SignalR.Hubs;

namespace RepoAnalyser.Logic
{
    public class PullRequestFacade : IPullRequestFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;

        public PullRequestFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token, PullRequestFilterOption filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetPullRequests(token, filterOption);
        }

        public Task<DetailedPullRequest> GetDetailedPullRequest(string token, long repoId, int pullNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}
