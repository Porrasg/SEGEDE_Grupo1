// TurbineManagementViewController.js (§22.1, §27) - Controlador JS para el Control de Turbinas y Detalle Técnico
document.addEventListener("DOMContentLoaded", function () {
    console.log("Inicializando TurbineManagementViewController...");

    const token = session.getToken();
    const role = session.getRole();
    if (!token || (role !== "Engineer" && role !== "Administrator" && role !== "Admin")) {
        notify.error("Acceso denegado. Requiere privilegios de Ingeniero u Operaciones.");
        setTimeout(() => {
            window.location.href = "/Login";
        }, 1500);
        return;
    }

    // ==========================================
    // 1. LISTADO Y OPERACIÓN (/Engineer/Turbines)
    // ==========================================
    const turbinesBody = document.getElementById("opTurbinesBody");
    if (turbinesBody) {
        let allTurbines = [];
        let selectedTurbineId = null;

        loadTurbines();

        const searchInput = document.getElementById("opSearchTurbine");
        if (searchInput) searchInput.addEventListener("input", filterAndRenderTurbines);

        function loadTurbines() {
            turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center"><span class="spinner-border spinner-border-sm" role="status"></span> Cargando turbinas...</td></tr>';
            apiClient.get("Turbines/RetrieveAll")
                .done(function (res) {
                    allTurbines = res?.data || res?.Data || [];
                    filterAndRenderTurbines();
                })
                .fail(function (xhr) {
                    turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Error al cargar la lista de turbinas.</td></tr>';
                    handleApiError(xhr);
                });
        }

        function filterAndRenderTurbines() {
            const query = searchInput?.value.toLowerCase().trim() || "";
            const filtered = allTurbines.filter(t => {
                return !query || 
                    (t.turbineCode || t.Code || "").toLowerCase().includes(query) || 
                    (t.name || "").toLowerCase().includes(query) || 
                    (t.location || "").toLowerCase().includes(query);
            });
            renderTurbinesTable(filtered);
        }

        function renderTurbinesTable(turbines) {
            if (!turbines.length) {
                turbinesBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No se encontraron turbinas.</td></tr>';
                return;
            }

            turbinesBody.innerHTML = turbines.map(t => {
                const code = t.turbineCode || t.Code || "-";
                const name = t.name || "-";
                const loc = t.location || "-";
                const cap = Number(t.weeklyNominalCapacity || t.WeeklyNominalCapacity || 0).toLocaleString("es-CR", { minimumFractionDigits: 2 });
                const stateBadge = getStateBadge(t.state || t.State);

                return `
                    <tr>
                        <td class="fw-bold">${code}</td>
                        <td>${name}</td>
                        <td>${loc}</td>
                        <td>${cap}</td>
                        <td>${stateBadge}</td>
                        <td>
                            <button class="btn btn-sm btn-outline-warning me-1 btn-state" data-id="${t.id}" data-state="${t.state || t.State || ''}" title="Cambiar Estado">
                                <i class="bi bi-gear"></i> Estado
                            </button>
                            <a href="/Engineer/TurbineDetail?id=${t.id}" class="btn btn-sm btn-outline-info" title="Ver Detalle Técnico">
                                <i class="bi bi-eye"></i> Detalle
                            </a>
                        </td>
                    </tr>
                `;
            }).join("");

            turbinesBody.querySelectorAll(".btn-state").forEach(btn => {
                btn.addEventListener("click", () => openStateModal(btn.getAttribute("data-id"), btn.getAttribute("data-state")));
            });
        }

        function getStateBadge(state) {
            const s = (state || "").toLowerCase();
            if (s === "active") return '<span class="badge bg-success">Activa</span>';
            if (s === "undermaintenance") return '<span class="badge bg-warning text-dark">Mantenimiento</span>';
            if (s === "damaged") return '<span class="badge bg-danger">Dañada / Falla</span>';
            if (s === "suspended") return '<span class="badge bg-dark">Suspendida</span>';
            return `<span class="badge bg-secondary">${state || "-"}</span>`;
        }

        const stateModalEl = document.getElementById("opStateModal");
        const stateModal = stateModalEl ? new bootstrap.Modal(stateModalEl) : null;
        const confirmStateBtn = document.getElementById("opConfirmStateBtn");
        const stateSelect = document.getElementById("opNewState");

        function openStateModal(id, currentState) {
            selectedTurbineId = id;
            if (stateSelect) {
                stateSelect.innerHTML = `
                    <option value="Active">Active (Operación Normal)</option>
                    <option value="UnderMaintenance">UnderMaintenance (En Mantenimiento)</option>
                    <option value="Damaged">Damaged (Falla Técnica)</option>
                    <option value="Suspended">Suspended (Incumplimiento / Parada)</option>
                `;
                stateSelect.value = currentState || "Active";
            }
            const reasonInput = document.getElementById("opStateReason");
            if (reasonInput) reasonInput.value = "";
            stateModal?.show();
        }

        if (confirmStateBtn) {
            confirmStateBtn.addEventListener("click", function () {
                const newState = stateSelect?.value;
                const reason = document.getElementById("opStateReason")?.value.trim();

                if (!reason) {
                    notify.warning("Por favor ingrese la razón técnica para el cambio de estado.");
                    return;
                }

                confirmStateBtn.disabled = true;
                confirmStateBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Cambiando...';

                apiClient.post("Turbines/ChangeState", {
                    turbineId: parseInt(selectedTurbineId),
                    newState: newState,
                    reason: reason
                }).done(function () {
                    notify.success("Estado operativo de la turbina actualizado.");
                    stateModal?.hide();
                    loadTurbines();
                }).fail(function (xhr) {
                    handleApiError(xhr);
                }).always(function () {
                    confirmStateBtn.disabled = false;
                    confirmStateBtn.textContent = "Confirmar";
                });
            });
        }
    }

    // ==========================================
    // 2. DETALLE TÉCNICO (/Engineer/TurbineDetail)
    // ==========================================
    if (document.getElementById("engDetName")) {
        const urlParams = new URLSearchParams(window.location.search);
        const turbineId = urlParams.get("id");

        if (!turbineId) {
            notify.error("Identificador de turbina no especificado.");
            setTimeout(() => window.location.href = "/Engineer/Turbines", 1500);
            return;
        }

        loadTurbineDetail(turbineId);
        loadTurbineMetrics(turbineId);
        loadTurbineHistory(turbineId);
    }

    function loadTurbineDetail(id) {
        apiClient.get("Turbines/" + id)
            .done(function (res) {
                const t = res?.data || res?.Data || {};
                const nameEl = document.getElementById("engDetName");
                const metaEl = document.getElementById("engDetMeta");
                const statusEl = document.getElementById("engDetStatus");

                if (nameEl) nameEl.textContent = t.name || `Turbina #${t.id || id}`;
                if (metaEl) metaEl.textContent = `Código: ${t.turbineCode || t.Code || "-"} | Ubicación: ${t.location || "-"} | Capacidad: ${Number(t.weeklyNominalCapacity || t.WeeklyNominalCapacity || 0).toLocaleString("es-CR")} MWh/sem`;
                
                if (statusEl) {
                    const s = (t.state || t.State || "").toLowerCase();
                    statusEl.textContent = t.state || t.State || "Unknown";
                    statusEl.className = "badge fs-6 " + (s === "active" ? "bg-success" : s === "undermaintenance" ? "bg-warning text-dark" : s === "damaged" ? "bg-danger" : "bg-secondary");
                }
            })
            .fail(function (xhr) {
                handleApiError(xhr);
            });
    }

    function loadTurbineMetrics(id) {
        apiClient.get("Turbines/Metrics/" + id)
            .done(function (res) {
                const m = res?.data || res?.Data || {};
                setText("engValDo", (m.operationalAvailability ?? m.OperationalAvailability ?? 0) + "%");
                setText("engValIo", (m.operationalUnavailability ?? m.OperationalUnavailability ?? 0) + "%");
                setText("engValMtbf", Number(m.mtbf ?? m.MTBF ?? 0).toLocaleString("es-CR", { maximumFractionDigits: 1 }) + " hrs");
                setText("engValMttr", Number(m.mttr ?? m.MTTR ?? 0).toLocaleString("es-CR", { maximumFractionDigits: 1 }) + " hrs");
            })
            .fail(function (xhr) {
                console.error("Error cargando métricas:", xhr);
            });
    }

    function loadTurbineHistory(id) {
        apiClient.get("Turbines/History/" + id)
            .done(function (res) {
                const h = res?.data || res?.Data || {};

                // Render Estado Histórico
                const histBody = document.getElementById("engHistoryBody");
                const stateChanges = h.stateChanges || h.StateChanges || [];
                if (histBody) {
                    histBody.innerHTML = stateChanges.length ? stateChanges.map(s => `
                        <tr>
                            <td>${new Date(s.changeDate || s.ChangeDate).toLocaleString("es-CR")}</td>
                            <td><span class="badge bg-secondary">${s.previousState || s.PreviousState || "-"}</span></td>
                            <td><span class="badge bg-primary">${s.newState || s.NewState || "-"}</span></td>
                            <td>${s.reason || s.Reason || "-"}</td>
                            <td>Usuario #${s.changedByUserId || s.ChangedByUserId || "---"}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="5" class="text-center text-muted">Sin cambios de estado registrados.</td></tr>';
                }

                // Render Mantenimientos
                const maintBody = document.getElementById("engMaintBody");
                const maintenances = h.maintenances || h.Maintenances || [];
                if (maintBody) {
                    maintBody.innerHTML = maintenances.length ? maintenances.map(m => `
                        <tr>
                            <td><span class="badge bg-info text-dark">${m.maintenanceType || m.MaintenanceType || "-"}</span></td>
                            <td>${new Date(m.scheduledStart || m.ScheduledStart).toLocaleDateString("es-CR")}</td>
                            <td>${new Date(m.scheduledEnd || m.ScheduledEnd).toLocaleDateString("es-CR")}</td>
                            <td><span class="badge bg-warning text-dark">${m.status || m.Status || "-"}</span></td>
                            <td>${m.outcomeNotes || m.OutcomeNotes || "-"}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="5" class="text-center text-muted">No hay mantenimientos registrados.</td></tr>';
                }

                // Render Fallas
                const failBody = document.getElementById("engFailBody");
                const failures = h.failures || h.Failures || [];
                if (failBody) {
                    failBody.innerHTML = failures.length ? failures.map(f => `
                        <tr>
                            <td>${new Date(f.failureDate || f.FailureDate).toLocaleString("es-CR")}</td>
                            <td><span class="badge bg-danger">${f.severityLevel || f.SeverityLevel || "-"}</span></td>
                            <td>${f.description || f.Description || "-"}</td>
                        </tr>
                    `).join("") : '<tr><td colspan="3" class="text-center text-muted">No se han reportado averías en esta turbina.</td></tr>';
                }
            })
            .fail(function (xhr) {
                console.error("Error cargando historial de turbina:", xhr);
            });
    }

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }
});
