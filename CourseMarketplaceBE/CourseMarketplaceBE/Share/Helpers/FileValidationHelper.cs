using System.IO;

namespace CourseMarketplaceBE.Share.Helpers
{
    public static class FileValidationHelper
    {
        private static readonly Dictionary<string, List<byte[]>> AllowedFileSignatures = new()
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".mp4", new List<byte[]> { 
                // typically ends with 66 74 79 70 ("ftyp") starting at byte 4
                new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 },
                new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 },
                new byte[] { 0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70 }
            } }
        };

        private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".bat", ".cmd", ".sh", ".dll", ".msi", ".vbs", ".ps1"
        };

        private static readonly HashSet<string> AllowedExtensionsWithoutSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".gif", ".bmp", ".tiff", ".webp",
            ".avi", ".mov", ".mkv", ".webm", ".flv", ".wmv", ".3gp",
            ".docx", ".doc", ".odt", ".rtf",
            ".pptx", ".ppt",
            ".xlsx", ".xls", ".zip"
        };

        public static bool IsValidFile(Stream fileStream, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (ExecutableExtensions.Contains(ext)) return false;

            if (AllowedFileSignatures.TryGetValue(ext, out var signatures))
            {
                using var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8, leaveOpen: true);
                
                // Read the maximum bytes we need to check
                var maxSigLength = signatures.Max(s => s.Length);
                var headerBytes = reader.ReadBytes(maxSigLength);
                fileStream.Position = 0; // Reset position for further reading

                return signatures.Any(sig => 
                    headerBytes.Length >= sig.Length && 
                    headerBytes.Take(sig.Length).SequenceEqual(sig));
            }

            // Fallback for extensions we don't have signatures for but are considered safe
            // In a production system, we'd add all required signatures.
            return AllowedExtensionsWithoutSignatures.Contains(ext);
        }
    }
}
