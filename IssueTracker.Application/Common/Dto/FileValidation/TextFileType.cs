namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Validation cho text files
/// </summary>
public class TextFileType : BaseFileType
{
    public override FileType Type => FileType.Text;

    public override string Folder => "texts";

    public override string[] AllowedExtensions => new[]
    {
        ".txt", ".md", ".log", ".json", ".xml", ".csv"
    };

    public override long MaxSizeInBytes => 5 * 1024 * 1024; // 5 MB

    public override byte[][] MagicBytes => new[]
    {
        // UTF-8 BOM
        new byte[] { 0xEF, 0xBB, 0xBF },
        // UTF-16 LE BOM
        new byte[] { 0xFF, 0xFE },
        // UTF-16 BE BOM
        new byte[] { 0xFE, 0xFF }
    };

    public override bool ValidateMagicBytes(byte[] fileBytes)
    {
        // Text files may not have magic bytes, so check if it's valid UTF-8/ASCII
        if (fileBytes == null || fileBytes.Length == 0)
            return false;

        // Check BOM first
        if (base.ValidateMagicBytes(fileBytes))
            return true;

        // If no BOM, assume it's valid text file (most text files don't have magic bytes)
        return true;
    }
}
