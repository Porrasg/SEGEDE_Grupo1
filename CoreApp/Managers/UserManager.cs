using SEGEDE_Grupo1.DataAccess.CRUD;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// TODO: Manager de usuarios segÃºn documento tÃ©cnico Â§14.1. InstanciaciÃ³n directa sin IoC.
public class UserManager
{
    private readonly UserCrudFactory _userCrudFactory = new();
}
