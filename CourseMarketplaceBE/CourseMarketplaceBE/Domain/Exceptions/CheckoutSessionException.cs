using System;

namespace CourseMarketplaceBE.Domain.Exceptions;

public class CheckoutSessionException : Exception
{
    public CheckoutSessionException(string message) : base(message)
    {
    }

    public CheckoutSessionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
