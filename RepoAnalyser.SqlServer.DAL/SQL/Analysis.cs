using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.SqlServer.DAL.SQL
{
    public static partial class Sql
    {
        public static string UpsertAnalysisResultsInfo = @"
		IF EXISTS (
				SELECT *
				FROM dbo.RepositoryAnalysis
				WHERE GitHubRepositoryId = @RepoId
				)
			UPDATE dbo.RepositoryAnalysis
			SET CodeOwnersLastUpdated = @CodeOwnersLastRunDate
			WHERE GitHubRepositoryId = @RepoId;
		ELSE
			INSERT INTO [dbo].[RepositoryAnalysis] (
				    [GitHubRepositoryId]
				        ,[RepositoryName]
				            ,[CodeOwnersLastUpdated]
				            )
			            VALUES (
				            @RepoId
				            ,@RepoName
				            ,@CodeOwnersLastRunDate)";

        public static string GetRepoAnalysisRunInfo = @"
		SELECT [RepositoryId]
			,[GitHubRepositoryId]
			,[RepositoryName]
			,[CodeOwnersLastUpdated] AS CodeOwnersLastRunDate
		FROM [RepoAnalyser].[dbo].[RepositoryAnalysis]
		WHERE GitHubRepositoryId = @RepoId
		
		SELECT [AnalysisResult]
		FROM [RepoAnalyser].[dbo].[CodeOwners]
		WHERE GitHubRepositoryId = @RepoId";

        public static string UpsertCodeOwnerAnalysis = @"
		IF EXISTS (
				SELECT *
				FROM dbo.CodeOwners
				WHERE GitHubRepositoryId = @RepoId
				)
			UPDATE dbo.CodeOwners
			SET AnalysisResult = @Result
			WHERE GitHubRepositoryId = @RepoId;
		ELSE
			INSERT INTO [dbo].[CodeOwners] (
				[GitHubRepositoryId]
				,[AnalysisResult]
				)
			VALUES (
				@RepoId
				,@Result
				)";	
    }
}
