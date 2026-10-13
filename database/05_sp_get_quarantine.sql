/* =====================================================================
   Script 05 - Procedimiento sp_get_quarantine
   Proyecto   : Prueba Técnica i-Route
   Descripción: Lista los comercios en cuarentena con su motivo.
                El filtro por fecha es opcional: si no se envía,
                devuelve todos los registros.
   ===================================================================== */

USE CommerceDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_get_quarantine
    @pc_processdate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        q.id,
        q.commerce_id,
        q.pc_processdate,
        q.pc_codcomred,
        q.pc_nomcomred,
        q.pc_tipdoc,
        q.pc_numdoc,
        q.pc_ciudad,
        q.motivo,
        q.quarantined_at
    FROM dbo.commerce_quarantine AS q
    WHERE @pc_processdate IS NULL
       OR q.pc_processdate = @pc_processdate
    ORDER BY q.quarantined_at DESC, q.id;
END
GO
