namespace RepoAnalyser.SqlServer.DAL.SQL
{
    public static partial class Sql
    {
        public static string InsertAuditItemSql = @"
           INSERT INTO [dbo].[RequestAudit]
           ([Page]
           ,[Referrer]
           ,[BrowserName]
           ,[BrowserEngine]
           ,[BrowserLanguage]
           ,[CookiesEnabled]
           ,[RequestTime]
           ,[EndpointRequested]
           ,[DateCreated])
           VALUES
           (@Page
           ,@Referrer
           ,@BrowserName
           ,@BrowserEngine
           ,@BrowserLanguage
           ,@CookiesEnabled
           ,@RequestTime
           ,@EndpointRequested
           ,GETDATE())";
    }
}
