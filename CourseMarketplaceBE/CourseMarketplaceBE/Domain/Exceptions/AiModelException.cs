using System;

namespace CourseMarketplaceBE.Domain.Exceptions;

public class AiModelException : Exception
{
    public AiModelException(string message) : base(message)
    {
    }

    public AiModelException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AiModelException()
    {
    }
}
