using System;

namespace RepoAnalyser.Objects.Attributes
{
    //Used to flag classes that have had their dependency injection handled outside of Scrutor.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScrutorIgnoreAttribute : Attribute
    {
    }
}
