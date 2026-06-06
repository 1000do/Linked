namespace CourseMarketplaceBE.Domain.Exceptions;

public class CourseExtException : Exception
{
    public CourseExtException(string message) : base(message)
    {
    }
    public CourseExtException(){}
}
