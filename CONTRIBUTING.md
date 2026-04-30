# Contributing Guidelines

## Introducción
Gracias por contribuir al proyecto Ticket Sales System. Este documento resume las normas de estilo, el flujo de trabajo de ramas y las prácticas obligatorias para el desarrollo del sistema.

## Flujo de trabajo de ramas
- Rama principal: `main` (produción). Nunca hacer push directo a `main`.
- Features: ramas `feature/<descripcion>`.
- Hotfixes: ramas `hotfix/<descripcion>`.
- Pull Requests: Todo cambio debe ser mediante PR y pasar la revisión de al menos un revisor.

## Formato y estilo
Se debe seguir estrictamente el archivo `.editorconfig` en la raíz del repositorio. Resumen rápido:
- Sangría: 4 espacios
- Encabezados de llaves: nueva línea antes de `{` en métodos y tipos
- Nombres: PascalCase para tipos, métodos y propiedades; `_camelCase` para campos privados; camelCase para parámetros y variables locales.

## Estructura del proyecto
Seguir una arquitectura en capas mínima:
- `Domain` (modelos/entidades POCO)
- `Data` (DbContext, configuración EF Core, migraciones)
- `DAL` (repositorios, interfaces de acceso a datos)
- `Services` (lógica de negocio)
- `API` (Controllers / Endpoints)
- `Tests` (unit y integration)

## Entity Framework Core — Code-First (DAL)
- Usar EF Core Code-First para definir entidades y relaciones.
- Definir `DbContext` en la capa `Data` y configurar entidades con `IEntityTypeConfiguration<T>` en carpetas `Configurations`.
- Generar migraciones localmente y revisarlas antes de mergear.
- Evitar lazy-loading implícito en producción; para cargar relaciones usar `Include`/`ThenInclude` para consultas explícitas (eager loading) cuando se necesiten objetos relacionados en una sola consulta.

Ejemplos de uso obligatorio en DAL:
- Cargar un `Pedido`/`Offer` con todos sus `Ticket` asociados: Use `Include`.
- Cargar un `Event` con todos los `Seat` disponibles: Use `Include` y filtros adecuados.

## Transacciones
- Si una operación actualiza varias tablas relacionadas (ej. reservar asientos + crear pedido + generar pago), usar transacciones explícitas: `IDbContextTransaction` o `BeginTransactionAsync` y confirmar/revertir según corresponda.
- Manejar errores y rollback en caso de fallo parcial.

## Concurrencia y consistencia
- Para recursos con alta contención (asientos), usar control de concurrencia optimista (ej. `rowversion` / `byte[]` como `RowVersion`) y tratamiento de `DbUpdateConcurrencyException`.
- Implementar lógica de reintento cuando proceda.

## Estados de asiento
- Implementar el estado de asiento como un enum: `Available`, `Reserved`, `Sold`.
- La transición de estados debe ser controlada: reservar -> cancelar -> disponible; reservar -> confirmar -> sold.

## Transacciones externas (pagos)
- Integraciones con pasarelas de pago deben ser tratadas como operaciones idempotentes y coordinadas con la base de datos mediante transacciones y/o mecanismos de compensación.

## Testing
- Escribir pruebas unitarias para la lógica de negocio y pruebas de integración para DAL (usar in-memory provider o una instancia real de SQL para pruebas de integración).
- Cobertura mínima recomendada: 70% para lógica crítica.

## Pull Request checklist
- [ ] Código cumple `.editorconfig`.
- [ ] Pasan tests locales.
- [ ] Migraciones incluidas si hay cambios de esquema.
- [ ] Documentación actualizada (README/CHANGELOG si aplica).

Gracias por seguir estas normas.
