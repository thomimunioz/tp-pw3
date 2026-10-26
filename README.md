# RuteaYa

Plataforma de transporte colaborativo: los conductores registran sus viajes habituales y la aplicación les sugiere envíos compatibles para llevar en el camino.

Trabajo práctico integrador de Programación Web III (UNLaM, 2.º cuatrimestre 2026).

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express (instancia `.\SQLEXPRESS`) u otro SQL Server
- Herramienta de Entity Framework: `dotnet tool install --global dotnet-ef`

## Estructura

| Proyecto | Contenido |
|---|---|
| `RuteaYa.Entidades` | Entidades, `RuteaYaContext` y migraciones |
| `RuteaYa.Logica` | Servicios con la lógica de negocio, mapas y optimizador |
| `RuteaYa.Web` | Aplicación MVC |
| `RuteaYa.Tests` | Tests unitarios (xUnit) |

## Cómo correrlo

Todos los comandos se ejecutan desde la carpeta raíz del repositorio.

1. **Configurar la base de datos.** Si tu SQL Server no es `.\SQLEXPRESS`, cambiá la cadena `RuteaYa` en `RuteaYa/RuteaYa.Web/appsettings.json`.
2. **Crear la base con las migraciones:**
   ```
   dotnet ef database update --project RuteaYa/RuteaYa.Entidades --startup-project RuteaYa/RuteaYa.Web
   ```
3. **Levantar la aplicación:**
   ```
   dotnet run --project RuteaYa/RuteaYa.Web --launch-profile https
   ```
   Abrir https://localhost:7221 y crear una cuenta desde la pantalla de registro.

## Tests

```
dotnet test RuteaYa/RuteaYa.slnx
```

## Servicio de mapas

Las direcciones se convierten en coordenadas con Nominatim (OpenStreetMap), que es gratuito y no requiere API key. Las distancias se calculan en línea recta con la fórmula de Haversine y un factor de 1,3 para aproximar el recorrido por calles.

Para trabajar sin conexión, se puede activar un geocodificador ficticio en `appsettings.json`:

```json
"Mapas": {
  "UsarGeocodificadorFalso": true
}
```
