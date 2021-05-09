using System;

namespace RepoAnalyser.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RequireConnectionIdAttribute : Attribute
    {
    }
}
