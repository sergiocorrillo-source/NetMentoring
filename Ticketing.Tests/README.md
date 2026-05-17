# Ticketing.Tests - Pruebas Unitarias

Este proyecto contiene pruebas unitarias para los componentes del sistema Ticketing usando **xUnit** y **Moq**.

## 📋 Estructura

```
Ticketing.Tests/
├── DAL/
│   ├── RepositoryTests.cs          # Pruebas para IRepository<T>
│   └── UnitOfWorkTests.cs          # Pruebas para IUnitOfWork
├── Services/
│   ├── OrderServiceTests.cs        # Pruebas para OrderService
│   └── PaymentServiceTests.cs      # Pruebas para PaymentService (futuro)
├── Controllers/
│   ├── OrdersControllerTests.cs    # Pruebas para OrdersController
│   ├── PaymentsControllerTests.cs  # Pruebas para PaymentsController
│   ├── EventsControllerTests.cs    # Pruebas para EventsController
│   └── CartsControllerTests.cs     # Pruebas para CartsController (futuro)
└── README.md
```

## 🧪 Categorías de Pruebas

### 1. **DAL Tests** (Data Access Layer)

#### RepositoryTests
- ✅ `GetByIdAsync_WithValidId_ReturnsEntity` - Obtiene entidad por ID válido
- ✅ `GetByIdAsync_WithInvalidId_ReturnsNull` - Retorna nulo con ID inválido
- ✅ `GetAllAsync_ReturnsAllEntities` - Obtiene todas las entidades
- ✅ `FindAsync_WithPredicate_ReturnsFilteredEntities` - Filtra por predicado
- ✅ `AddAsync_WithValidEntity_CallsRepository` - Añade entidad válida
- ✅ `Update_WithValidEntity_CallsRepository` - Actualiza entidad
- ✅ `Remove_WithValidEntity_CallsRepository` - Elimina entidad

#### UnitOfWorkTests
- ✅ `Repository_WithValidType_ReturnsRepository` - Obtiene repositorio por tipo
- ✅ `SaveChangesAsync_CallsCommit` - Guarda cambios
- ✅ `ExecuteInTransactionAsync_ExecutesAction` - Ejecuta en transacción
- ✅ `Dispose_DisposesResources` - Libera recursos

### 2. **Services Tests**

#### OrderServiceTests
- ✅ `GetOrderAsync_WithValidId_ReturnsOrder` - Obtiene orden por ID
- ✅ `GetOrderAsync_WithInvalidId_ReturnsNull` - Retorna nulo con ID inválido
- ✅ `CreateOrderAsync_WithValidData_CreatesOrder` - Crea orden válida
- ✅ `CreateOrderAsync_WithInvalidCustomer_ThrowsException` - Error con cliente inválido
- ✅ `UpdateOrderStatusAsync_WithValidStatus_UpdatesOrder` - Actualiza estado válido
- ✅ `UpdateOrderStatusAsync_WithInvalidStatus_ThrowsException` - Error con estado inválido
- ✅ `CancelOrderAsync_CancelsOrder` - Cancela orden

### 3. **Controllers Tests**

#### OrdersControllerTests
- ✅ `GetOrder_WithValidId_ReturnsOkResult` - GET con ID válido
- ✅ `GetOrder_WithInvalidId_ReturnsNotFoundResult` - GET retorna 404
- ✅ `GetOrdersByCustomer_WithValidCustomerId_ReturnsOkResult` - GET órdenes por cliente
- ✅ `GetOrdersByEvent_WithValidEventId_ReturnsOkResult` - GET órdenes por evento
- ✅ `CreateOrder_WithValidData_ReturnsCreatedAtActionResult` - POST crea orden
- ✅ `CreateOrder_WithNullData_ReturnsBadRequest` - POST rechaza datos nulos
- ✅ `CreateOrder_WithEmptyCustomerId_ReturnsBadRequest` - POST rechaza CustomerId vacío
- ✅ `CreateOrder_WithZeroAmount_ReturnsBadRequest` - POST rechaza amount = 0
- ✅ `UpdateOrderStatus_WithValidStatus_ReturnsOkResult` - PUT actualiza estado
- ✅ `UpdateOrderStatus_WithNullDto_ReturnsBadRequest` - PUT rechaza DTO nulo
- ✅ `UpdateOrderStatus_WithInvalidStatus_ReturnsBadRequest` - PUT rechaza estado inválido
- ✅ `CancelOrder_WithValidId_ReturnsNoContent` - DELETE cancela orden
- ✅ `CancelOrder_WithInvalidId_ReturnsBadRequest` - DELETE retorna error

#### PaymentsControllerTests
- ✅ `GetPayment_WithValidId_ReturnsOkResult` - GET pago válido
- ✅ `GetPayment_WithInvalidId_ReturnsNotFoundResult` - GET retorna 404
- ✅ `CompletePayment_WithValidId_ReturnsOkResult` - POST completa pago
- ✅ `CompletePayment_WithInvalidId_ReturnsBadRequest` - POST error con ID inválido

#### EventsControllerTests
- ✅ `GetEvent_WithValidId_ReturnsOkResult` - GET evento válido
- ✅ `GetEvent_WithInvalidId_ReturnsNotFoundResult` - GET retorna 404
- ✅ `GetAllEvents_ReturnsOkResult` - GET todos los eventos

## 🚀 Cómo Ejecutar las Pruebas

### Desde la línea de comandos:

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas de un proyecto específico
dotnet test Ticketing.Tests

# Ejecutar pruebas con salida verbose
dotnet test --verbosity normal

# Ejecutar pruebas con cobertura
dotnet test /p:CollectCoverage=true
```

### Desde Visual Studio:

1. Abre el **Test Explorer** (Test → Windows → Test Explorer)
2. Haz clic en **Run All Tests** o filtra por categoría
3. Visualiza los resultados en el panel de resultados

## 📦 Dependencias

- **xUnit** (2.6.2) - Framework de pruebas
- **Moq** (4.20.70) - Mocking framework
- **Microsoft.NET.Test.Sdk** (17.7.0) - SDK de pruebas .NET
- **coverlet.collector** (6.0.0) - Code coverage

## 🎯 Patrones de Prueba Usados

### AAA (Arrange-Act-Assert)

Todas las pruebas siguen el patrón AAA:

```csharp
[Fact]
public async Task TestMethod_Scenario_ExpectedResult()
{
    // Arrange - Preparar datos y mocks
    var mockService = new Mock<IService>();
    mockService.Setup(...).Returns(...);

    // Act - Ejecutar la lógica
    var result = await sut.Method(...);

    // Assert - Verificar resultados
    Assert.NotNull(result);
    mockService.Verify(...);
}
```

### Naming Convention

- **Test Method**: `MethodName_Scenario_ExpectedResult`
- **Example**: `CreateOrder_WithValidData_ReturnsCreatedAtActionResult`

## 🔍 Cobertura de Código

Las pruebas cubren:

- ✅ **Happy Path**: Casos de éxito normal
- ✅ **Error Cases**: Excepciones y validaciones
- ✅ **Edge Cases**: Valores límite (nulos, vacíos, etc.)
- ✅ **Integration**: Interacción entre capas (Mock)

## 📝 Notas Importantes

1. **Mocking**: Se usa `Moq` para aislar dependencias
2. **Async/Await**: Las pruebas soportan métodos asincronos
3. **Data Validation**: Se validan inputs en controladores
4. **Status Codes**: Se verifican códigos HTTP correctos (200, 201, 204, 400, 404, 500)

## 🔄 Próximas Mejoras

- [ ] Pruebas para CartsController
- [ ] Pruebas para VenuesController
- [ ] Pruebas de integración con DbContext
- [ ] Aumento de cobertura a 80%+
- [ ] Tests de autenticación/autorización

## 📧 Contacto

Para más información sobre las pruebas, revisa la documentación del proyecto principal.
