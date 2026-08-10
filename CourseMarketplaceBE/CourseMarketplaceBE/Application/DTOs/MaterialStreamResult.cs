using System.IO;

namespace CourseMarketplaceBE.Application.DTOs;

public class MaterialStreamResult
{
    public Stream Stream { get; set; } = Stream.Null;
    public string ContentType { get; set; } = "application/octet-stream";
    public long? ContentLength { get; set; }
    public string? ContentRangeHeader { get; set; }
    public int StatusCode { get; set; } = 200;
    public string? FileName { get; set; }
}
