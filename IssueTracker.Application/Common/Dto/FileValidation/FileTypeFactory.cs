namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Factory để tạo instance của BaseFileType dựa trên magic bytes
/// </summary>
public static class FileTypeFactory
{
    private static readonly BaseFileType[] SupportedFileTypes = new BaseFileType[]
    {
        new ImageFileType(),
        new DocumentFileType(),
        new SpreadsheetFileType(),
        new TextFileType()
    };

    /// <summary>
    /// Detect file type từ magic bytes
    /// </summary>
    public static BaseFileType CreateFileTypeInstance(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            throw new ArgumentException("File bytes cannot be null or empty");

        // Try to detect file type by magic bytes
        foreach (var fileType in SupportedFileTypes)
        {
            if (fileType.ValidateMagicBytes(fileBytes))
            {
                return fileType;
            }
        }

        // If no magic bytes match, default to text file type
        // (many text files don't have distinctive magic bytes)
        return new TextFileType();
    }

    /// <summary>
    /// Get file type instance by FileType enum
    /// </summary>
    public static BaseFileType GetFileTypeInstance(FileType type)
    {
        return type switch
        {
            FileType.Image => new ImageFileType(),
            FileType.Document => new DocumentFileType(),
            FileType.Spreadsheet => new SpreadsheetFileType(),
            FileType.Text => new TextFileType(),
            _ => throw new ArgumentException($"Unsupported file type: {type}")
        };
    }
}
