/* =====================================================================
   Script 01 - Creación de la base de datos
   Proyecto   : Prueba Técnica i-Route - Carga y validación de comercios
   Requisito  : SQL Server 2017 o superior
   Descripción: Crea la base de datos CommerceDB si aún no existe.
                Es seguro ejecutarlo más de una vez.
   ===================================================================== */

IF DB_ID(N'CommerceDB') IS NULL
BEGIN
    CREATE DATABASE CommerceDB;
    PRINT 'Base de datos CommerceDB creada.';
END
ELSE
BEGIN
    PRINT 'La base de datos CommerceDB ya existe.';
END
GO
