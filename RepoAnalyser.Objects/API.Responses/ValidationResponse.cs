using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class ValidationResponse : BaseResponse
    {
        public Dictionary<string, string> ValidationErrors { get; set; }
    }
}