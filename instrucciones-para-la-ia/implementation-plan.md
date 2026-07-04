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

### Fase 2: Modelado del Dominio y Acceso a Datos
- [ ] Definir las entidades de dominio en `Entities-DTOs`.
- [ ] Crear los DTOs de entrada/salida para cada caso de uso principal.
- [ ] Configurar el contexto de base de datos (`DbContext` / Conexiones) en `DataAccess`.
- [ ] Implementar repositorios e interfaces de acceso a datos.

### Fase 3: Lógica de Negocio (`CoreApp`)
- [ ] Implementar interfaces y servicios de negocio.
- [ ] Agregar validaciones de datos y reglas de negocio en los servicios.
- [ ] Configurar mapeos entre Entidades y DTOs (ej. AutoMapper o mapeos manuales).

### Fase 4: Exposición de Servicios (`WebAPI`)
- [ ] Crear los controladores REST en `Controllers/`.
- [ ] Configurar inyección de dependencias en `Program.cs`.
- [ ] Documentar endpoints con Swagger / OpenAPI.
- [ ] Configurar manejo global de excepciones y logs.

### Fase 5: Interfaz de Usuario (`WebApp`)
- [ ] Estructurar las vistas / páginas en `Pages/` o interfaz de usuario.
- [ ] Integrar el consumo de servicios y conexión con el backend.
- [ ] Aplicar estilos coherentes en `wwwroot/`.

---

## 4. Tareas Pendientes Inmediatas
- [x] Analizar los requerimientos de negocio en `documentacion-raw-data/` y generar el esqueleto inicial sin lógica de negocio funcional ni ORMs.
- [x] Limpiar las clases de prueba generadas por defecto en los proyectos (`Class1.cs` y `WeatherForecast.cs` marcados como placeholders temporalmente).
- [x] Conectar referencias jerárquicas entre los proyectos en sus archivos `.csproj`.
- [ ] **Siguiente paso prioritario:** Implementar los Stored Procedures SQL y el código de acceso a datos en `SqlDao` con `Microsoft.Data.SqlClient` para conectar a Azure SQL Database.
- [ ] Implementar progresivamente las propiedades y métodos en las clases de `Entities-DTOs`, `DataAccess`, `CoreApp`, `WebAPI` y `WebApp` reemplazando los `TODO`s.
