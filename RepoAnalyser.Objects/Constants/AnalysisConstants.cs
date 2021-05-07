using RepoAnalyser.Objects.API.Responses;

namespace RepoAnalyser.Objects.Constants
{
    public static class AnalysisConstants
    {
        public static string DefaultBuildPath = "RepoAnalyserBuild";

        public static string FallbackEmail(string user = null) => $"{user ?? "unknown"}@RepoAnalyser.com";

        public static string NoReportText = "No Report";



    }
}
