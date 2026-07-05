# Plan General de Implementación - SEGEDE_Grupo1

Este documento describe los objetivos, la arquitectura técnica, las fases de desarrollo y las tareas pendientes para la implementación del proyecto **SEGEDE_Grupo1**.

---

## 1. Objetivos del Proyecto
- **Desarrollar una solución modular, escalable y mantenible** bajo una arquitectura en capas utilizando **.NET / C#**.
- **Separar claramente las responsabilidades** entre el modelo de dominio, el acceso a datos, la lógica de negocio y las interfaces de usuario (API REST y Frontend Web).
- **Facilitar el trabajo colaborativo** y la asistencia automatizada de IA manteniendo estándares de código limpios y documentados.

---

## 2. Decisiones Técnicas y Arquitectura
El proyecto sigue un enfoque de **Arquitectura en Capas (N-Tier Architecture / Clean Architecture)** distribuida en los siguientes proyectos:

1. **`Entities-DTOs`**: 
   - Define el modelo de dominio (entidades puras que representan tablas de base de datos) y los DTOs (*Data Transfer Objects*) utilizados para enviar y recibir datos sin exponer las entidades internas.
2. **`DataAccess`**: 
   - Encargada de la persistencia de datos y comunicación con el motor de base de datos (mediante ORM como Entity Framework Core o Dapper, implementación de patrón Repositorio / Unit of Work).
3. **`CoreApp`**: 
   - Contiene la lógica de negocio, reglas de validación, servicios de aplicación y la coordinación entre repositorios y presentadores.
4. **`WebAPI`**: 
   - Capa de presentación para servicios RESTful. Contiene los controladores (`Controllers`), autenticación/autorización (JWT/OAuth), y serialización de datos para integraciones externas o clientes front-end.
5. **`WebApp`**: 
   - Interfaz de usuario final orientada a web (ASP.NET Core / Razor Pages / Blazor / MVC), que consume los servicios o lógica de la aplicación para interactuar con los usuarios.

---

## 3. Pasos Principales de Implementación

### Fase 1: Estructuración y Configuración Base (Actual)
- [x] Creación de la solución `.slnx` y proyectos en .NET.
- [x] Configuración de directrices e ignorados de Git (`.gitignore`).
- [x] Definición del entorno de seguimiento para la IA (`instrucciones-para-la-ia/`).

### Fase 2: Modelado del Dominio y Acceso a Datos (Completada)
- [x] Definir las entidades de dominio en `Entities-DTOs`.
- [x] Crear los DTOs de entrada/salida para cada caso de uso principal.
- [x] Configurar el contexto de base de datos / acceso SQL genérico en `DataAccess`.
- [x] Implementar repositorios e interfaces de acceso a datos (`CrudFactories`).

### Fase 3: Lógica de Negocio (`CoreApp`) (Completada)
- [x] Implementar interfaces y servicios de negocio (`Managers`).
- [x] Agregar validaciones de datos y reglas de negocio en los servicios.
- [x] Configurar mapeos entre Entidades y DTOs y exportaciones (CSV, Excel, HTML).

### Fase 4: Exposición de Servicios (`WebAPI`) (Completada)
- [x] Crear los 13 controladores REST en `Controllers/`.
- [x] Configurar servicios e inyección de dependencias en `Program.cs`.
- [x] Configurar manejo global de excepciones (`ExceptionHandlingMiddleware`) y Background Jobs.

### Fase 5: Interfaz de Usuario (`WebApp`) (Completada)
- [x] Estructurar las vistas / páginas en `Pages/` e interfaz de usuario.
- [x] Implementar infraestructura JavaScript compartida (`apiClient.js`, `session.js`, `notifications.js`, `theme.js`).
- [x] Integrar el consumo de servicios y conexión con el backend mediante los ViewControllers JS para todas las áreas (Pública, Administrador, Operador/Ingeniero, Comprador).
- [x] Aplicar y refinar estilos coherentes en `wwwroot/`.

---

## 4. Tareas Pendientes Inmediatas
- [x] Analizar los requerimientos de negocio en `documentacion-raw-data/` y generar el esqueleto inicial sin lógica de negocio funcional ni ORMs.
- [x] Limpiar las clases de prueba generadas por defecto en los proyectos (`Class1.cs` y `WeatherForecast.cs`).
- [x] Conectar referencias jerárquicas entre los proyectos en sus archivos `.csproj`.
- [x] Implementar los Stored Procedures SQL y el código de acceso a datos en `SqlDao` con `Microsoft.Data.SqlClient`.
- [x] Implementar las clases de `Entities-DTOs`, `DataAccess`, `CoreApp` y `WebAPI`.
- [x] Conectar el flujo de autenticación y registro en `LoginViewController.js` con las 6 vistas públicas y `UsersController`.
- [x] Conectar el Dashboard y gestión de usuarios del Administrador (`AdminDashboardViewController.js`) con `Admin/Dashboard.cshtml`, `Admin/Users.cshtml` y sus endpoints REST.
- [x] Conectar el área de Operaciones e Ingeniería (`Engineer`), implementando `OperationsDashboardViewController.js` y `TurbineManagementViewController.js` para monitorizar y gestionar el parque eólico.
- [x] Conectar el área de Clientes y Compradores (`Buyer`), implementando `BuyerDashboardViewController.js` y `BuyerManagementViewController.js` junto a sus vistas y endpoints REST.
- [x] Conectar los módulos complementarios de Operaciones y Auditoría (`Energy`, `Maintenances`, `Failures`, `CentralBank`, `FlushHistory`, `Audit`) implementando sus respectivos controladores y endpoints REST.
- [ ] **Siguiente paso prioritario:** Ejecución en servidor local (`dotnet run`) de `WebAPI` y `WebApp`, verificación de conexión a base de datos e inicio de pruebas E2E en navegador.

