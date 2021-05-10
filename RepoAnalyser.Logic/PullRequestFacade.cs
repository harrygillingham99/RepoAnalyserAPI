using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Octokit;
using RepoAnalyser.Logic.Analysis;
using RepoAnalyser.Logic.Analysis.Interfaces;
using RepoAnalyser.Logic.BackgroundTaskQueue;
using RepoAnalyser.Logic.Interfaces;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.API.Responses;
using RepoAnalyser.Objects.Constants;
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;
using RepoAnalyser.SignalR.Helpers;
using RepoAnalyser.SignalR.Hubs;
using static RepoAnalyser.SignalR.Objects.SignalRNotificationType;

namespace RepoAnalyser.Logic
{
    public class PullRequestFacade : IPullRequestFacade
    {
        private readonly IGitAdapter _gitAdapter;
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IMsBuildRunner _buildRunner;
        private readonly IHubContext<AppHub, IAppHub> _hub;

        public PullRequestFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent, IGitAdapter gitAdapter,
            IOctoKitAuthServiceAgent octoKitAuthServiceAgent, IBackgroundTaskQueue backgroundTaskQueue, IMsBuildRunner buildRunner, IHubContext<AppHub, IAppHub> hub)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
            _gitAdapter = gitAdapter;
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
            _backgroundTaskQueue = backgroundTaskQueue;
            _buildRunner = buildRunner;
            _hub = hub;
        }

        public Task<IEnumerable<UserPullRequestResult>> GetPullRequests(string token,
            PullRequestFilterOption filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetPullRequests(token, filterOption);
        }

        public async Task<DetailedPullRequest> GetDetailedPullRequest(string token, long repoId, int pullNumber)
        {
            var detailedCommits = new List<GitHubCommit>();
            var pullRequest = await _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var repo = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var gitAction = new GitActionRequest
            {
                RepoUrl = repo.PullUrl,
                RepoName = repo.Name,
                Token = token,
                Username = user.Login,
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login)
            };
            var repoRelativePaths = _gitAdapter.GetRelativeFilePathsForRepository(gitAction);
            var commits = (await _octoKitServiceAgent.GetCommitsForPullRequest(repoId, pullNumber, token,
                pullRequest.UpdatedAt?.DateTime ?? DateTime.Now)).ToList();

            foreach (var commit in commits)
                detailedCommits.Add(await _octoKitServiceAgent.GetDetailedCommit(token, repoId, commit.Sha));

            var filesModifiedInPull = detailedCommits
                .SelectMany(commit => commit?.Files ?? new List<GitHubCommitFile>())
                .Select(file => file.Filename ?? "Untracked File").Distinct();

            return new DetailedPullRequest
            {
                PullRequest = pullRequest,
                Commits = detailedCommits,
                ModifiedFilePaths = repoRelativePaths.Where(path => filesModifiedInPull.Any(path.Contains)),
                IsDotNetProject = _gitAdapter.IsDotNetProject(gitAction)
            };
        }

        public async Task<PullFileInfo> GetPullFileInformation(long repoId, int pullNumber, string fileName,
            string token)
        {
            var pull = await _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var pullCommits = await _octoKitServiceAgent.GetCommitsForPullRequest(repoId, pullNumber, token,
                pull.UpdatedAt?.DateTime ?? DateTime.Now);
            var detailedPullCommits = await Task.WhenAll(pullCommits.Select(commit =>
                _octoKitServiceAgent.GetDetailedCommit(token, repoId, commit.Sha)));
            var commitsForFile = detailedPullCommits.SelectMany(x => x.Files)
                .Where(file => file.Filename.Contains(fileName)).ToList();

            return new PullFileInfo
            {
                Additions = commitsForFile.Sum(x => x.Additions),
                Deletions = commitsForFile.Sum(x => x.Deletions),
                CommitsThatIncludeFile = commitsForFile.Count
            };
        }

        public async Task<IDictionary<string, int>> GetPullCyclomaticComplexity(long repoId, int pullNumber, string token, string connectionId)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var pull = await _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Started calculating cyclomatic complexities for methods in {repository.Name}",
                PullRequestAnalysisProgressUpdate));

            var repoDir = _gitAdapter.GetRepoDirectory(new GitActionRequest
            {
                BranchName = pull.HeadBranchName,
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login),
                RepoName = repository.Name,
                RepoUrl = repository.PullUrl,
                Token = token,
                Username = user.Login

            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                "Building solution and analyzing assemblies",
                PullRequestAnalysisProgressUpdate));


            var result = CecilHelper.ReadAssembly(_buildRunner.Build(repoDir.Directory, repoDir.DotNetBuildDirectory), _gitAdapter.GetSlnName(repository.Name, pull.HeadBranchName))
                .ScanForMethods(null)
                .GetCyclomaticComplexities();

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Finished calculating cyclomatic complexities for methods in {repository.Name}",
                PullRequestAnalysisDone));

            return result;
        }

        public async Task<PullDiscussionResult> GetPullIssuesAndDiscussion(long repoId, int pullNumber, string token)
        {
            var pull =  _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            return await _octoKitServiceAgent.GetPullReviewInformation(token, repoId, pullNumber, (await pull).UpdatedAt?.DateTime ?? DateTime.Now);
        }

        public async Task<PullSummaryResponse> GetPullRequestSummary(long repoId, int pullNumber, string token)
        {
            var pull = _octoKitGraphQlServiceAgent.GetPullRequest(token, repoId, pullNumber);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var reviewInfo = await _octoKitServiceAgent.GetPullReviewInformation(token, repoId, pullNumber,
                (await pull).UpdatedAt?.DateTime ?? DateTime.Now);

            return new PullSummaryResponse
            {
                IsReviewer = reviewInfo.AssignedReviewers.Any(users => users.Login == user.Login),

            };
        }
    }
}