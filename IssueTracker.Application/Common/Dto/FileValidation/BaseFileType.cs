namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Base class cho file type validation
/// </summary>
public abstract class BaseFileType
{
    /// <summary>
    /// Loại file (Image, Document, etc.)
    /// </summary>
    public abstract FileType Type { get; }

    /// <summary>
    /// Tên thư mục lưu trữ
    /// </summary>
    public abstract string Folder { get; }

    /// <summary>
    /// Danh sách extension được phép
    /// </summary>
    public abstract string[] AllowedExtensions { get; }

    /// <summary>
    /// Kích thước tối đa (bytes)
    /// </summary>
    public abstract long MaxSizeInBytes { get; }

    /// <summary>
    /// Magic bytes để detect file type
    /// </summary>
    public abstract byte[][] MagicBytes { get; }

    /// <summary>
    /// Validate file bằng magic bytes
    /// </summary>
    public virtual bool ValidateMagicBytes(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return false;

        foreach (var magic in MagicBytes)
        {
            if (fileBytes.Length >= magic.Length)
            {
                var matches = true;
                for (int i = 0; i < magic.Length; i++)
                {
                    if (fileBytes[i] != magic[i])
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches)
                    return true;
            }
        }
        return false;
    }
}
