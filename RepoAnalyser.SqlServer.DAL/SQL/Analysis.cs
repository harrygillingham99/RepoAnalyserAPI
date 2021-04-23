﻿namespace RepoAnalyser.SqlServer.DAL.SQL
{
    public static partial class Sql
    {
        public static string UpsertAnalysisResultsInfo = @"
		IF NOT EXISTS (
				SELECT *
				FROM dbo.RepositoryAnalysis
				WHERE GitHubRepositoryId = @RepoId
				)
			INSERT INTO [dbo].[RepositoryAnalysis] (
				    [GitHubRepositoryId]
				        ,[RepositoryName]
				            )
			            VALUES (
				            @RepoId
				            ,@RepoName)";

        public static string GetRepoAnalysisRunInfo = @"
		SELECT ra.[RepositoryId]
			,ra.[GitHubRepositoryId]
			,ra.[RepositoryName]
			,co.LastUpdated AS CodeOwnersLastRunDate
			,cc.LastUpdated AS CyclomaticComplexitiesLastUpdated
		FROM [RepoAnalyser].[dbo].[RepositoryAnalysis] AS ra
		JOIN CodeOwners AS co ON co.GitHubRepositoryId = ra.GitHubRepositoryId
		JOIN CyclomaticComplexity AS cc ON cc.GitHubRepositoryId = ra.GitHubRepositoryId
		WHERE ra.GitHubRepositoryId = @RepoId

		SELECT [AnalysisResult]
		FROM [RepoAnalyser].[dbo].[CodeOwners]
		WHERE GitHubRepositoryId = @RepoId
        
        SELECT [AnalysisResult]
		FROM [RepoAnalyser].[dbo].[CyclomaticComplexity]
		WHERE GitHubRepositoryId = @RepoId";

        public static string UpsertCodeOwnerAnalysis = @"
		IF EXISTS (
				SELECT *
				FROM dbo.CodeOwners
				WHERE GitHubRepositoryId = @RepoId
				)
			UPDATE dbo.CodeOwners
			SET AnalysisResult = @Result,
                LastUpdated = @LastUpdated
			WHERE GitHubRepositoryId = @RepoId;
		ELSE
			INSERT INTO [dbo].[CodeOwners] (
				[GitHubRepositoryId]
				,[AnalysisResult]
                ,[LastUpdated]
				)
			VALUES (
				@RepoId
				,@Result
                ,@LastUpdated
				)";

        public static string UpsertCyclomaticComplexityAnalysis = @"
		IF EXISTS (
				SELECT *
				FROM [RepoAnalyser].[dbo].[CyclomaticComplexity]
				WHERE GitHubRepositoryId = @RepoId
				)
			UPDATE [RepoAnalyser].[dbo].[CyclomaticComplexity]
			SET AnalysisResult = @Result,
                LastUpdated = @LastUpdated
			WHERE GitHubRepositoryId = @RepoId;
		ELSE
			INSERT INTO [RepoAnalyser].[dbo].[CyclomaticComplexity] (
				[GitHubRepositoryId]
				,[AnalysisResult]
                ,[LastUpdated]
				)
			VALUES (
				@RepoId
				,@Result
                ,@LastUpdated
				)";
	}
}
