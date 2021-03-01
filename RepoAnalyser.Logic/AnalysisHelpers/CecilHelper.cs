using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using RepoAnalyser.Objects;

namespace RepoAnalyser.Logic.AnalysisHelpers
{
    public static class CecilHelper
    {
        public static List<TargetAssemblyDefinition> ReadAssemblies(List<string> pathToAssemblies)
        {
            if (!pathToAssemblies.Any()) return new List<TargetAssemblyDefinition>();

            return pathToAssemblies.Select(assemblyPath =>
                new TargetAssemblyDefinition
                {
                    Assembly = AssemblyDefinition.ReadAssembly(assemblyPath),
                    LocalPath = assemblyPath
                }).ToList();
        }

        public static List<MethodDefinition> ScanForMethods(this List<AssemblyDefinition> targetAssemblies,
            List<string> methodSearchTerms)
        {
            var methods = new List<MethodDefinition>();
            foreach (var assembly in targetAssemblies)
            {
                foreach (var methodToSearch in methodSearchTerms)
                {
                    methods.AddRange(assembly.MainModule.Types.Where(o => o.IsClass)
                        .SelectMany(type => type.Methods)
                        .Where(o => o.FullName.ToLower().Contains(methodToSearch.ToLower())).ToList());
                }
            }
            return methods;
        }
    }
}