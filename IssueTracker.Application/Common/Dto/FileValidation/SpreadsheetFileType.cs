namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Validation cho spreadsheet files
/// </summary>
public class SpreadsheetFileType : BaseFileType
{
    public override FileType Type => FileType.Spreadsheet;

    public override string Folder => "spreadsheets";

    public override string[] AllowedExtensions => new[]
    {
        ".xls", ".xlsx", ".csv", ".ods"
    };

    public override long MaxSizeInBytes => 20 * 1024 * 1024; // 20 MB

    public override byte[][] MagicBytes => new[]
    {
        // XLS (MS Excel)
        new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
        // XLSX (ZIP-based)
        new byte[] { 0x50, 0x4B, 0x03, 0x04 }
    };
}
