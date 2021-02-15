using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{
    [ScrutorIgnore]
    public class HeaderParameterOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            context.OperationDescription.Operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "Metadata",
                    Kind = OpenApiParameterKind.Header,
                    Type = JsonObjectType.Object,
                    IsRequired = false,
                    Description = "ClientMetadata",
                    Default = null
                });

            return true;
        }
    }
}