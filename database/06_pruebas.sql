/* =====================================================================
   Script 06 - Pruebas manuales (SOLO PARA DESARROLLO)
   Proyecto   : Prueba Técnica i-Route
   Descripción: Inserta los mismos datos del CSV de ejemplo
                (commerce_13102026.csv), ejecuta la validación y
                muestra el resultado.

   Resultado esperado: quarantined_count = 7
                       commerce queda con 8 registros válidos.

   ATENCIÓN: el bloque inicial borra los datos de 2026-10-13
             para poder repetir la prueba desde cero.
   ===================================================================== */

USE CommerceDB;
GO

-- Limpieza previa (solo pruebas)
DELETE FROM dbo.commerce            WHERE pc_processdate = '2026-10-13';
DELETE FROM dbo.commerce_quarantine WHERE pc_processdate = '2026-10-13';
GO

-- ---------- Registros válidos ----------
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0001', N'Farmacia San José',          N'RUC', N'0991234567001', N'Guayaquil';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0002', N'Panadería La Espiga',        N'RUC', N'0992233445001', N'Milagro';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0003', N'Ferretería El Constructor',  N'RUC', N'1790012345001', N'Quito';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0005', N'Minimarket Don Lucho',       N'CED', N'0923456789',    N'Durán';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0007', N'Restaurante Sabor Manaba',   N'RUC', N'1391122334001', N'Manta';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0009', N'Librería Cervantes',         N'RUC', N'0190011223001', N'Cuenca';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0011', N'Óptica Visión Clara',        N'CED', N'0912345678',    N'Guayaquil';
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0014', N'Comercial Hermanos Peña',    N'RUC', N'0993344556001', N'Babahoyo';

-- ---------- Registros inválidos ----------
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0004', N'',                           N'RUC', N'0992345678001', N'Milagro';      -- nombre vacío
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0006', N'Tienda Mi Barrio',           N'RUC', N'09923A5678001', N'Guayaquil';    -- letras
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0008', N'Bazar La Economía',          N'CED', N'',              N'Machala';      -- numdoc vacío
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0010', N'Papelería El Estudiante',    N'CED', N'0912-345678',   N'Guayaquil';    -- caracter especial
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0012', N'   ',                        N'RUC', N'17900ABC45001', N'Quito';        -- nombre vacío + letras
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0013', N'Distribuidora Costa Azul',   N'RUC', N'0991.234.567',  N'Salinas';      -- caracteres especiales
EXEC dbo.sp_create_commerce '2026-10-13', N'CR0015', N'Abarrotes Doña Rosa',        N'CED', N'09#2345@78',    N'Milagro';      -- caracteres especiales
GO

-- Ejecutar la validación
EXEC dbo.sp_process_commerce @pc_processdate = '2026-10-13';
GO

-- Verificar resultados
SELECT * FROM dbo.commerce WHERE pc_processdate = '2026-10-13';
EXEC dbo.sp_get_quarantine @pc_processdate = '2026-10-13';
GO
