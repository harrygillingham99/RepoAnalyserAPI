namespace RepoAnalyser.Logic.Analysis.Interfaces
{
    public interface IMsBuildRunner
    {
        string Build(string repoName, string outputDir = null);
    }
}