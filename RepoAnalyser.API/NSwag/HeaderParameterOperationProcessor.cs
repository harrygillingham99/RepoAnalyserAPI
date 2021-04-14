using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{
    /*
     * NSwag Operation Post-Processor to add a ClientMetadata header param to every endpoint
     * for detailed request logging, also adds connection Id header param for methods with [RequireConnectionId]
     */
    [ScrutorIgnore]
    public class HeaderParameterOperationProcessor : IOperationProcessor
    {
        private const string ClientMetadataKey = "Metadata";
        private const string ConnectionIdKey = "ConnectionId";
        public bool Process(OperationProcessorContext context)
        {
            if (context.MethodInfo.CustomAttributes.Any(attr => attr.AttributeType == typeof(RequireConnectionIdAttribute)))
            {
                context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
                {
                    Name = ConnectionIdKey,
                    Kind = OpenApiParameterKind.Header,
                    Type = JsonObjectType.String,
                    IsRequired = true,
                    Description = "SignalR Client Connection ID",
                    Default = string.Empty
                });
            }

            if (context.OperationDescription.Operation.Parameters.All(param => param.Name != ClientMetadataKey))
            {
                context.OperationDescription.Operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = ClientMetadataKey,
                        Kind = OpenApiParameterKind.Header,
                        Type = JsonObjectType.Object,
                        IsRequired = false,
                        Description = "Client Metadata JSON",
                        Default = new ClientMetadata()
                    });
            }

            return true;
        }
    }
}