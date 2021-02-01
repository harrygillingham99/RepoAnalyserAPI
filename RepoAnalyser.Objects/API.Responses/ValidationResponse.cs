using System.Collections.Generic;
using RepoAnalyser.Objects.Responses;

namespace RepoAnalyser.Objects.API.Responses
{
    public class ValidationResponse : Response
    {
        public Dictionary<string, string> ValidationErrors { get; set; }
    }
}