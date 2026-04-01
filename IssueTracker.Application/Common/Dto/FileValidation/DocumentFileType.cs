namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Validation cho document files
/// </summary>
public class DocumentFileType : BaseFileType
{
    public override FileType Type => FileType.Document;

    public override string Folder => "documents";

    public override string[] AllowedExtensions => new[]
    {
        ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt"
    };

    public override long MaxSizeInBytes => 50 * 1024 * 1024; // 50 MB

    public override byte[][] MagicBytes => new[]
    {
        // PDF
        new byte[] { 0x25, 0x50, 0x44, 0x46 },
        // DOC (MS Word)
        new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
        // DOCX (ZIP-based)
        new byte[] { 0x50, 0x4B, 0x03, 0x04 },
        // RTF
        new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66 }
    };
}
