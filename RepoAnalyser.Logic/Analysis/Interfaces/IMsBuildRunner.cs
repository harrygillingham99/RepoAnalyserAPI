namespace RepoAnalyser.Logic.Analysis
{
    public interface IMsBuildRunner
    {
        string Build(string repoName, string outputDir = null);
    }
}