// notifications.js (§29.3) - Gestión de alertas y toasts
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
