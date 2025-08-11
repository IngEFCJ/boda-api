-- Stored Procedure para obtener configuración de API por ID
CREATE PROCEDURE sp_GetApiConfigById
    @ApiId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT FIID, FCJSON
    FROM TBApis
    WHERE FIID = @ApiId
END
GO

-- Stored Procedure para validar credenciales de usuario
CREATE PROCEDURE sp_ValidateUserCredentials
    @Username NVARCHAR(100),
    @Password NVARCHAR(100),
    @ApiId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Aquí deberías implementar la lógica de validación
    -- Por ejemplo, verificar contra una tabla de usuarios
    -- Por ahora, retornamos true si el usuario y contraseña no están vacíos
    
    IF @Username IS NOT NULL AND @Username != '' AND 
       @Password IS NOT NULL AND @Password != ''
    BEGIN
        SELECT 1 -- Credenciales válidas
    END
    ELSE
    BEGIN
        SELECT 0 -- Credenciales inválidas
    END
END
GO

-- Nota: Necesitarás crear o modificar la tabla TBApis para incluir campos de usuario y contraseña
-- o crear una tabla separada para las credenciales de autenticación

-- Ejemplo de tabla de usuarios (opcional)
/*
CREATE TABLE TBUsers (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL, -- Debería estar hasheada
    ApiID INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ApiID) REFERENCES TBApis(FIID)
)
GO
*/
