using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects.API.Requests;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{
    [ScrutorIgnore]
    public class HeaderParameterOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
            {
                context.OperationDescription.Operation.Parameters.Add(
                    new OpenApiParameter()
                    {
                        Name = "Metadata",
                        Kind = OpenApiParameterKind.Header,
                        Type = NJsonSchema.JsonObjectType.Object,
                        IsRequired = false,
                        Description = "ClientMetadata",
                        Default = new ClientMetadata()
                    });

                return true;
            }
        }

}
