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
     * for detailed request logging.
     */
    [ScrutorIgnore]
    public class HeaderParameterOperationProcessor : IOperationProcessor
    {
        private const string ClientMetadataKey = "Metadata";
        public bool Process(OperationProcessorContext context)
        {
            if (context.OperationDescription.Operation.Parameters.All(param => param.Name != ClientMetadataKey))
            {
                context.OperationDescription.Operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = ClientMetadataKey,
                        Kind = OpenApiParameterKind.Header,
                        Type = JsonObjectType.Object,
                        IsRequired = false,
                        Description = "ClientMetadata",
                        Default = new ClientMetadata()
                    });
            }

            return true;
        }
    }
}