/* =====================================================================
   Script 03 - Procedimiento sp_create_commerce
   Proyecto   : Prueba Técnica i-Route
   Descripción: Inserta un registro del archivo CSV en la tabla commerce.
                La API lo invoca una vez por cada fila del archivo,
                reutilizando una sola conexión y una sola transacción.

   Nota: los valores se guardan tal como llegan. No se valida aquí,
         porque la validación es responsabilidad de sp_process_commerce.
   ===================================================================== */

USE CommerceDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @pc_processdate  DATE,
    @pc_codcomred    NVARCHAR(20)  = NULL,
    @pc_nomcomred    NVARCHAR(200) = NULL,
    @pc_tipdoc       NVARCHAR(10)  = NULL,
    @pc_numdoc       NVARCHAR(50)  = NULL,
    @pc_ciudad       NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
    (
        pc_processdate,
        pc_codcomred,
        pc_nomcomred,
        pc_tipdoc,
        pc_numdoc,
        pc_ciudad
    )
    VALUES
    (
        @pc_processdate,
        @pc_codcomred,
        @pc_nomcomred,
        @pc_tipdoc,
        @pc_numdoc,
        @pc_ciudad
    );
END
GO
