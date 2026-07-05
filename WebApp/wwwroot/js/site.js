// site.js (§29.2, §29.3) - Control global de navegación, estética intuitiva, roles y seguridad
document.addEventListener("DOMContentLoaded", function () {
    initNavigation();
    initSignOut();
    initLandingLogin();
    checkRouteSecurity();
});

function initNavigation() {
    const isLoggedIn = !session.isExpired() && session.getToken();
    const role = session.getRole();
    const email = session.getEmail() || "Usuario Autenticado";

    const navLiAdmin = document.getElementById("navLiAdmin");
    const navLiEngineer = document.getElementById("navLiEngineer");
    const navLiBuyer = document.getElementById("navLiBuyer");
    const navLiPublicLogin = document.getElementById("navLiPublicLogin");
    const navLiPublicRegister = document.getElementById("navLiPublicRegister");
    const navUserName = document.getElementById("navUserName");
    const btnSignOut = document.getElementById("btnSignOut");

    if (isLoggedIn) {
        if (btnSignOut) btnSignOut.classList.remove("d-none");
        if (navLiPublicLogin) navLiPublicLogin.classList.add("d-none");
        if (navLiPublicRegister) navLiPublicRegister.classList.add("d-none");

        if (role === "Administrator" || role === "Admin") {
            if (navLiAdmin) navLiAdmin.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-shield-check"></i> ${email} [Admin]`;
                navUserName.className = "badge bg-danger border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        } else if (role === "Engineer") {
            if (navLiEngineer) navLiEngineer.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-lightning-charge-fill"></i> ${email} [Ingeniero]`;
                navUserName.className = "badge bg-info border border-dark text-dark px-3 py-2 fw-bold shadow-sm";
            }
        } else if (role === "Buyer") {
            if (navLiBuyer) navLiBuyer.classList.remove("d-none");
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-building"></i> ${email} [Comprador]`;
                navUserName.className = "badge bg-success border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        } else {
            if (navUserName) {
                navUserName.innerHTML = `<i class="bi bi-person-fill"></i> ${email}`;
                navUserName.className = "badge bg-primary border border-dark text-white px-3 py-2 fw-bold shadow-sm";
            }
        }
    } else {
        // No logueado: mantener ocultos menús de roles y mostrar botones de login/registro
        if (navLiAdmin) navLiAdmin.classList.add("d-none");
        if (navLiEngineer) navLiEngineer.classList.add("d-none");
        if (navLiBuyer) navLiBuyer.classList.add("d-none");
        if (btnSignOut) btnSignOut.classList.add("d-none");
        if (navLiPublicLogin) navLiPublicLogin.classList.remove("d-none");
        if (navLiPublicRegister) navLiPublicRegister.classList.remove("d-none");
        if (navUserName) {
            navUserName.innerHTML = `🔒 No Autenticado`;
            navUserName.className = "badge bg-secondary border border-dark text-dark px-3 py-2 fw-bold";
        }
    }
}

function initSignOut() {
    const btnSignOut = document.getElementById("btnSignOut");
    if (!btnSignOut) return;

    btnSignOut.addEventListener("click", function (e) {
        e.preventDefault();
        session.clear();
        if (typeof notify !== "undefined") {
            notify.info("Has cerrado sesión exitosamente.");
        }
        setTimeout(() => {
            window.location.href = "/Login";
        }, 300);
    });
}

function initLandingLogin() {
    const form = document.getElementById("landingLoginForm");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        const ident = document.getElementById("loginIdent")?.value.trim();
        const pass = document.getElementById("loginPass")?.value;

        if (!ident || !pass) {
            if (typeof notify !== "undefined") notify.warning("Por favor ingrese usuario y contraseña.");
            return;
        }

        const btn = form.querySelector("button[type='submit']");
        const originalText = btn ? btn.innerHTML : "";
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Conectando...';
        }

        apiClient.post("Users/Login", { identificationOrEmail: ident, password: pass })
            .done(function (res) {
                const data = res?.data || res?.Data || res;
                const userEmail = data?.email || ident;
                sessionStorage.setItem("sgde_login_email", userEmail);
                if (typeof notify !== "undefined") notify.success("Credenciales validadas. Redirigiéndote a verificación OTP...");
                setTimeout(() => {
                    window.location.href = `/LoginOtp?email=${encodeURIComponent(userEmail)}`;
                }, 800);
            })
            .fail(function (xhr) {
                if (btn) {
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }
                if (typeof handleApiError === "function") {
                    handleApiError(xhr);
                } else {
                    alert("Error al iniciar sesión: Verifique sus credenciales.");
                }
            });
    });
}

function checkRouteSecurity() {
    const path = window.location.pathname.toLowerCase();
    const isLoggedIn = !session.isExpired() && session.getToken();
    const role = session.getRole();

    // Páginas públicas que no requieren validación de rol
    if (path === "/" || path === "/index" || path.startsWith("/login") || path.startsWith("/register") || path.startsWith("/recover") || path.startsWith("/reset") || path.startsWith("/activate") || path.startsWith("/accessdenied")) {
        return;
    }

    if (!isLoggedIn) {
        window.location.href = `/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
        return;
    }

    // Validación por Rol
    if (path.startsWith("/admin/") && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
    if (path.startsWith("/engineer/") && role !== "Engineer" && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
    if (path.startsWith("/buyer/") && role !== "Buyer" && role !== "Administrator" && role !== "Admin") {
        window.location.href = "/AccessDenied";
        return;
    }
}
