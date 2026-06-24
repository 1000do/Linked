using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class CourseAiIntegrationException : Exception
    {
        public CourseAiIntegrationException() { }

        public CourseAiIntegrationException(string message) : base(message) { }

        public CourseAiIntegrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
