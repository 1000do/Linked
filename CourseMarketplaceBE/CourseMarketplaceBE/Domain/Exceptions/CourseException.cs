using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class CourseException : Exception
    {
        public CourseException(string message) : base(message)
        {
        }
        
        public CourseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
