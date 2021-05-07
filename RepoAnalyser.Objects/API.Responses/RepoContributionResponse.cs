using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class RepoContributionResponse
    {
        public IDictionary<string, AddedRemoved> LocForFiles { get; set; }
    }
}
