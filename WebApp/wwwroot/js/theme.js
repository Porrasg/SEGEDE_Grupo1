// theme.js (§29.4) - Gestión de modo oscuro / claro (monochrome alto contraste)
document.addEventListener("DOMContentLoaded", function () {
    const btnToggle = document.getElementById("btnToggleTheme");
    const themeIcon = document.getElementById("themeIcon");
    
    // Cargar tema persistido en localStorage o preferido por sistema
    let currentTheme = localStorage.getItem("sgde_theme") || "light";
    applyTheme(currentTheme);

    if (btnToggle) {
        btnToggle.addEventListener("click", function () {
            currentTheme = currentTheme === "light" ? "dark" : "light";
            applyTheme(currentTheme);
            localStorage.setItem("sgde_theme", currentTheme);
        });
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-bs-theme", theme);
        if (btnToggle && themeIcon) {
            if (theme === "dark") {
                btnToggle.innerHTML = '<span id="themeIcon">◑</span> Modo Claro';
                btnToggle.classList.replace("btn-outline-secondary", "btn-outline-light");
            } else {
                btnToggle.innerHTML = '<span id="themeIcon">◑</span> Modo Oscuro';
                btnToggle.classList.replace("btn-outline-light", "btn-outline-secondary");
            }
        }
    }
});
