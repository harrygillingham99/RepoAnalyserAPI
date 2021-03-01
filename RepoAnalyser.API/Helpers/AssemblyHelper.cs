using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace RepoAnalyser.API.Helpers
{
    public static class AssemblyHelper
    {
        public static void LoadAssembliesWithDependencies(bool includeFramework = false, string[] specificAssemblies = null)
        {
            // Storage to ensure not loading the same assembly twice and optimize calls to GetAssemblies()
            ConcurrentDictionary<string, bool> loadedAssemblies = new ConcurrentDictionary<string, bool>();

            // Filter to avoid loading all the .net framework
            bool ShouldLoad(string assemblyName)
            {
                return (includeFramework || (NotFrameworkAssembly(assemblyName) && ShouldLoadAssembly(assemblyName)))
                       && !loadedAssemblies.ContainsKey(assemblyName);
            }

            bool NotFrameworkAssembly(string assemblyName)
            {
                return !assemblyName.StartsWith("Microsoft.")
                       && !assemblyName.StartsWith("System.")
                       && !assemblyName.StartsWith("Newtonsoft.")
                       && assemblyName != "netstandard";
            }

            bool ShouldLoadAssembly(string assemblyName)
            {
                if (specificAssemblies == null) return true;
                var results = specificAssemblies.Select(assemblyName.StartsWith).ToList();

                return results.Any(x => x);
            }

            void LoadReferencedAssembly(Assembly assembly)
            {
                // Check all referenced assemblies of the specified assembly
                foreach (AssemblyName an in assembly.GetReferencedAssemblies().Where(a => ShouldLoad(a.FullName)))
                {
                    // Load the assembly and load its dependencies
                    LoadReferencedAssembly(Assembly.Load(an)); 
                    loadedAssemblies.TryAdd(an.FullName, true);
                }
            }

            // Populate already loaded assemblies
            System.Diagnostics.Debug.WriteLine($">> Already loaded assemblies:");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => ShouldLoad(a.FullName)))
            {
                loadedAssemblies.TryAdd(assembly.FullName, true);
            }

            // Loop on loaded assembliesto load dependencies (it includes Startup assembly so should load all the dependency tree) 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => NotFrameworkAssembly(a.FullName)))
                LoadReferencedAssembly(assembly);
        }
    }
}
