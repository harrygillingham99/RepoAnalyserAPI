namespace RepoAnalyser.Logic.Analysis.Interfaces
{
    public interface IMsBuildRunner
    { 
        string Build(string repoDirectory, string outputDir);
    }
}