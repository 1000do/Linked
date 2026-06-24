using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class TextEmbeddingException : Exception
    {
        public TextEmbeddingException() { }

        public TextEmbeddingException(string message) : base(message) { }

        public TextEmbeddingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
