using SEGEDE_Grupo1.DataAccess.DAO;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.DataAccess.CRUD;

// Clase base abstracta para todas las CrudFactories (§11.3).
// Patrón: constructor instancia SqlDao; cada factory implementa CRUD + métodos custom.
// Factories WORM: Update()/Delete() lanzan NotSupportedException.
public abstract class CrudFactory
{
    protected SqlDao sqlDao;

    protected CrudFactory() => sqlDao = SqlDao.GetInstance();

    public abstract void Create(BaseDTO baseDTO);
    public abstract void Update(BaseDTO baseDTO);
    public abstract void Delete(BaseDTO baseDTO);
    public abstract T RetrieveById<T>(int id);
    public abstract List<T> RetrieveAll<T>();
}
