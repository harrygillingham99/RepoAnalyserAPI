using System;
using System.Linq;
using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{
    /*
     * NSwag Schema Post-Processor to add any classes with the [NSwagInclude] attribute
     * to the schema
     */
    [ScrutorIgnore]
    public class SchemaExtenderDocumentProcessor : IDocumentProcessor
    {
        private const string NamespaceIdentifier = "RepoAnalyser.";
        private readonly Type[] _typesToLoadAssembliesOf = {typeof(AppSettings)};

        public void Process(DocumentProcessorContext context)
        {
            //Only load specific assemblies
            var assemblies = _typesToLoadAssembliesOf.Select(x => x.GetTypeInfo().Assembly);

            //Merge the lists of assemblies and check for any types with the [NSwagInclude] attribute
            var types = assemblies.SelectMany(x => x.ExportedTypes).Where(type =>
                !string.IsNullOrWhiteSpace(type.FullName) && type.FullName.StartsWith(NamespaceIdentifier) &&
                type.GetTypeInfo().CustomAttributes.Any(x => x.AttributeType == typeof(NSwagIncludeAttribute)));

            //Add the types to the schema
            foreach (var type in types)
                if (!context.SchemaResolver.HasSchema(type, type.IsEnum))
                    context.SchemaGenerator.Generate(type, context.SchemaResolver);
        }
    }
}