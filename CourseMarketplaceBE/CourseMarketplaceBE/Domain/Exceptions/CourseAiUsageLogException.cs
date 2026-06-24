using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class CourseAiUsageLogException : Exception
    {
        public CourseAiUsageLogException() { }

        public CourseAiUsageLogException(string message) : base(message) { }

        public CourseAiUsageLogException(string message, Exception innerException) : base(message, innerException) { }
    }
}
