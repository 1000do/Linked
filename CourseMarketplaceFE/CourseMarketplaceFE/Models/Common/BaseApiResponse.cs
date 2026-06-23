namespace CourseMarketplaceFE.Models.Common
{
    public class BaseApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class BaseApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
