using RepoAnalyser.Objects.API.Requests;

namespace RepoAnalyser.Objects.Misc
{
    #nullable enable
    public class RequestAudit
    {
        public RequestAudit(ClientMetadata? metadata, long executionTime, string requestedEndpoint)
        {
            Metadata = metadata;
            ExecutionTime = executionTime;
            RequestedEndpoint = requestedEndpoint;
        }

        public ClientMetadata? Metadata { get; set; }
        public long ExecutionTime { get; set; } 
        public string RequestedEndpoint { get; set; }
    }
    #nullable disable
}
