using System;
using System.Collections.Generic;
using System.IO;
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
using RepoAnalyser.SqlServer.DAL.Interfaces;
using Serilog;
using static RepoAnalyser.SignalR.Objects.SignalRNotificationType;
using static RepoAnalyser.Objects.Helpers.HtmlHelper;

namespace RepoAnalyser.Logic
{
    public class RepositoryFacade : IRepositoryFacade
    {
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IMsBuildRunner _buildRunner;
        private readonly IGendarmeRunner _gendarmeRunner;
        private readonly IGitAdapter _gitAdapter;
        private readonly IHubContext<AppHub, IAppHub> _hub;
        private readonly IOctoKitAuthServiceAgent _octoKitAuthServiceAgent;
        private readonly IOctoKitGraphQlServiceAgent _octoKitGraphQlServiceAgent;
        private readonly IOctoKitServiceAgent _octoKitServiceAgent;

        public RepositoryFacade(IOctoKitGraphQlServiceAgent octoKitGraphQlServiceAgent,
            IOctoKitServiceAgent octoKitServiceAgent, IGitAdapter gitAdapter,
            IOctoKitAuthServiceAgent octoKitAuthServiceAgent,
            IHubContext<AppHub, IAppHub> hub, IBackgroundTaskQueue backgroundTaskQueue,
            IAnalysisRepository analysisRepository, IMsBuildRunner buildRunner, IGendarmeRunner gendarmeRunner)
        {
            _octoKitGraphQlServiceAgent = octoKitGraphQlServiceAgent;
            _octoKitServiceAgent = octoKitServiceAgent;
            _gitAdapter = gitAdapter;
            _octoKitAuthServiceAgent = octoKitAuthServiceAgent;
            _hub = hub;
            _backgroundTaskQueue = backgroundTaskQueue;
            _analysisRepository = analysisRepository;
            _buildRunner = buildRunner;
            _gendarmeRunner = gendarmeRunner;
        }

        public async Task<DetailedRepository> GetDetailedRepository(long repoId, string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var commits = _octoKitServiceAgent.GetCommitsForRepo(repoId, repository.LastUpdated, token);
            var repoStats = _octoKitServiceAgent.GetStatisticsForRepository(repoId, repository.LastUpdated, token);
            var results = await _analysisRepository.GetAnalysisResult(repoId);

            return new DetailedRepository
            {
                Repository = repository,
                Commits = await commits,
                Statistics = await repoStats,
                CodeOwners = results.CodeOwners,
                CodeOwnersLastUpdated = results.Result?.CodeOwnersLastRunDate,
                IsDotNetProject = _gitAdapter.IsDotNetProject(new GitActionRequest
                {
                    Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login),
                    RepoName = repository.Name,
                    RepoUrl = repository.PullUrl,
                    Token = token,
                    Username = user.Login
                }),
                CyclomaticComplexities = results.Complexities,
                CyclomaticComplexitiesLastUpdated = results.Result?.CyclomaticComplexitiesLastUpdated,
                StaticAnalysisLastUpdated = results.Result?.StaticAnalysisLastUpdated,
                StaticAnalysisHtml = await TryGetReportHtml(results.GendarmeReportDirectory)
            };
        }

        public Task<IEnumerable<UserRepositoryResult>> GetRepositories(string token, RepoFilterOptions filterOption)
        {
            return _octoKitGraphQlServiceAgent.GetRepositories(token, filterOption);
        }

        public async Task<IDictionary<string, string>> GetRepositoryCodeOwners(long repoId, string connectionId,
            string token)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Started calculating code owners for {repository.Name}",
                RepoAnalysisProgressUpdate));

            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);

            var filesInRepo = _gitAdapter.GetRelativeFilePathsForRepository(new GitActionRequest
            {
                BranchName = null,
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login),
                RepoName = repository.Name,
                RepoUrl = repository.PullUrl,
                Token = token,
                Username = user.Login
            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
                _hub.DirectNotify(connectionId, "Started building code-owner dictionary",
                    RepoAnalysisProgressUpdate));

            var result =
                await _octoKitServiceAgent.GetFileCodeOwners(token, filesInRepo, repository.Id, repository.LastUpdated);

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken =>
                _hub.DirectNotify(connectionId, "Finished calculating code-owner dictionary", RepoAnalysisDone));

            await _analysisRepository.UpsertAnalysisResults(new AnalysisResults
            {
                CodeOwnersLastRunDate = DateTime.Now,
                RepoId = repository.Id,
                RepoName = repository.Name
            }, result);

            return result;
        }

        public async Task<IEnumerable<GitHubCommit>> GetFileInformation(long repoId, string token, string filePath)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var repoFiles = _gitAdapter.GetRelativeFilePathsForRepository(new GitActionRequest
            {
                RepoUrl = repository.PullUrl,
                RepoName = repository.Name,
                Token = token,
                Username = user.Login,
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login)
            });

            return await _octoKitServiceAgent.GetFileCommits(repoId, token,
                repoFiles.FirstOrDefault(file => file.Contains(filePath)), repository.LastUpdated);
        }

        public async Task<IDictionary<string, int>> GetCyclomaticComplexities(string connectionId, string token,
            CyclomaticComplexityRequest request)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, request.RepoId);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Started calculating cyclomatic complexities for methods in {repository.Name}",
                RepoAnalysisProgressUpdate));

            var repoDir = _gitAdapter.GetRepoDirectory(new GitActionRequest
            {
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login),
                RepoName = repository.Name, RepoUrl = repository.PullUrl, Token = token, Username = user.Login
            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                "Building solution and analyzing assemblies",
                RepoAnalysisProgressUpdate));


            var result = CecilHelper.ReadAssembly(_buildRunner.Build(repoDir.Directory, repoDir.DotNetBuildDirectory),
                    _gitAdapter.GetSlnName(repository.Name))
                .ScanForMethods(request.FilesToSearch)
                .GetCyclomaticComplexities();

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                $"Finished calculating cyclomatic complexities for methods in {repository.Name}",
                RepoAnalysisDone));

            await _analysisRepository.UpsertAnalysisResults(new AnalysisResults
            {
                RepoId = repository.Id,
                RepoName = repository.Name,
                CyclomaticComplexitiesLastUpdated = DateTime.Now
            }, null, result);

            return result;
        }

        public async Task<RepoIssuesResponse> GetRepoIssues(long repoId, string token)
        {
            return new RepoIssuesResponse
            {
                Issues = await _octoKitServiceAgent.GetIssuesForRepo(token, repoId)
            };
        }

        public async Task<RepoSummaryResponse> GetRepoSummary(long repoId, string token, string connectionId)
        {
            var detailedRepo = await GetDetailedRepository(repoId, token);
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var issues = (await _octoKitServiceAgent.GetIssuesForRepo(token, repoId)).ToList();
            var loc = _gitAdapter.GetFileLocMetrics(new GitActionRequest
            {
                Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login),
                RepoName = detailedRepo.Repository.Name,
                RepoUrl = detailedRepo.Repository.PullUrl,
                Token = token,
                Username = user.Login
            });
            var res = new RepoSummaryResponse
            {
                OwnershipPercentage = detailedRepo.CodeOwners == null || detailedRepo.CodeOwners.Count == 0 
                    ? -1
                    : (double)detailedRepo.CodeOwners.Count(kv => kv.Value != null && kv.Value == user.Login) /
                    detailedRepo.CodeOwners.Count(kv => kv.Value != null) * 100,
                LocContributed = loc.Count > 0 ? loc.Sum(kv => kv.Value.Added) : 0,
                LocRemoved = loc.Count > 0 ? loc.Sum(kv => kv.Value.Removed) : 0,
                AverageCyclomaticComplexity =
                    detailedRepo.CyclomaticComplexities != null && detailedRepo.CyclomaticComplexities?.Count > 0
                        ? (double)detailedRepo.CyclomaticComplexities?.Average(x => x.Value)
                        : -1,
                TotalIssues = issues.Count,
                IssuesRaised = issues.Count > 0
                    ? issues.Count(x => (x.User?.Login ?? string.Empty) == user.Login)
                    : 0,
                IssuesSolved = issues.Count > 0
                    ? issues.Count(x => (x.ClosedBy?.Login ?? string.Empty) == user.Login)
                    : 0,
                AnalysisIssues = detailedRepo.StaticAnalysisHtml != AnalysisConstants.NoReportText ? 0 : -1
            };

            return res;
        }

        public async Task<RepoContributionResponse> GetRepoContributionVolumes(long repoId, string token,
            string connectionId)
        {
            var user = await _octoKitAuthServiceAgent.GetUserInformation(token);
            var repo = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);
            return new RepoContributionResponse
            {
                LocForFiles = _gitAdapter.GetFileLocMetrics(new GitActionRequest
                {
                    RepoUrl = repo.PullUrl,
                    RepoName = repo.Name,
                    Token = token,
                    Username = user.Login,
                    Email = user.Email ?? AnalysisConstants.FallbackEmail(user.Login)
                })
            };
        }

        public async Task<string> GetGendarmeReportHtml(long repoId, string token, string connectionId)
        {
            var repository = await _octoKitGraphQlServiceAgent.GetRepository(token, repoId);

            var (buildPath, assemblies) = _gitAdapter.GetBuiltAssembliesForRepo(repository.Name);

            if (!Directory.Exists(buildPath.DotNetBuildDirectory) ||
                !Directory.EnumerateFiles(buildPath.DotNetBuildDirectory).Any())
            {
                _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                    $"{repository.Name} has not yet been built, compiling now.",
                    RepoAnalysisProgressUpdate));
                _buildRunner.Build(buildPath.Directory, buildPath.DotNetBuildDirectory);
            }

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                "Invoking Gendarme report generation.",
                RepoAnalysisProgressUpdate));

            var (reportDir, htmlResult) = _gendarmeRunner.Run(new GendarmeAnalyisRequest
            {
                PathToAssemblies = assemblies,
                RepoName = repository.Name,
                RepoBuildPath = buildPath.DotNetBuildDirectory
            });

            _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => _hub.DirectNotify(connectionId,
                "Success! Saving analysis result.",
                RepoAnalysisDone));

            await _analysisRepository.UpsertAnalysisResults(new AnalysisResults
                    {RepoId = repoId, RepoName = repository.Name, StaticAnalysisLastUpdated = DateTime.Now}, null, null,
                reportDir);

            return CleanGendarmeHtml(htmlResult);
        }

        private async Task<string> TryGetReportHtml(string reportDir)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reportDir) || !File.Exists(reportDir))
                    return AnalysisConstants.NoReportText;

                var result = await File.ReadAllTextAsync(reportDir);

                return string.IsNullOrWhiteSpace(result) ? AnalysisConstants.NoReportText : CleanGendarmeHtml(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting report HTML");
                return AnalysisConstants.NoReportText;
            }
        }
    }
}