namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// Recurso inexistente → HTTP 404 (§5).
public class NotFoundException : Exception
{
    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public NotFoundException(string message) : base(message) { }
}
