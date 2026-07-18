-- ============================================================================
-- 03_Index_Flush_Active.sql
-- Índice único filtrado que garantiza, a NIVEL DE BASE DE DATOS, que solo pueda
-- existir un flush en estado 'InProgress' a la vez (un único vaciado activo).
--
-- Por qué: FlushManager.PerformFlush crea el registro de Flush FUERA de la
-- transacción crítica (para poder marcarlo 'Failed' ante un fallo). Este índice
-- hace que un segundo INSERT concurrente con Status='InProgress' falle con
-- violación de índice único (error 2601/2627), que el manager traduce a
-- BusinessException("FLUSH_IN_PROGRESS") -> HTTP 409. Es la protección atómica
-- real contra dos vaciados simultáneos que drenarían las baterías dos veces.
--
-- ⚠️ BD COMPARTIDA: coordinar con el equipo antes de ejecutar. Es idempotente
-- (verifica existencia antes de crear) y NO destructivo. Si ya hubiera más de
-- una fila 'InProgress' por datos históricos inconsistentes, la creación del
-- índice fallará: en ese caso, resolver primero esas filas (marcarlas Completed
-- o Failed) y volver a ejecutar.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Flush_Active' AND object_id = OBJECT_ID('dbo.tblFlush')
)
BEGIN
    CREATE UNIQUE INDEX UX_Flush_Active
        ON dbo.tblFlush(Status)
        WHERE Status = 'InProgress';
END
GO
