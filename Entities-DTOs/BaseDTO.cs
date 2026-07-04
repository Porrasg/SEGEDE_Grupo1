namespace SEGEDE_Grupo1.EntitiesDTOs;

/// <summary>
/// Clase base para todas las entidades del sistema.
/// Propiedades comunes: Id, Created, Updated (§3.1).
/// </summary>
public class BaseDTO
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
