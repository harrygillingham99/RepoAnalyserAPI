using System;
using System.Linq;
using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RepoAnalyser.Objects.Attributes;

namespace RepoAnalyser.API.NSwag
{ 
    [ScrutorIgnore]
    public class SchemaExtenderDocumentProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            var assembliesToInclude = Assembly
                .GetEntryAssembly()
                ?.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(x => x.DefinedTypes)
                .Where(type => type.GetCustomAttributes().Any(attr => attr.GetType() == typeof(NSwagIncludeAttribute)));

            if (assembliesToInclude != null)
            {
                foreach (var type in assembliesToInclude)
                {
                    if (!context.SchemaResolver.HasSchema(type, type.IsEnum))
                    {
                        context.SchemaGenerator.Generate(type, context.SchemaResolver);
                    }

                }
            }
        }
    }
}
