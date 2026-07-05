// BuyerDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Comprador y Perfil
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando BuyerDashboardViewController...");

    const token = session.getToken();
    const role = session.getRole();
    const userId = session.getUserId() || 1;

    if (!token || (role !== "Buyer" && role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Comprador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. PANEL PRINCIPAL (/Buyer/Dashboard)
    // ==========================================
    if (document.getElementById("buyActiveForecasts")) {
        loadBuyerDashboard();
    }

    function loadBuyerDashboard() {
        apiClient.get("Dashboard/Buyer?buyerUserId=" + userId)
            .done(function (res) {
                const d = res?.data || res?.Data || {};

                setText("buyActiveForecasts", d.activeForecasts ?? d.ActiveForecasts ?? 0);
                setText("buyMonthReq", formatNumber(d.monthRequestedMWh ?? d.MonthRequestedMWh ?? 0) + " MWh");
                setText("buyLastAssign", formatNumber(d.lastAssignment ?? d.LastAssignment ?? 0) + " MWh");
                setText("buyTotalBilled", formatNumber(d.totalBilledAccumulated ?? d.TotalBilledAccumulated ?? 0) + " CRC");

                const dateVal = d.lastStatementDate || d.LastStatementDate;
                setText("buyLastStmtDate", dateVal ? new Date(dateVal).toLocaleDateString("es-CR") : "Sin registros");
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    // ==========================================
    // 2. GESTIÓN DE PERFIL (/Buyer/Profile)
    // ==========================================
    if (document.getElementById("profName")) {
        loadBuyerProfile();

        const profileForm = document.getElementById("profileForm");
        if (profileForm) {
            profileForm.addEventListener("submit", function (e) {
                e.preventDefault();
                updateProfile();
            });
        }

        const confirmDeactBtn = document.getElementById("confirmDeactBtn");
        if (confirmDeactBtn) {
            confirmDeactBtn.addEventListener("click", function () {
                deactivateAccount();
            });
        }
    }

    function loadBuyerProfile() {
        apiClient.get("Users/" + userId)
            .done(function (res) {
                const u = res?.data || res?.Data || {};
                setText("profName", u.name || u.Name || "Comprador SGDE");
                setText("profEmail", u.email || u.Email || "-");
                setText("profId", u.identification || u.Identification || "-");
                
                const created = u.created || u.Created;
                setText("profDate", created ? new Date(created).toLocaleDateString("es-CR") : "-");

                const phoneInput = document.getElementById("pPhone");
                if (phoneInput) phoneInput.value = u.phoneNumber || u.PhoneNumber || "";
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    function updateProfile() {
        const phone = document.getElementById("pPhone")?.value.trim();
        const currPass = document.getElementById("pCurrPass")?.value;
        const newPass = document.getElementById("pNewPass")?.value;

        const submitBtn = document.querySelector("#profileForm button[type='submit']");
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Guardando...';
        }

        apiClient.put("Users/Update", {
            userId: parseInt(userId),
            phoneNumber: phone,
            currentPassword: currPass || null,
            newPassword: newPass || null,
            role: "Buyer"
        }).done(function () {
            notify.success("Datos de perfil y seguridad actualizados.");
            if (document.getElementById("pCurrPass")) document.getElementById("pCurrPass").value = "";
            if (document.getElementById("pNewPass")) document.getElementById("pNewPass").value = "";
        }).fail(function (xhr) {
            handleApiError(xhr);
        }).always(function () {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "Guardar Cambios";
            }
        });
    }

    function deactivateAccount() {
        const keepForecasts = document.getElementById("keepForecasts")?.checked || false;
        const btn = document.getElementById("confirmDeactBtn");
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Desactivando...';
        }

        apiClient.post("Users/Deactivate", {
            userId: parseInt(userId),
            cancelFutureForecasts: !keepForecasts
        }).done(function () {
            notify.info("Tu cuenta ha sido desactivada. Cerrando sesión...");
            setTimeout(() => {
                session.clear();
                window.location.href = "/Login";
            }, 2000);
        }).fail(function (xhr) {
            handleApiError(xhr);
            if (btn) {
                btn.disabled = false;
                btn.textContent = "Confirmar Desactivación";
            }
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
