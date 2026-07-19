namespace SEGEDE_Grupo1.CoreApp.Exceptions;

// Excepción lanzada cuando un recurso solicitado no existe → HTTP 404.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
