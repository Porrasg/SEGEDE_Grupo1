// OperationsDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Operaciones / Ingeniero
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando OperationsDashboardViewController...");

    // Verificación de seguridad en el cliente (RBAC §24.2)
    const token = session.getToken();
    const role = session.getRole();
    if (!token || (role !== "Engineer" && role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero u Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    if (document.getElementById("opTotalTurbines")) {
        loadOperationsDashboard();
    }

    function loadOperationsDashboard() {
        apiClient.get("Dashboard/Operations")
            .done(function (res) {
                const data = res?.data || res?.Data || {};

                setText("opTotalTurbines", data.totalTurbines ?? data.TotalTurbines ?? 0);
                setText("opActiveTurbines", data.activeTurbines ?? data.ActiveTurbines ?? 0);
                setText("opMaintTurbines", data.turbinesUnderMaintenance ?? data.TurbinesUnderMaintenance ?? 0);
                setText("opDamagedTurbines", data.damagedTurbines ?? data.DamagedTurbines ?? 0);
                setText("opSuspendedTurbines", data.suspendedTurbines ?? data.SuspendedTurbines ?? 0);
                setText("opCbInventory", formatNumber(data.centralBankInventory ?? data.CentralBankInventory ?? 0) + " MWh");
                setText("opOverdueAlerts", data.overdueMaintenanceAlerts ?? data.OverdueMaintenanceAlerts ?? 0);

                const flushDate = data.lastFlushDate || data.LastFlushDate;
                setText("opFlushDate", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Sin registros");
                setText("opFlushEnergy", formatNumber(data.lastFlushEnergy ?? data.LastFlushEnergy ?? 0) + " MWh");
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function formatNumber(num) {
        return Number(num).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
});
