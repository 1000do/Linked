using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class ReportException : Exception
    {
        public ReportException(string message) : base(message) { }
        public ReportException(string message, Exception innerException) : base(message, innerException) { }
    }
}
