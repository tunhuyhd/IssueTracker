namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Validation cho image files
/// </summary>
public class ImageFileType : BaseFileType
{
    public override FileType Type => FileType.Image;

    public override string Folder => "images";

    public override string[] AllowedExtensions => new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".ico"
    };

    public override long MaxSizeInBytes => 10 * 1024 * 1024; // 10 MB

    public override byte[][] MagicBytes => new[]
    {
        // JPEG
        new byte[] { 0xFF, 0xD8, 0xFF },
        // PNG
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        // GIF
        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
        new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 },
        // BMP
        new byte[] { 0x42, 0x4D },
        // WEBP
        new byte[] { 0x52, 0x49, 0x46, 0x46 },
        // ICO
        new byte[] { 0x00, 0x00, 0x01, 0x00 },
        // SVG (XML-based, harder to detect)
        new byte[] { 0x3C, 0x73, 0x76, 0x67 } // "<svg"
    };
}
