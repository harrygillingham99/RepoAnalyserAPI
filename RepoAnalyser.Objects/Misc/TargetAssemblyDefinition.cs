using Mono.Cecil;

namespace RepoAnalyser.Objects.Misc
{
    public class TargetAssemblyDefinition
    {
        public AssemblyDefinition Assembly { get; set; }
        public string LocalPath { get; set; }
    }

}
