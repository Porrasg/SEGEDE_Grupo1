namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

/// <summary>
/// Log de exportaciones → tblExportLog (§9.24). WORM.
/// </summary>
public class ExportLog : BaseDTO
{
    public int UserId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string Format { get; set; } = string.Empty;
    public string CloneFilePath { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
}
