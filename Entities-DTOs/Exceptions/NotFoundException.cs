namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

/// <summary>
/// Recurso inexistente → HTTP 404 (§5).
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
