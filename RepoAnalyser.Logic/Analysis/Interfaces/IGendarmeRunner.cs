using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.Logic.Analysis.Interfaces
{
    public interface IGendarmeRunner
    {
        (string reportFileDir, string htmlResult) Run(GendarmeAnalyisRequest request);
    }
}