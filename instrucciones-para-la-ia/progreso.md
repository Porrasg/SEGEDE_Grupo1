# Registro y Seguimiento de Progreso

> ⚠️ **INSTRUCCIÓN OBLIGATORIA PARA LA IA**:
> **Antes de iniciar, modificar código o continuar trabajando en el proyecto**, la IA debe revisar detenidamente este archivo y `implementation-plan.md` para entender qué tareas se han completado, en qué estado se encuentra el desarrollo y cuál es el siguiente paso prioritario. Cada vez que la IA complete una tarea significativa, debe actualizar este archivo reflejando el nuevo estado del proyecto.

---

## 📌 Estado Actual del Proyecto
- **Fase**: Fase 1 — Estructuración y Configuración Base (Esqueleto inicial completado).
- **Estado General**: Se ha construido el esqueleto arquitectónico en las 5 capas del sistema (`Entities-DTOs`, `DataAccess`, `CoreApp`, `WebAPI` y `WebApp`) basándose rigurosamente en el documento técnico oficial `SGDE_TechDesign_Complete.md`. Todas las referencias entre proyectos están conectadas en los `.csproj` y las clases principales cuentan con comentarios `TODO` sin lógica funcional compleja.

---

## ✅ Tareas Completadas
- [x] Creación de la solución `SEGEDE_Grupo1.slnx` con los proyectos base: `Entities-DTOs`, `DataAccess`, `CoreApp`, `WebAPI` y `WebApp`.
- [x] Configuración del archivo `.gitignore` y de la carpeta de control local `instrucciones-para-la-ia/`.
- [x] Conexión jerárquica de proyectos en archivos `.csproj` (`DataAccess` -> `Entities-DTOs`; `CoreApp` -> `Entities-DTOs`, `DataAccess`; `WebAPI` -> `Entities-DTOs`, `CoreApp`).
- [x] Limpieza y preparación de archivos generados por plantillas (`Class1.cs` y `WeatherForecast.cs` marcados como placeholders para eliminación).
- [x] Creación de carpetas y archivos esqueleto en **Capa 0 (`Entities-DTOs`)**: `BaseDTO.cs`, `Constants/` (`SystemActor`, `UserRoles`, `UserStates`), `Entities/` (`User`, `Turbine`, `Maintenance`), `DTOs/` (`ApiResponse`, `PagedRequest`, `PagedResponse`), `Exceptions/`, `Validation/` y `Helpers/`.
- [x] Creación de carpetas y archivos esqueleto en **Capa 1 (`DataAccess`)**: `DAO/` (`SqlDao`, `Operation`), `CRUD/` (`CrudFactory` abstracta, `UserCrudFactory`, `TurbineCrudFactory`, `MaintenanceCrudFactory`).
- [x] Creación de carpetas y archivos esqueleto en **Capa 2 (`CoreApp`)**: `Helpers/` (`JwtHelper`, `PasswordHasher`), `External/` (`OtpServiceClient`), `Export/` (`CsvBuilder`, `ExcelBuilder`, `HtmlStatementBuilder`), `Managers/` (`UserManager`, `TurbineManager`, `DashboardManager` instanciando factories con `new()`).
- [x] Creación de carpetas y archivos esqueleto en **Capa 3 (`WebAPI`)**: `Controllers/` (`UsersController`, `TurbinesController`, `DashboardController`), `Middleware/` (`ExceptionHandlingMiddleware`), `BackgroundServices/` (`JobBase`, `EnergySimulationJob`).
- [x] Creación de carpetas y archivos esqueleto en **Capa 4 (`WebApp`)**: Scripts estáticos en `wwwroot/js/` (`apiClient.js`, `session.js`) y `pages-controller/` (`LoginViewController.js`, `AdminDashboardViewController.js`), así como Razor Pages esquemáticas: `Pages/Login`, `Pages/Admin/Dashboard`, `Pages/Engineer/Dashboard` y `Pages/Buyer/Dashboard`.

---

## 🚧 En Progreso
- [ ] Implementación manual paso a paso de la persistencia (Stored Procedures SQL en Azure SQL Database y métodos de acceso en `SqlDao`).

---

## 📋 Tareas Pendientes y Siguiente Paso
### 🎯 Siguiente Paso Recomendado:
1. **Fase 2 - Base de Datos e Infraestructura SQL (`DataAccess`)**:
   - Crear el script DDL de las tablas SQL en Azure SQL (`tblUsers`, `tblTurbines`, `tblMaintenances`, etc.) y sus Stored Procedures asociados según el catálogo del documento técnico.
   - Implementar la lógica real de conexión a base de datos en `SqlDao.cs` usando `Microsoft.Data.SqlClient`.
2. **Fase 2 - Completar Entidades y DTOs (`Entities-DTOs`)**:
   - Agregar las 21 entidades restantes del catálogo (§9) y los DTOs concretos de Request/Response (§8).
3. **Fase 3 - Lógica de Negocio (`CoreApp`)**:
   - Implementar la validación y reglas de negocio en los Managers (ej. BCrypt en `UserManager`, transacciones en `TurbineManager`).

---

## 📝 Notas para el Desarrollador y la IA
- Se respetó estrictamente la restricción del documento de no utilizar ORMs (Entity Framework está prohibido) y de no utilizar contenedores de inyección de dependencias (IoC) para la instanciación de fábricas y gestores (`new XxxCrudFactory()`).
- Todo el código base esqueleto está redactado con nombres en inglés y explicaciones/TODOs en español.
