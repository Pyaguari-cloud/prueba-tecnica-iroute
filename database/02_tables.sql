/* =====================================================================
   Script 02 - Creación de tablas
   Proyecto   : Prueba Técnica i-Route
   Descripción: Crea las tablas commerce y commerce_quarantine.

   Estructura del CSV (commerce_DDMMYYYY.csv):
     pc_processdate  -> Fecha de proceso (formato yyyy-MM-dd)
     pc_codcomred    -> Código del comercio en la red
     pc_nomcomred    -> Nombre del comercio en la red
     pc_tipdoc       -> Tipo de documento (RUC / CED)
     pc_numdoc       -> Número de documento
     pc_ciudad       -> Ciudad del comercio

   Decisiones de diseño:
     - Las columnas de texto son NVARCHAR y aceptan NULL, incluida
       pc_numdoc. La tabla commerce debe poder recibir datos "sucios"
       (vacíos, con letras o símbolos), porque la validación ocurre
       después, en sp_process_commerce. Si pc_numdoc fuera numérica,
       el registro inválido fallaría al insertarse y nunca llegaría
       a la cuarentena.
     - Se agrega un id IDENTITY como llave primaria, que permite
       identificar con precisión qué fila mover y eliminar.
     - Índice sobre pc_processdate, porque el procesamiento siempre
       filtra por esa columna.
   ===================================================================== */

USE CommerceDB;
GO

/* ---------------------------------------------------------------------
   Tabla principal: commerce
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.commerce', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce
    (
        id              INT IDENTITY(1,1) NOT NULL,
        pc_processdate  DATE              NOT NULL,
        pc_codcomred    NVARCHAR(20)      NULL,
        pc_nomcomred    NVARCHAR(200)     NULL,
        pc_tipdoc       NVARCHAR(10)      NULL,
        pc_numdoc       NVARCHAR(50)      NULL,
        pc_ciudad       NVARCHAR(100)     NULL,
        created_at      DATETIME2(0)      NOT NULL
            CONSTRAINT DF_commerce_created_at DEFAULT SYSDATETIME(),

        CONSTRAINT PK_commerce PRIMARY KEY CLUSTERED (id)
    );

    CREATE NONCLUSTERED INDEX IX_commerce_processdate
        ON dbo.commerce (pc_processdate);

    PRINT 'Tabla dbo.commerce creada.';
END
GO

/* ---------------------------------------------------------------------
   Tabla de cuarentena: commerce_quarantine
   Misma estructura que commerce, más:
     - commerce_id    : id original que tenía el registro en commerce.
     - motivo         : razón por la que el registro fue rechazado.
     - quarantined_at : fecha y hora en que se movió a cuarentena.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.commerce_quarantine', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.commerce_quarantine
    (
        id              INT IDENTITY(1,1) NOT NULL,
        commerce_id     INT               NOT NULL,
        pc_processdate  DATE              NOT NULL,
        pc_codcomred    NVARCHAR(20)      NULL,
        pc_nomcomred    NVARCHAR(200)     NULL,
        pc_tipdoc       NVARCHAR(10)      NULL,
        pc_numdoc       NVARCHAR(50)      NULL,
        pc_ciudad       NVARCHAR(100)     NULL,
        motivo          NVARCHAR(500)     NOT NULL,
        quarantined_at  DATETIME2(0)      NOT NULL
            CONSTRAINT DF_commerce_quarantine_quarantined_at DEFAULT SYSDATETIME(),

        CONSTRAINT PK_commerce_quarantine PRIMARY KEY CLUSTERED (id)
    );

    CREATE NONCLUSTERED INDEX IX_commerce_quarantine_processdate
        ON dbo.commerce_quarantine (pc_processdate);

    PRINT 'Tabla dbo.commerce_quarantine creada.';
END
GO
