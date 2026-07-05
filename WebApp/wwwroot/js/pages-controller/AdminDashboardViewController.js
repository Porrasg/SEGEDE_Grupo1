// AdminDashboardViewController.js (§22.1, §27) - Controlador JS para el Panel de Administración y Gestión de Usuarios
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando AdminDashboardViewController...");

    // Verificación de seguridad en el cliente (RBAC §24.2)
    const token = session.getToken();
    const role = session.getRole();
    if (!token || (role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Administrador.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. PANEL PRINCIPAL (/Admin/Dashboard)
    // ==========================================
    if (document.getElementById("kpiTotalTurbines")) {
        loadAdminDashboard();
    }

    function loadAdminDashboard() {
        apiClient.get("Dashboard/Admin")
            .done(function (res) {
                const data = res?.data || res?.Data || {};
                
                setText("kpiTotalTurbines", data.totalTurbines ?? data.TotalTurbines ?? 0);
                setText("kpiActiveTurbines", data.activeTurbines ?? data.ActiveTurbines ?? 0);
                setText("kpiCbInventory", formatNumber(data.centralBankInventory ?? data.CentralBankInventory ?? 0) + " MWh");
                setText("kpiEffectiveCap", formatNumber(data.effectiveCapacity ?? data.EffectiveCapacity ?? 0) + " MWh");
                setText("kpiMonthForecasts", data.monthForecasts ?? data.MonthForecasts ?? 0);
                setText("kpiTotalDemand", formatNumber(data.monthTotalDemand ?? data.MonthTotalDemand ?? 0) + " MWh");
                setText("kpiTotalBilled", "₡ " + formatNumber(data.monthTotalBilled ?? data.MonthTotalBilled ?? 0));
                
                const flushDate = data.lastFlush || data.LastFlush;
                setText("kpiLastFlush", flushDate ? new Date(flushDate).toLocaleDateString("es-CR", { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Sin registros");
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

    // ==========================================
    // 2. GESTIÓN DE USUARIOS (/Admin/Users)
    // ==========================================
    const tableBody = document.getElementById("usersTableBody");
    if (tableBody) {
        let allUsers = [];
        let editingUserId = null;

        loadUsers();

        // Búsqueda y filtrado en tiempo real
        const searchInput = document.getElementById("searchUser");
        const filterSelect = document.getElementById("filterRole");

        if (searchInput) searchInput.addEventListener("input", filterAndRenderUsers);
        if (filterSelect) filterSelect.addEventListener("change", filterAndRenderUsers);

        function loadUsers() {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando usuarios...</td></tr>';
            apiClient.get("Users/RetrieveAll")
                .done(function (res) {
                    allUsers = res?.data || res?.Data || [];
                    filterAndRenderUsers();
                })
                .fail(function (xhr) {
                    tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al cargar la lista de usuarios.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderUsers() {
            const query = searchInput?.value.toLowerCase().trim() || "";
            const roleFilter = filterSelect?.value || "";

            const filtered = allUsers.filter(u => {
                const matchesQuery = !query || 
                    (u.identification || "").toLowerCase().includes(query) || 
                    (u.firstName || "").toLowerCase().includes(query) || 
                    (u.lastName || "").toLowerCase().includes(query) || 
                    (u.email || "").toLowerCase().includes(query);
                const matchesRole = !roleFilter || (u.role || "") === roleFilter;
                return matchesQuery && matchesRole;
            });

            renderUsersTable(filtered);
        }

        function renderUsersTable(users) {
            if (!users.length) {
                tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron usuarios que coincidan con los filtros.</td></tr>';
                return;
            }

            tableBody.innerHTML = users.map(u => {
                const fullName = `${u.firstName || ""} ${u.lastName || ""}`.trim();
                const roleBadge = getRoleBadge(u.role);
                const statusBadge = getStatusBadge(u.status);
                const isActive = (u.status || "").toLowerCase() === "active";

                let actions = `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${u.id}" title="Editar usuario"><i class="bi bi-pencil"></i> Editar</button>`;
                if (isActive) {
                    actions += `<button class="btn btn-sm btn-outline-danger btn-deactivate" data-id="${u.id}" title="Desactivar usuario"><i class="bi bi-person-x"></i> Desactivar</button>`;
                } else {
                    actions += `<button class="btn btn-sm btn-outline-success btn-reactivate" data-id="${u.id}" title="Reactivar usuario"><i class="bi bi-person-check"></i> Reactivar</button>`;
                }

                return `
                    <tr>
                        <td class="fw-bold">${u.identification || "-"}</td>
                        <td>${fullName || "-"}</td>
                        <td>${u.email || "-"}</td>
                        <td>${roleBadge}</td>
                        <td>${statusBadge}</td>
                        <td>${actions}</td>
                    </tr>
                `;
            }).join("");

            // Enlazar eventos de acción
            tableBody.querySelectorAll(".btn-edit").forEach(btn => {
                btn.addEventListener("click", () => openEditModal(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-deactivate").forEach(btn => {
                btn.addEventListener("click", () => deactivateUser(btn.getAttribute("data-id")));
            });
            tableBody.querySelectorAll(".btn-reactivate").forEach(btn => {
                btn.addEventListener("click", () => reactivateUser(btn.getAttribute("data-id")));
            });
        }

        function getRoleBadge(role) {
            if (role === "Administrator") return '<span class="badge bg-danger">Administrador</span>';
            if (role === "Engineer") return '<span class="badge bg-info text-dark">Ingeniero</span>';
            if (role === "Buyer") return '<span class="badge bg-success">Comprador</span>';
            return `<span class="badge bg-secondary">${role || "-"}</span>`;
        }

        function getStatusBadge(status) {
            if ((status || "").toLowerCase() === "active") return '<span class="badge bg-success">Activo</span>';
            if ((status || "").toLowerCase() === "inactive") return '<span class="badge bg-secondary">Inactivo</span>';
            if ((status || "").toLowerCase() === "blocked") return '<span class="badge bg-danger">Bloqueado</span>';
            return `<span class="badge bg-warning text-dark">${status || "-"}</span>`;
        }

        // Acciones modales y CRUD
        const userModalEl = document.getElementById("userModal");
        const userModal = userModalEl ? new bootstrap.Modal(userModalEl) : null;
        const saveBtn = document.getElementById("saveUserBtn");

        const btnNewUser = document.querySelector('button[data-bs-target="#userModal"]');
        if (btnNewUser) {
            btnNewUser.addEventListener("click", function () {
                editingUserId = null;
                document.getElementById("userForm")?.reset();
                const modalTitle = userModalEl?.querySelector(".modal-title");
                if (modalTitle) modalTitle.textContent = "Nuevo Usuario Interno";
                
                const idInput = document.getElementById("uId");
                const emailInput = document.getElementById("uEmail");
                if (idInput) idInput.disabled = false;
                if (emailInput) emailInput.disabled = false;
            });
        }

        function openEditModal(id) {
            const u = allUsers.find(item => String(item.id) === String(id));
            if (!u) return;

            editingUserId = u.id;
            const modalTitle = userModalEl?.querySelector(".modal-title");
            if (modalTitle) modalTitle.textContent = "Editar Usuario Interno";

            const idInput = document.getElementById("uId");
            const first1Input = document.getElementById("uFirst1") || document.getElementById("uFirst");
            const first2Input = document.getElementById("uFirst2");
            const last1Input = document.getElementById("uLast1") || document.getElementById("uLast");
            const last2Input = document.getElementById("uLast2");
            const emailInput = document.getElementById("uEmail");
            const roleInput = document.getElementById("uRole");

            if (idInput) { idInput.value = u.identification || ""; idInput.disabled = true; }
            if (first1Input) {
                const names = (u.firstName || "").split(" ");
                first1Input.value = names[0] || "";
                if (first2Input) first2Input.value = names.slice(1).join(" ") || "";
            }
            if (last1Input) {
                const lasts = (u.lastName || "").split(" ");
                last1Input.value = lasts[0] || "";
                if (last2Input) last2Input.value = lasts.slice(1).join(" ") || "";
            }
            if (emailInput) { emailInput.value = u.email || ""; emailInput.disabled = true; }
            if (roleInput) roleInput.value = u.role || "Engineer";

            userModal?.show();
        }

        if (saveBtn) {
            saveBtn.addEventListener("click", function () {
                const idVal = document.getElementById("uId")?.value.trim();
                const f1 = document.getElementById("uFirst1")?.value.trim() || document.getElementById("uFirst")?.value.trim() || "";
                const f2 = document.getElementById("uFirst2")?.value.trim() || "";
                const l1 = document.getElementById("uLast1")?.value.trim() || document.getElementById("uLast")?.value.trim() || "";
                const l2 = document.getElementById("uLast2")?.value.trim() || "";
                
                const firstVal = f2 ? `${f1} ${f2}` : f1;
                const lastVal = l2 ? `${l1} ${l2}` : l1;
                const emailVal = document.getElementById("uEmail")?.value.trim();
                const roleVal = document.getElementById("uRole")?.value;
                const passVal = document.getElementById("uPass")?.value;

                if (!firstVal || !lastVal) {
                    notify.warning("Por favor ingrese primer nombre y primer apellido.");
                    return;
                }

                saveBtn.disabled = true;
                saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

                if (editingUserId) {
                    // Editar usuario existente
                    const dto = {
                        userId: editingUserId,
                        firstName: firstVal,
                        lastName: lastVal,
                        phone: "88888888",
                        role: roleVal,
                        status: "Active"
                    };

                    apiClient.put("Users/Update", dto)
                        .done(function (res) {
                            notify.success("Usuario actualizado correctamente.");
                            userModal?.hide();
                            loadUsers();
                        })
                        .fail(function (xhr) {
                            handleApiError(xhr);
                        })
                        .always(function () {
                            saveBtn.disabled = false;
                            saveBtn.textContent = "Guardar";
                        });
                } else {
                    // Crear nuevo usuario interno
                    if (!idVal || !emailVal) {
                        notify.warning("Por favor complete identificación y correo para nuevos usuarios.");
                        saveBtn.disabled = false;
                        saveBtn.textContent = "Guardar";
                        return;
                    }

                    const dto = {
                        identification: idVal,
                        firstName: firstVal,
                        lastName: lastVal,
                        email: emailVal,
                        role: roleVal,
                        phone: "88888888",
                        birthDate: "1990-01-01",
                        password: passVal || "SEGEDE_Temp123!"
                    };

                    apiClient.post("Users/Internal", dto)
                        .done(function (res) {
                            notify.success("Usuario interno creado con éxito. Contraseña temporal generada.");
                            userModal?.hide();
                            loadUsers();
                        })
                        .fail(function (xhr) {
                            handleApiError(xhr);
                        })
                        .always(function () {
                            saveBtn.disabled = false;
                            saveBtn.textContent = "Guardar";
                        });
                }
            });
        }

        function deactivateUser(id) {
            if (!confirm("¿Está seguro de que desea desactivar este usuario?")) return;
            apiClient.post("Users/Deactivate", { userId: parseInt(id), cancelForecasts: true })
                .done(function () {
                    notify.success("Usuario desactivado correctamente.");
                    loadUsers();
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                });
        }

        function reactivateUser(id) {
            if (!confirm("¿Desea reactivar este usuario?")) return;
            apiClient.post(`Users/Reactivate/${id}`)
                .done(function () {
                    notify.success("Usuario reactivado con éxito.");
                    loadUsers();
                })
                .fail(function (xhr) {
                    handleApiError(xhr);
                });
        }
    }
});
