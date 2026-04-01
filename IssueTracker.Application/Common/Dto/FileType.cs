namespace IssueTracker.Application.Common.Dto;

/// <summary>
/// Enum định nghĩa các loại file được phép upload
/// </summary>
public enum FileType
{
    /// <summary>
    /// File ảnh (jpg, jpeg, png, gif, webp)
    /// </summary>
    Image,

    /// <summary>
    /// File document (pdf, doc, docx)
    /// </summary>
    Document,

    /// <summary>
    /// File excel (xls, xlsx, csv)
    /// </summary>
    Spreadsheet,

    /// <summary>
    /// File text (txt, md)
    /// </summary>
    Text,

    /// <summary>
    /// File video (mp4, avi, mov)
    /// </summary>
    Video,

    /// <summary>
    /// File audio (mp3, wav, ogg)
    /// </summary>
    Audio,

    /// <summary>
    /// Tất cả các loại file
    /// </summary>
    All
}
