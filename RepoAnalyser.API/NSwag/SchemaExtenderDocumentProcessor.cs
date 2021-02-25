using System;
using System.Linq;
using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{
    [ScrutorIgnore]
    public class SchemaExtenderDocumentProcessor : IDocumentProcessor
    {
        private readonly Type[] _typesToLoadAssembliesOf = new Type[] { typeof(AppSettings) };
        private const string _namespaceIdentifier = "RepoAnalyser.";

        public void Process(DocumentProcessorContext context)
        {
            //only load specific assemblies
            var assemblies = _typesToLoadAssembliesOf.Select(x => x.GetTypeInfo().Assembly);
            var types = assemblies.SelectMany(x => x.ExportedTypes).Where(type => type.FullName.StartsWith(_namespaceIdentifier) &&
                type.GetTypeInfo().CustomAttributes.Any(x => x.AttributeType == typeof(NSwagIncludeAttribute)));

            foreach (var type in types)
            {
                if (!context.SchemaResolver.HasSchema(type, false))
                {
                    context.SchemaGenerator.Generate(type, context.SchemaResolver);
                }
            }
        }
    }
}

