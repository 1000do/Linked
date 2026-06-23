using System;

namespace CourseMarketplaceBE.Domain.Exceptions;

public class SystemConfigException : Exception
{
    public SystemConfigException(string message) : base(message)
    {
    }

    public SystemConfigException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SystemConfigException()
    {
    }
}
