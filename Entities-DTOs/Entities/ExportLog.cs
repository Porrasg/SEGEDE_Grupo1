namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Log de exportaciones → tblExportLog (§9.24). WORM.
public class ExportLog : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string DocumentType { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int DocumentId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Format { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string CloneFilePath { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EventDate { get; set; }
}
