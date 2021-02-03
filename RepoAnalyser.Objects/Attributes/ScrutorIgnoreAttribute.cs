using System;

namespace RepoAnalyser.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScrutorIgnoreAttribute : Attribute
    {
    }
}
