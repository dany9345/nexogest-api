# NexoGest API

API REST local de gestión empresarial realizada con ASP.NET Core 8, Entity Framework Core 8 y SQLite.

Incluye empleados, gastos, inventario, movimientos de stock y consultas agregadas para el dashboard.

## Requisitos

- .NET 8 SDK

## Arranque

```powershell
dotnet restore
dotnet run --project .\NexoGest.Api
```

La API escucha en `http://localhost:5080`; Swagger está en `http://localhost:5080/swagger`.
La base de datos SQLite se crea dentro de `NexoGest.Api/data/` aplicando las migraciones al iniciar, con datos de demostración.

## Pruebas

```powershell
dotnet test
```

Proyecto de demostración local; no incluye autenticación ni configuración de despliegue a producción.
