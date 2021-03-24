using System;
using System.Collections.Generic;
using System.Text;

namespace RepoAnalyser.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequireConnectionIdAttribute : Attribute
    {
    }
}
