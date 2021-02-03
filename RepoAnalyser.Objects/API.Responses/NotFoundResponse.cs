using System.Collections.Generic;

namespace RepoAnalyser.Objects.API.Responses
{
    public class NotFoundResponse : BaseResponse
    {
        public Dictionary<string,string> BadProperties { get; set; }
    }
}
