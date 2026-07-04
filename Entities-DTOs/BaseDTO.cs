namespace SEGEDE_Grupo1.EntitiesDTOs;

// Clase base para todas las entidades del sistema.
// Propiedades comunes: Id, Created, Updated (§3.1).
public class BaseDTO
{
    // Identificador único primario (PK) en la tabla relacional.
    public int Id { get; set; }
    // Marca de tiempo (UTC/Local) en que se creó el registro de auditoría.
    public DateTime Created { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime Updated { get; set; }
}
