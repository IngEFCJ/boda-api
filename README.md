# API Confirmación Asistencia Invitación

## Endpoint de Autenticación

### Generar Token JWT

**POST** `/api/auth/token/{apiId}`

Genera un bearer token usando autenticación Basic.

#### Parámetros

- `apiId` (path): ID de la API (ej: 104)

#### Headers

- `Authorization`: Basic Auth con usuario y contraseña
  - Formato: `Basic {base64(username:password)}`

#### Ejemplo de Uso

```bash
curl -X POST "https://localhost:7000/api/auth/token/104" \
  -H "Authorization: Basic dXN1YXJpbzE6Y29udHJhc2VhMQ==" \
  -H "Content-Type: application/json"
```

#### Respuesta Exitosa (200)

```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Autenticación exitosa",
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

#### Respuesta de Error (401)

```json
{
  "success": false,
  "message": "Credenciales inválidas"
}
```

### Validar Token

**POST** `/api/auth/validate`

Valida un token JWT.

#### Body

```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=your_database;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyHere12345678901234567890",
    "Issuer": "ApiConfirmacionAsistenciaInvitacion",
    "Audience": "ApiConfirmacionAsistenciaInvitacion",
    "ExpirationHours": 24
  }
}
```

### Base de Datos

La API utiliza la tabla `TBApis` con los siguientes campos:
- `FIID`: ID de la API
- `FCJSON`: JSON con la configuración de la API

#### Ejemplo de FCJSON

```json
{
  "idAPI": "104",
  "nombre": "ApiRestAdmonFacturacionGTITK",
  "descripcion": "Esta api nos proporcionará la administración de los recursos para facturación de Guatemala",
  "puerto": "6088",
  "estatus": "1",
  "estatusDescripcion": "API Activa"
}
```

## Stored Procedures Requeridas

### sp_GetApiConfigById
Obtiene la configuración de una API por su ID.

### sp_ValidateUserCredentials
Valida las credenciales del usuario contra la base de datos.

## Arquitectura

- **Controllers**: Contienen solo las funciones del endpoint
- **Services**: Contienen la lógica de negocio
- **Repositories**: Contienen las llamadas a stored procedures
- **Interfaces**: Definen los contratos para los servicios y repositorios

## Dependencias

- Microsoft.AspNetCore.Authentication.JwtBearer
- System.Data.SqlClient
- Microsoft.EntityFrameworkCore.SqlServer

## Notas Importantes

1. **Cambiar la cadena de conexión** en `appsettings.json`
2. **Configurar una clave secreta segura** para JWT
3. **Implementar la lógica de validación** en `sp_ValidateUserCredentials`
4. **Considerar hashear las contraseñas** en la base de datos
5. **Configurar CORS** si es necesario para aplicaciones web
