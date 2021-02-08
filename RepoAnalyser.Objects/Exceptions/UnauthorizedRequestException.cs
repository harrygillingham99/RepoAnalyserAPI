using System;

namespace RepoAnalyser.Objects.Exceptions
{
    public class UnauthorizedRequestException : Exception
    {
        public UnauthorizedRequestException()
        {

        }

        public UnauthorizedRequestException(string message) : base(message)
        {

        }

        public UnauthorizedRequestException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
