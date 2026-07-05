// apiClient.js (§24.1) - Cliente HTTP modular para comunicación AJAX con la Web API
const apiClient = (function () {
    // Obtiene la base URL configurada en el servidor o utiliza el valor por defecto
    const BASE = window.SGDE_API_BASE || "https://localhost:7056/api/";

    // Construye la URL completa sumando la ruta relativa al endpoint base
    function url(path) { 
        const cleanPath = path.startsWith('/') ? path.substring(1) : path;
        return BASE + cleanPath; 
    }

    // Genera el encabezado de autorización Bearer si existe una sesión activa en el navegador
    function authHeader() {
        const token = window.session?.getToken();
        return token ? { 'Authorization': 'Bearer ' + token } : {};
    }

    // Realiza una petición asíncrona utilizando jQuery AJAX con soporte de autenticación y JSON
    function request(method, path, body) {
        return $.ajax({
            url: url(path),
            method: method,
            contentType: 'application/json',
            headers: authHeader(),
            data: body ? JSON.stringify(body) : null
        });
    }

    return {
        url,
        authHeader,
        // Ejecuta petición GET asíncrona para obtener recursos
        get: p => request('GET', p),
        // Ejecuta petición POST asíncrona para crear recursos o procesar acciones
        post: (p, b) => request('POST', p, b),
        // Ejecuta petición PUT asíncrona para modificar recursos existentes
        put: (p, b) => request('PUT', p, b),
        // Ejecuta petición DELETE asíncrona para eliminar recursos
        delete: (p, b) => request('DELETE', p, b)
    };
})();

// Exponer en el objeto global con inicial minúscula y mayúscula por compatibilidad
window.apiClient = apiClient;
window.ApiClient = apiClient;
