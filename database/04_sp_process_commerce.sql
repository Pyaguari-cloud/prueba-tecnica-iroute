/* =====================================================================
   Script 04 - Procedimiento sp_process_commerce
   Proyecto   : Prueba Técnica i-Route
   Descripción: Valida los registros de commerce para una fecha de proceso
                y mueve los inválidos a commerce_quarantine.

   Reglas de validación:
     1. pc_nomcomred no debe estar vacío (NULL o solo espacios).
     2. pc_numdoc no debe estar vacío.
     3. pc_numdoc no debe contener letras.
     4. pc_numdoc no debe contener caracteres especiales.
   Si un registro incumple varias reglas, los motivos se concatenan
   separados por " | ".

   Flujo:
     a) Identifica los registros inválidos y su motivo (tabla temporal).
     b) Los inserta en commerce_quarantine.
     c) Los elimina de commerce.
   Los pasos b) y c) van dentro de una transacción: o se mueven todos
   o no se mueve ninguno. Así nunca hay registros duplicados ni perdidos.

   Retorna: una fila con la columna quarantined_count (cantidad de
            registros insertados en commerce_quarantine).

   Detalle técnico: las comparaciones LIKE usan la intercalación binaria
   Latin1_General_100_BIN2, para que los rangos [a-z] y [0-9] signifiquen
   exactamente esos caracteres, sin depender de la intercalación de la BD.
   ===================================================================== */

USE CommerceDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @pc_processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;  -- Ante cualquier error, revierte la transacción completa.

    DECLARE @quarantined_count INT = 0;

    /* -----------------------------------------------------------------
       a) Identificar registros inválidos y construir su motivo
       ----------------------------------------------------------------- */
    CREATE TABLE #invalid
    (
        commerce_id INT           NOT NULL PRIMARY KEY,
        motivo      NVARCHAR(500) NOT NULL
    );

    INSERT INTO #invalid (commerce_id, motivo)
    SELECT
        c.id,
        v.motivo
    FROM dbo.commerce AS c
    CROSS APPLY
    (
        SELECT CONCAT_WS(N' | ',
            -- Regla 1: nombre del comercio vacío
            CASE WHEN NULLIF(TRIM(c.pc_nomcomred), N'') IS NULL
                 THEN N'El nombre del comercio (nomcomred) se encuentra vacío' END,

            -- Regla 2: número de documento vacío
            CASE WHEN NULLIF(TRIM(c.pc_numdoc), N'') IS NULL
                 THEN N'El número de documento (numdoc) se encuentra vacío' END,

            -- Regla 3: número de documento con letras (incluye tildes y ñ)
            CASE WHEN TRIM(c.pc_numdoc) COLLATE Latin1_General_100_BIN2
                      LIKE N'%[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]%'
                 THEN N'El número de documento (numdoc) contiene letras' END,

            -- Regla 4: número de documento con caracteres especiales
            --          (todo lo que no sea dígito ni letra: - . / espacios, etc.)
            CASE WHEN TRIM(c.pc_numdoc) COLLATE Latin1_General_100_BIN2
                      LIKE N'%[^0-9a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]%'
                 THEN N'El número de documento (numdoc) contiene caracteres especiales' END
        ) AS motivo
    ) AS v
    WHERE c.pc_processdate = @pc_processdate
      AND v.motivo <> N'';   -- CONCAT_WS devuelve '' si ninguna regla falló

    /* -----------------------------------------------------------------
       b) y c) Mover a cuarentena y eliminar de commerce (atómico)
       ----------------------------------------------------------------- */
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.commerce_quarantine
        (
            commerce_id,
            pc_processdate,
            pc_codcomred,
            pc_nomcomred,
            pc_tipdoc,
            pc_numdoc,
            pc_ciudad,
            motivo
        )
        SELECT
            c.id,
            c.pc_processdate,
            c.pc_codcomred,
            c.pc_nomcomred,
            c.pc_tipdoc,
            c.pc_numdoc,
            c.pc_ciudad,
            i.motivo
        FROM dbo.commerce AS c
        INNER JOIN #invalid AS i ON i.commerce_id = c.id;

        SET @quarantined_count = @@ROWCOUNT;

        DELETE c
        FROM dbo.commerce AS c
        INNER JOIN #invalid AS i ON i.commerce_id = c.id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;  -- Propaga el error original hacia la API.
    END CATCH;

    SELECT @quarantined_count AS quarantined_count;
END
GO
