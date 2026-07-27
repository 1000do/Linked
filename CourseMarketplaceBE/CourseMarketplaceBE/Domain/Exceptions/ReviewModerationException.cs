using System;

namespace CourseMarketplaceBE.Domain.Exceptions;

public class ReviewModerationException : Exception
{
    public ReviewModerationException(string message) : base(message)
    {
    }

    public ReviewModerationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
