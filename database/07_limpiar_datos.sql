/* =====================================================================
   Script 07 - Limpieza de datos (SOLO PARA DESARROLLO)
   Proyecto   : Prueba Técnica i-Route
   Descripción: Vacía las tablas commerce y commerce_quarantine y
                reinicia sus contadores IDENTITY, para repetir las
                pruebas desde cero (por ejemplo, desde Swagger o Angular).
   ===================================================================== */

USE CommerceDB;
GO

TRUNCATE TABLE dbo.commerce;
TRUNCATE TABLE dbo.commerce_quarantine;
GO

PRINT 'Tablas commerce y commerce_quarantine vaciadas.';
GO
