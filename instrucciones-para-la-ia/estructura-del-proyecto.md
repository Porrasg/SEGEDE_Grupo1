# Estructura del Proyecto - SEGEDE_Grupo1

Este documento proporciona una visión general y detallada de la organización de directorios y archivos de la solución **SEGEDE_Grupo1**. Su propósito es servir de mapa para que los desarrolladores y la IA entiendan rápidamente dónde se ubica cada responsabilidad dentro de la arquitectura.

---

## 🗺️ Vista General de la Estructura

```text
SEGEDE_Grupo1/
│
├── SEGEDE_Grupo1.slnx                 # Archivo principal de solución .NET
├── .gitignore                         # Reglas de exclusión de control de versiones de Git
│
├── Entities-DTOs/                     # Capa 1: Modelo de Dominio y Transferencia de Datos
│   ├── Entities-DTOs.csproj           # Archivo de proyecto C#
│   └── Class1.cs                      # (Plantilla inicial)
│
├── DataAccess/                        # Capa 2: Persistencia y Acceso a Base de Datos
│   ├── DataAccess.csproj              # Archivo de proyecto C#
│   └── Class1.cs                      # (Plantilla inicial)
│
├── CoreApp/                           # Capa 3: Lógica de Negocio y Servicios
│   ├── CoreApp.csproj                 # Archivo de proyecto C#
│   └── Class1.cs                      # (Plantilla inicial)
│
├── WebAPI/                            # Capa 4: API REST Backend
│   ├── WebAPI.csproj                  # Archivo de proyecto C#
│   ├── Program.cs                     # Configuración de servicios, DI y tubería HTTP de la API
│   ├── appsettings.json               # Configuración de aplicación (cadenas de conexión, logs, etc.)
│   ├── WebAPI.http                    # Archivo de pruebas HTTP locales para endpoints
│   ├── WeatherForecast.cs             # (Modelo de prueba por defecto)
│   ├── Controllers/                   # Controladores REST que manejan las peticiones HTTP
│   └── Properties/                    # Configuración de ejecución (launchSettings.json)
│
├── WebApp/                            # Capa 5: Interfaz de Usuario Frontend (Web)
│   ├── WebApp.csproj                  # Archivo de proyecto C#
│   ├── Program.cs                     # Configuración de servicios y tubería web del cliente
│   ├── appsettings.json               # Configuración del frontend
│   ├── Pages/                         # Vistas / Páginas Razor o componentes de interfaz de usuario
│   ├── wwwroot/                       # Archivos estáticos públicos (CSS, JavaScript, imágenes)
│   └── Properties/                    # Configuración de ejecución (launchSettings.json)
│
└── instrucciones-para-la-ia/          # ⚠️ Carpeta local de control y directrices (Ignorada en Git)
    ├── implementation-plan.md         # Plan de implementación, objetivos y arquitectura
    ├── progreso.md                    # Registro del progreso y siguientes pasos de desarrollo
    ├── estructura-del-proyecto.md     # Este archivo explicativo de la estructura
    └── documentacion-raw-data/        # Referencias y requerimientos agregados manualmente
```

---

## 🏛️ Propósito y Detalle de Cada Capa

### 1. `Entities-DTOs` (Capa de Modelo y Datos Compartidos)
- **Propósito**: Es la base del sistema y representa los datos puros.
- **Qué debe contener**:
  - **Entities (Entidades)**: Clases que corresponden directamente a las tablas de la base de datos (ej. `Usuario.cs`, `Rol.cs`, `Producto.cs`).
  - **DTOs (Data Transfer Objects)**: Clases especializadas para transportar datos entre el backend y los clientes o entre capas sin exponer modelos de base de datos internos (ej. `UsuarioCreateDto.cs`, `LoginResponseDto.cs`).
- **Dependencias**: No debe depender de ninguna otra capa del proyecto.

### 2. `DataAccess` (Capa de Acceso a Datos)
- **Propósito**: Manejar la comunicación directa con el motor de base de datos.
- **Qué debe contener**:
  - Contextos de base de datos (por ejemplo, `SegedeDbContext.cs` en Entity Framework Core).
  - Implementación de repositorios (`Repositories/`) y patrones de acceso a datos.
  - Configuración y mapeo de tablas (Fluent API o Data Annotations).
- **Dependencias**: Depende de `Entities-DTOs`.

### 3. `CoreApp` (Capa de Lógica de Negocio)
- **Propósito**: Contener el corazón de la aplicación, las reglas de negocio y los flujos de trabajo principales.
- **Qué debe contener**:
  - **Servicios (`Services/`)**: Lógica que coordina operaciones entre el repositorio de base de datos y validaciones de negocio.
  - **Interfaces (`Interfaces/`)**: Definición de contratos para inversión de dependencias.
  - **Validaciones**: Reglas para asegurar la consistencia del negocio antes de llamar al acceso a datos.
- **Dependencias**: Depende de `Entities-DTOs` y `DataAccess` (o sus interfaces).

### 4. `WebAPI` (Backend / Capa de Presentación REST)
- **Propósito**: Exponer la funcionalidad del sistema al exterior mediante endpoints HTTP RESTful para ser consumidos por el frontend u otros clientes.
- **Archivos Clave**:
  - `Program.cs`: Punto de entrada que configura la inyección de dependencias (DI), CORS, Swagger, JWT y el ciclo de vida de la aplicación.
  - `Controllers/`: Define los endpoints HTTP (`GET`, `POST`, `PUT`, `DELETE`).
  - `appsettings.json`: Almacena configuraciones del servidor, como cadenas de conexión y claves secretas.

### 5. `WebApp` (Frontend / Capa de Interfaz de Usuario)
- **Propósito**: Brindar una interfaz gráfica web a los usuarios finales del sistema.
- **Archivos Clave**:
  - `Pages/`: Contiene las páginas, vistas y componentes que se presentan al usuario en el navegador.
  - `wwwroot/`: Almacena las hojas de estilo (CSS), scripts frontend (JS), iconos e imágenes estáticas del portal web.

### 6. `instrucciones-para-la-ia` (Directrices Locales de Inteligencia Artificial)
- **Propósito**: Carpeta de gobernanza, planificación y contexto diseñada exclusivamente para el entorno local y la asistencia con Inteligencia Artificial.
- **Nota Importante**: Esta carpeta se encuentra excluida en `.gitignore` y **nunca será empujada a GitHub**, protegiendo notas internas, directrices y contexto temporal del desarrollador y la IA.
