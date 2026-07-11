// notifications.js (§24.3, §29.3) - Gestión de alertas y confirmaciones con SweetAlert2, consciente del tema claro/oscuro
function sgdeIsDark() {
    return document.documentElement.getAttribute("data-bs-theme") === "dark";
}

// Toast no bloqueante en la esquina superior derecha, se autocierra (Enmienda 4 — nunca congela la pantalla).
const sgdeToastMixin = function () {
    return Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true,
        customClass: { popup: sgdeIsDark() ? "sgde-swal-dark" : "sgde-swal-light" },
        didOpen: (el) => {
            el.addEventListener("mouseenter", Swal.stopTimer);
            el.addEventListener("mouseleave", Swal.resumeTimer);
        }
    });
};

const Notifications = {
    // Compatibilidad con el nombre anterior (§29.3) — ahora respaldado por SweetAlert2.
    showToast: function (message, type = "info") {
        const icon = type === "error" ? "error" : type === "success" ? "success" : type === "warning" ? "warning" : "info";
        sgdeToastMixin().fire({ icon: icon, title: message });
    }
};

const notify = {
    success: m => Notifications.showToast(m, "success"),
    error: m => Notifications.showToast(m, "error"),
    warning: m => Notifications.showToast(m, "warning"),
    info: m => Notifications.showToast(m, "info"),

    // Reemplaza al confirm() nativo del navegador (Enmienda 3: prohibido usar confirm()/alert() nativos).
    // Retorna una Promise<boolean> — uso: notify.confirm("¿Seguro?").then(ok => { if (!ok) return; ... }).
    confirm: function (message, options = {}) {
        return Swal.fire({
            icon: options.icon || "warning",
            title: options.title || "Confirmar acción",
            text: message,
            showCancelButton: true,
            confirmButtonText: options.confirmText || "Confirmar",
            cancelButtonText: options.cancelText || "Cancelar",
            confirmButtonColor: options.dangerous ? "var(--sgde-state-damaged)" : "var(--sgde-primary)",
            reverseButtons: true,
            customClass: { popup: sgdeIsDark() ? "sgde-swal-dark" : "sgde-swal-light" }
        }).then(function (result) {
            return result.isConfirmed;
        });
    }
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
