using System;

namespace RepoAnalyser.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequireConnectionIdAttribute : Attribute
    {
    }
}
