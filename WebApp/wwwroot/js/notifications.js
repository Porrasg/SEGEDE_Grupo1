// notifications.js (§24.3, §29.3) - Gestión de alertas y toasts
const Notifications = {
    // Función de cliente en JavaScript que gestiona la interactividad de la interfaz y comunicación asíncrona.
    showToast: function (message, type = "info") {
        const toastContainer = document.getElementById("toastContainer");
        if (!toastContainer) return;

        const toastId = "toast_" + Date.now();
        const bgClass = type === "error" ? "bg-dark text-white border-danger" : (type === "success" ? "bg-dark text-white border-success" : "bg-white text-dark border-dark");
        
        const html = `
            <div id="${toastId}" class="toast align-items-center ${bgClass} border-2 shadow-sm mb-2" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body fw-bold">
                        ${message}
                    </div>
                    <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        toastContainer.insertAdjacentHTML('beforeend', html);
        const toastEl = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    }
};

// Objeto notify conciso según especificación §24.3
const notify = {
    success: m => Notifications.showToast(m, 'success'),
    error:   m => Notifications.showToast(m, 'error'),
    warning: m => Notifications.showToast(m, 'warning'),
    info:    m => Notifications.showToast(m, 'info')
};

// Manejador global estándar para errores de respuestas AJAX / API (§24.3)
function handleApiError(xhr) {
    const res = xhr.responseJSON || (xhr.responseText ? JSON.parse(xhr.responseText) : null);
    if (xhr.status === 401) { 
        window.session?.clear(); 
        window.location = '/Login'; 
        return; 
    }
    if (xhr.status === 403) { 
        notify.error('No tiene permisos para realizar esta acción.'); 
        return; 
    }
    if (res?.errors?.length) {
        notify.error(res.errors.join(' '));
    } else {
        notify.error(res?.message ?? res?.Message ?? 'Ocurrió un error inesperado al procesar la solicitud.');
    }
}

// Exponer en el alcance global del navegador
window.Notifications = Notifications;
window.notify = notify;
window.handleApiError = handleApiError;
