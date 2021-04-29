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
using RepoAnalyser.Services.libgit2sharp.Adapter.Interfaces;
using RepoAnalyser.Services.OctoKit.GraphQL.Interfaces;
using RepoAnalyser.Services.OctoKit.Interfaces;
using RepoAnalyser.SignalR.Helpers;
using RepoAnalyser.SignalR.Hubs;
using RepoAnalyser.SqlServer.DAL.Interfaces;
using static RepoAnalyser.SignalR.Objects.SignalRNotificationType;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        private readonly IGitAdapter _gitAdapter;
        private readonly IHubContext<AppHub, IAppHub> _hub;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IMsBuildRunner _buildRunner;

        public RepositoryFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent, IGitAdapter gitAdapter, IOctoKitAuthServiceAgent octoKitAuthServiceAgent, 
            IHubContext<AppHub, IAppHub> hub, IBackgroundTaskQueue backgroundTaskQueue, IAnalysisRepository analysisRepository, IMsBuildRunner buildRunner)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
            _gitAdapter = gitAdapter;
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
            _hub = hub;
            _backgroundTaskQueue = backgroundTaskQueue;
            _analysisRepository = analysisRepository;
            _buildRunner = buildRunner;
        }

        public async Task<DetailedRepository> GetDetailedRepository(long repoId, string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var commits = _octoKitServiceAgent.GetCommitsForRepo(repoId,repository.LastUpdated, token);
            var repoStats = _octoKitServiceAgent.GetStatisticsForRepository(repoId,repository.LastUpdated ,token);
            var (results, codeOwners, cyclomaticComplexity) = await _analysisRepository.GetAnalysisResult(repoId);

            return new DetailedRepository
            {
                Repository = repository,
                Commits = await commits,
                Statistics = await repoStats,
                CodeOwners = codeOwners,
                CodeOwnersLastUpdated = results?.CodeOwnersLastRunDate,
                IsDotNetProject = _gitAdapter.IsDotNetProject(new GitActionRequest
                {
                    Email = user.Email ?? "test@RepoAnalyser.com",
                    RepoName = repository.Name,
                    RepoUrl = repository.PullUrl, 
                    Token = token, 
                    Username = user.Login
                }),
                CyclomaticComplexities = cyclomaticComplexity
            };
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetRepositories(token, filterOption);
        }

        public async Task<IDictionary<string, string>> GetRepositoryCodeOwners(long repoId, string connectionId, string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Started calculating code owners for {repository.Name}",
                RepoAnalysisProgressUpdate));

            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);

            var filesInRepo = _gitAdapter.GetRelativeFilePathsForRepository(new GitActionRequest
            {
                BranchName = null,
                Email = user.Email ?? "unknown@RepoAnalyser.test",
                RepoName = repository.Name,
                RepoUrl = repository.PullUrl,
                Token = token,
                Username = user.Login
            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
                _hub.DirectNotify(connectionId, "Started building code-owner dictionary",
                    RepoAnalysisProgressUpdate));

           

            var result = await _octoKitServiceAgent.GetFileCodeOwners(token, filesInRepo, repository.Id, repository.LastUpdated);
            
            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
                _hub.DirectNotify(connectionId,"Finished calculating code-owner dictionary", RepoAnalysisDone));

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _analysisRepository.UpsertAnalysisResults(new AnalysisResults
            {
                CodeOwnersLastRunDate = DateTime.Now,
                RepoId = repository.Id,
                RepoName = repository.Name
            }, result));

            return result;
        }

        public async Task<IEnumerable<GitHubCommit>> GetFileInformation(long repoId, string token, string filePath)
        {
            var repository = await  _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var repoFiles = _gitAdapter.GetRelativeFilePathsForRepository(new GitActionRequest
            {
                RepoUrl = repository.PullUrl,
                RepoName = repository.Name,
                Token = token,
                Username = user.Login,
                Email = user.Email ?? "unknown@RepoAnalyser.test"
            });

            return await _octoKitServiceAgent.GetFileCommits(repoId, token, repoFiles.FirstOrDefault(file => file.Contains(filePath)), (repository).LastUpdated);
        }

        public async Task<IDictionary<string, int>> GetCyclomaticComplexities(string connectionId, string token, CyclomaticComplexityRequest request)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, request.RepoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Started calculating cyclomatic complexities for methods in {repository.Name}",
                RepoAnalysisProgressUpdate));

            string pullRequestBranch = null;

            if (request.PullRequestNumber.HasValue)
            {
                pullRequestBranch =
                    (await _octoKitGraphQlServiceAgent.GetPullRequest(token, request.RepoId,
                        request.PullRequestNumber.Value)).HeadBranchName;
            }

            var repoDir = _gitAdapter.GetRepoDirectory(new GitActionRequest
            {
                BranchName = pullRequestBranch,
                Email = user.Email ?? "test@RepoAnalyser.com",
                RepoName = repository.Name, RepoUrl = repository.PullUrl, Token = token, Username = user.Login

            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                "Building solution and analyzing assemblies",
                RepoAnalysisProgressUpdate));


            var result = CecilHelper.ReadAssembly(_buildRunner.Build(repoDir.Directory, repoDir.DotNetBuildDirectory), _gitAdapter.GetSlnName(repository.Name, pullRequestBranch))
                .ScanForMethods(request.FilesToSearch)
                .GetCyclomaticComplexities();

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Finished calculating cyclomatic complexities for methods in {repository.Name}",
                RepoAnalysisDone));

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _analysisRepository.UpsertAnalysisResults(new AnalysisResults
            {
                RepoId = repository.Id,
                RepoName = repository.Name,
                CyclomaticComplexitiesLastUpdated = DateTime.Now
            }, null, result));

            return result;
        }
    }
}