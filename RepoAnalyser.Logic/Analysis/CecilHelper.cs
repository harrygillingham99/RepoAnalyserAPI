using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using RepoAnalyser.Objects;

namespace RepoAnalyser.Logic.Analysis
{
    public static class CecilHelper
    {
        public static TargetAssemblyDefinition ReadAssembly(string pathToAssembly) =>
            ReadAssemblies(new List<string> {pathToAssembly}).FirstOrDefault();

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

        public static List<MethodDefinition> ScanForMethods(this List<TargetAssemblyDefinition> targetAssemblies,
            List<string> methodSearchTerms)
        {
            var methods = new List<MethodDefinition>();
            foreach (var assembly in targetAssemblies.Select(x => x.Assembly))
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

        public static List<MethodDefinition> ScanForMethods(this TargetAssemblyDefinition targetAssembly, List<string> methodSearchTerms) =>
            ScanForMethods(new List<TargetAssemblyDefinition> {targetAssembly}, methodSearchTerms);
    }
}