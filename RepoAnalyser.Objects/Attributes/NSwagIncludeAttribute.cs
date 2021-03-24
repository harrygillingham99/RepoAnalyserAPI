using System;

namespace RepoAnalyser.Objects.Attributes
{
    //Used for objects that are not referenced in API output but need to be in the schema.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class NSwagIncludeAttribute : Attribute
    {
    }
}
