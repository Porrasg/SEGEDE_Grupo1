namespace SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

// TODO: ExcepciÃ³n de negocio base segÃºn documento tÃ©cnico Â§5.
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
