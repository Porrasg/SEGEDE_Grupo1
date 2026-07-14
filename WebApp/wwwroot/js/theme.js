// theme.js (§29.4) - Gestión de modo oscuro / claro (monochrome alto contraste)
document.addEventListener("DOMContentLoaded", function () {
    const btnToggle = document.getElementById("btnToggleTheme");
    const themeIcon = document.getElementById("themeIcon");
    
    // Cargar tema persistido en localStorage o preferido por sistema
    let currentTheme = localStorage.getItem("sgde_theme") || "light";
    applyTheme(currentTheme);

    if (btnToggle) {
        // Función de cliente en JavaScript que gestiona la interactividad de la interfaz y comunicación asíncrona.
        btnToggle.addEventListener("click", function () {
            currentTheme = currentTheme === "light" ? "dark" : "light";
            applyTheme(currentTheme);
            localStorage.setItem("sgde_theme", currentTheme);
        });
    }

    // Función de cliente en JavaScript que gestiona la interactividad de la interfaz y comunicación asíncrona.
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
        applyChartJsTheme(theme);
    }

    // Chart.js no hereda el tema de Bootstrap: sus textos de leyenda/ejes usan un gris casi
    // negro por defecto, ilegible sobre las cards oscuras del modo oscuro. Se fija el color
    // global aquí y se refrescan los gráficos ya creados (los dashboards los crean después
    // de este listener, así que además quedan bien desde la primera carga).
    function applyChartJsTheme(theme) {
        if (typeof Chart === "undefined") return;
        const styles = getComputedStyle(document.documentElement);
        const textColor = styles.getPropertyValue("--sgde-body-secondary").trim() || (theme === "dark" ? "#A3A1AC" : "#5C5A66");
        const gridColor = styles.getPropertyValue("--sgde-border").trim() || (theme === "dark" ? "#383546" : "#DDDCE4");

        Chart.defaults.color = textColor;
        Chart.defaults.borderColor = gridColor;

        Object.values(Chart.instances || {}).forEach(function (chart) {
            chart.update();
        });
    }
});
