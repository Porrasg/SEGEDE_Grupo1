namespace SEGEDE_Grupo1.EntitiesDTOs;

// TODO: Implementar propiedades base (Id, Created, Updated) segÃºn documento tÃ©cnico Â§3.1.
public abstract class BaseDTO
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
