namespace RepoAnalyser.SqlServer.DAL.SQL
{
    public static partial class Sql
    {
        public static string TruncateRequestAudit = @"  
        TRUNCATE TABLE RequestAudit
        DBCC CHECKIDENT ('[RequestAudit]', RESEED, 0)";
    }
}
