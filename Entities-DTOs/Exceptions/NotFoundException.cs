namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Recurso inexistente → HTTP 404 (§5).
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
