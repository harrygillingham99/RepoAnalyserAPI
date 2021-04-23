using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using RepoAnalyser.Objects;

namespace RepoAnalyser.Logic.Analysis
{
    public static class CecilHelper
    {
        public static TargetAssemblyDefinition ReadAssembly(string buildPath, string projectName)
        {
            return ReadAssemblies(new List<string> {buildPath}, projectName).FirstOrDefault();
        }

        public static List<TargetAssemblyDefinition> ReadAssemblies(List<string> buildPaths, string projectName)
        {
            if (!buildPaths.Any()) return new List<TargetAssemblyDefinition>();

            var assemblyDirs = buildPaths.SelectMany(Directory.GetFiles).Where(file =>
                file.Split('\\').Last().ToLower().Contains(projectName.ToLower()) && file.EndsWith(".dll"));

            return assemblyDirs.Select(assemblyPath =>
                new TargetAssemblyDefinition
                {
                    Assembly = AssemblyDefinition.ReadAssembly(assemblyPath),
                    LocalPath = assemblyPath
                }).ToList();
        }

        public static List<MethodDefinition> ScanForMethods(this List<TargetAssemblyDefinition> targetAssemblies,
            IEnumerable<string> methodSearchTerms)
        {
            var methods = new List<MethodDefinition>();
            var terms = methodSearchTerms?.ToList();
            foreach (var assembly in targetAssemblies.Select(x => x.Assembly))
                if (terms != null && terms.Count > 0)
                    foreach (var methodToSearch in terms)
                        methods.AddRange(assembly.MainModule.Types.Where(o => o.IsClass)
                            .SelectMany(type => type.Methods)
                            .Where(o => o.FullName.ToLower().Contains(methodToSearch.ToLower())).ToList());
                else
                    methods.AddRange(assembly.MainModule.Types.Where(o => o.IsClass)
                        .SelectMany(type => type.Methods)
                        .ToList());
            return methods;
        }

        public static List<MethodDefinition> ScanForMethods(this TargetAssemblyDefinition targetAssembly,
            IEnumerable<string> methodSearchTerms)
        {
            return ScanForMethods(new List<TargetAssemblyDefinition> {targetAssembly}, methodSearchTerms);
        }
    }
}