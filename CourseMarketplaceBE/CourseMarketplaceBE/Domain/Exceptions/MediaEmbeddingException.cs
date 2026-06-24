using System;

namespace CourseMarketplaceBE.Domain.Exceptions
{
    public class MediaEmbeddingException : Exception
    {
        public MediaEmbeddingException() { }

        public MediaEmbeddingException(string message) : base(message) { }

        public MediaEmbeddingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
