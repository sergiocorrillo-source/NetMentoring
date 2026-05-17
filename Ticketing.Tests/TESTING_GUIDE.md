# Pruebas Unitarias del Proyecto Ticketing - Resumen

## Descripción

Se han creado pruebas unitarias e integración completas para:
- **DAL (Data Access Layer)**: Repository e UnitOfWork
- **Servicios**: ReservationService
- **Controladores API**: EventsController, VenuesController

## Archivos Creados

### Tests Unitarios (con Moq)
1. **RepositoryTests.cs** - Pruebas del patrón Repository
   - GetByIdAsync
   - GetAllAsync
   - FindAsync con predicados
   - AddAsync, Update, Remove
   - GetWithIncludesAsync para relaciones

2. **UnitOfWorkTests.cs** - Pruebas del patrón UnitOfWork
   - Repository<T>()
   - SaveChangesAsync
   - ExecuteInTransactionAsync
   - Dispose
   - Manejo de excepciones

3. **ReservationServiceTests.cs** - Pruebas del servicio de reservas
   - ReserveSeatAsync (casos exitosos y errores)
   - ConfirmPurchaseAsync
   - Validaciones de asientos disponibles
   - Manejo de concurrencia

4. **EventsControllerTests.cs** - Tests de endpoint de eventos
   - POST /api/events/{eventId}/seats/{seatId}/reserve
   - Respuestas 201 Created, 400 BadRequest, 409 Conflict
   - Validación de datos de entrada

5. **VenuesControllerTests.cs** - Tests de endpoint de recintos
   - GET /api/venues
   - GET /api/venues/{venueId}/sections
   - Manejo de errores 500

### Tests de Integración (con EF Core en memoria)
1. **RepositoryIntegrationTests.cs**
   - Interacción real con DbContext en memoria
   - Pruebas de persistencia
   - Validación de relaciones entre entidades

2. **UnitOfWorkIntegrationTests.cs**
   - Transacciones con múltiples operaciones
   - Commit y Rollback
   - Consistency entre entidades relacionadas

## Herramientas Utilizadas

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.7.2" />
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.1" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## Cómo Ejecutar las Pruebas

### Opción 1: Desde Visual Studio
```
1. Abre Test Explorer (Ctrl + E, T)
2. Haz clic en "Ejecutar todas las pruebas"
3. Visualiza los resultados en la ventana Test Explorer
```

### Opción 2: Desde PowerShell/CMD
```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con verbosidad
dotnet test -v detailed

# Ejecutar solo un archivo de pruebas
dotnet test --filter "FullyQualifiedName~RepositoryTests"

# Ejecutar una prueba específica
dotnet test --filter "FullyQualifiedName~ReserveSeatAsync_WithAvailableSeat"

# Generar reporte de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutput=./coverage/ /p:CoverletOutputFormat=opencover
```

## Patrones y Mejores Prácticas

### 1. Patrón AAA (Arrange-Act-Assert)
```csharp
[Fact]
public async Task ReserveSeatAsync_WithAvailableSeat_CreatesTicket()
{
    // ARRANGE: Preparar mocks y datos de prueba
    var eventId = Guid.NewGuid();
    var seat = new Seat { Status = SeatStatus.Available };
    _mockSeatRepo.Setup(r => r.GetWithIncludesAsync(...))
        .ReturnsAsync(new List<Seat> { seat });

    // ACT: Ejecutar el método bajo prueba
    var ticketId = await _reservationService.ReserveSeatAsync(...);

    // ASSERT: Validar resultados
    ticketId.Should().NotBe(Guid.Empty);
}
```

### 2. Mocking con Moq
```csharp
// Setup: Configurar comportamiento del mock
_mockRepo.Setup(r => r.GetByIdAsync(id, default))
    .ReturnsAsync(customer);

// Verify: Verificar que se llamó al método
_mockRepo.Verify(r => r.GetByIdAsync(id, default), Times.Once);
```

### 3. Assertions Fluidas (FluentAssertions)
```csharp
result.Should().NotBeNull();
result?.Email.Should().Be("test@example.com");
tickets.Should().HaveCount(1);
ex.Message.Should().Contain("not found");
```

### 4. Isolamiento de Dependencias
```csharp
// Cada test tiene sus propios mocks
private readonly Mock<IUnitOfWork> _mockUnitOfWork;
private readonly Mock<IRepository<Seat>> _mockSeatRepo;
private readonly ReservationService _service;

public ReservationServiceTests()
{
    _mockUnitOfWork = new Mock<IUnitOfWork>();
    _mockSeatRepo = new Mock<IRepository<Seat>>();
    _service = new ReservationService(_mockUnitOfWork.Object);
}
```

## Casos de Prueba Cubiertos

### DAL
✅ CRUD completo (Create, Read, Update, Delete)
✅ Búsquedas con predicados LINQ
✅ Carga de relaciones con Include
✅ Transacciones
✅ Manejo de excepciones

### Servicios
✅ Flujo exitoso de reserva
✅ Validaciones de asiento (disponible, no existe)
✅ Confirmación de compra con cambio de estado
✅ Transacciones y rollback
✅ Concurrencia optimista

### API
✅ Respuestas HTTP correctas (200, 201, 400, 409, 500)
✅ Validación de parámetros
✅ Manejo de excepciones esperadas
✅ Formato correcto de datos en respuesta

## Estadísticas

- **Total de Pruebas Unitarias**: 15+
- **Total de Pruebas de Integración**: 8+
- **Métodos Cubiertos**: Repository, UnitOfWork, ReservationService, Controladores
- **Herramientas**: xUnit, Moq, FluentAssertions

## Próximas Mejoras

- [ ] Añadir tests de seguridad/autorización en controladores
- [ ] Pruebas de validación de DTOs
- [ ] Tests de rendimiento (benchmarks)
- [ ] Aumentar cobertura a otros controladores (Orders, Payments, Carts)
- [ ] Tests de configuración de dependencias (DI Container)
- [ ] Integración con CI/CD (GitHub Actions)

## Solución de Problemas

### Error: "Cannot use DbContext in memory"
**Solución**: Usar `UseInMemoryDatabase` en tests de integración:
```csharp
var options = new DbContextOptionsBuilder<TicketingDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
```

### Error: "Mocked method not returning value"
**Solución**: Asegurar que todo mock tenga setup:
```csharp
_mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
    .ReturnsAsync((Seat?)null); // Retorna null si no coincide
```

### Tests lentos
**Solución**: Usar mocks en lugar de BD real, mantener tests unitarios pequeños

## Referencia de Comandos

| Comando | Descripción |
|---------|-------------|
| `dotnet test` | Ejecutar todas las pruebas |
| `dotnet test -v detailed` | Tests con salida detallada |
| `dotnet test --filter "RepositoryTests"` | Ejecutar clase específica |
| `dotnet test --logger "console;verbosity=detailed"` | Logger detallado |

## Contacto y Soporte

Para preguntas sobre las pruebas, consulta la documentación oficial:
- xUnit: https://xunit.net/
- Moq: https://github.com/moq/moq
- FluentAssertions: https://fluentassertions.com/
