# Guía de Pruebas Unitarias - Ticketing.Tests

## 📚 Introducción

Este documento proporciona una guía completa para entender, ejecutar y crear pruebas unitarias en el proyecto Ticketing usando **xUnit** y **Moq**.

---

## 🏗️ Estructura del Proyecto

```
Ticketing.Tests/
│
├── DAL/                           # Pruebas de Data Access Layer
│   ├── RepositoryTests.cs         # IRepository<T> interface
│   └── UnitOfWorkTests.cs         # IUnitOfWork interface
│
├── Services/                      # Pruebas de servicios de negocio
│   └── OrderServiceTests.cs       # OrderService
│
├── Controllers/                   # Pruebas de endpoints API
│   ├── OrdersControllerTests.cs   # GET, POST, PUT, DELETE órdenes
│   ├── PaymentsControllerTests.cs # Pagos
│   └── EventsControllerTests.cs   # Eventos
│
├── Utilities/                     # Utilitarios de prueba
│   └── TestDataBuilder.cs         # Builder para objetos de prueba
│
├── TestConstants.cs               # Constantes globales
└── README.md                      # Documentación
```

---

## 🧪 Qué Son las Pruebas Unitarias

Una **prueba unitaria** verifica que una unidad de código (función, método, clase) funciona correctamente de forma aislada.

### Principios SOLID para Pruebas:
- **S**ingle Responsibility: Una prueba = Una funcionalidad
- **O**pen/Closed: Extensible sin modificar
- **L**iskov Substitution: Mocks reemplazan dependencias
- **I**nterface Segregation: Usar interfaces específicas
- **D**ependency Inversion: Inyectar dependencias

---

## 🧩 Patrón AAA (Arrange-Act-Assert)

Todas las pruebas siguen este patrón:

```csharp
[Fact]  // o [Theory] para múltiples datos
public async Task GetOrder_WithValidId_ReturnsOkResult()
{
    // ARRANGE: Preparar datos y mocks
    var orderId = Guid.NewGuid();
    var orderDto = new OrderDto { OrderId = orderId, ... };

    _mockOrderService
        .Setup(s => s.GetOrderAsync(orderId))
        .ReturnsAsync(orderDto);

    // ACT: Ejecutar el método a probar
    var result = await _controller.GetOrder(orderId);

    // ASSERT: Verificar el resultado
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Equal(200, okResult.StatusCode);
}
```

---

## 🎯 Tipos de Pruebas Incluidas

### 1️⃣ Pruebas DAL (Repository Pattern)

**Qué prueba:** Operaciones CRUD de acceso a datos

```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsEntity()
{
    // Usa Mock<IRepository<T>> para simular el repositorio
}
```

**Casos cubiertos:**
- ✅ Obtener por ID válido → retorna entidad
- ✅ Obtener por ID inválido → retorna null
- ✅ Obtener todos → retorna lista completa
- ✅ Filtrar con predicado → retorna entidades filtradas
- ✅ Añadir entidad → llama al repositorio
- ✅ Actualizar → verifica update
- ✅ Eliminar → verifica remove

### 2️⃣ Pruebas de Servicios

**Qué prueba:** Lógica de negocio con dependencias mockeadas

```csharp
public async Task CreateOrderAsync_WithValidData_CreatesOrder()
{
    // Mock del UnitOfWork que usa el servicio
    // Verifica que la lógica de negocio funciona correctamente
}
```

**Casos cubiertos:**
- ✅ Crear orden con datos válidos
- ✅ Validar que customer existe
- ✅ Validar que event existe
- ✅ Manejo de errores
- ✅ Actualizar estados con lógica de negocio
- ✅ Cancelar orden y liberar asientos

### 3️⃣ Pruebas de Controladores (API Endpoints)

**Qué prueba:** Respuestas HTTP y validación de entrada

```csharp
[Fact]
public async Task GetOrder_WithValidId_ReturnsOkResult()
{
    // Mock del servicio
    // Verifica código HTTP (200, 201, 400, 404, etc.)
    // Verifica estructura de respuesta
}
```

**Casos cubiertos por endpoint:**

| Método | Endpoint | Tests |
|--------|----------|-------|
| GET | `/api/orders/{id}` | 2 (válido, inválido) |
| GET | `/api/orders/customer/{id}` | 1 (retorna lista) |
| GET | `/api/orders/event/{id}` | 1 (retorna lista) |
| POST | `/api/orders` | 5 (válido, nulo, campos vacíos, etc.) |
| PUT | `/api/orders/{id}/status` | 3 (válido, nulo, inválido) |
| DELETE | `/api/orders/{id}` | 2 (válido, inválido) |

---

## 🚀 Ejecutar las Pruebas

### Opción 1: Terminal (CMD/PowerShell)

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas específicas
dotnet test --filter MethodName

# Ejecutar con verbosidad
dotnet test --verbosity detailed

# Ejecutar y generar reporte de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Opción 2: Visual Studio

1. **Test → Windows → Test Explorer** (o Ctrl+E, T)
2. **Buscar pruebas** por nombre o clase
3. **Run** (▶) o **Debug** (🐛)
4. Ver resultados en el panel

### Opción 3: Rider/Visual Studio Code

```bash
# Watch mode (re-ejecuta al cambiar archivos)
dotnet watch test
```

---

## 🔍 Entender el Mocking con Moq

### ¿Qué es Moq?

Moq es un framework que permite crear **objetos simulados (mocks)** para aislar la unidad bajo prueba.

### Ejemplo Básico:

```csharp
// 1. Crear un mock
var mockService = new Mock<IOrderService>();

// 2. Configurar comportamiento
mockService
    .Setup(s => s.GetOrderAsync(It.IsAny<Guid>()))  // Cualquier Guid
    .ReturnsAsync(new OrderDto { /* ... */ });

// 3. Usar en la prueba
var result = await _controller.GetOrder(someGuid);

// 4. Verificar que fue llamado
mockService.Verify(s => s.GetOrderAsync(It.IsAny<Guid>()), Times.Once);
```

### Métodos Importantes de Moq:

| Método | Descripción |
|--------|-------------|
| `Setup()` | Configura lo que debe hacer el mock |
| `Returns()` | Retorna un valor sincrónico |
| `ReturnsAsync()` | Retorna un Task (asincrónico) |
| `Throws()` | Lanza una excepción |
| `Verify()` | Verifica que fue llamado |
| `It.IsAny<T>()` | Cualquier valor del tipo T |
| `Times.Once` | Fue llamado exactamente 1 vez |
| `Times.Never` | Nunca fue llamado |
| `Times.AtLeast(n)` | Fue llamado al menos n veces |

---

## 📊 Cobertura de Código

### Verificar cobertura:

```bash
# Generar reporte de cobertura
dotnet test /p:CollectCoverage=true

# Ver archivo index.html en:
# Ticketing.Tests/coverage/index.html
```

### Meta: **80%+ cobertura**

La cobertura mide qué porcentaje de líneas de código son ejecutadas por pruebas.

---

## ✅ Checklist para Escribir Pruebas

- [ ] **Nombre descriptivo**: `MethodName_Scenario_ExpectedResult`
- [ ] **Aislamiento**: Solo testa una cosa
- [ ] **Mocks**: Todas las dependencias mockeadas
- [ ] **Arrange**: Datos realistas
- [ ] **Act**: Una sola acción
- [ ] **Assert**: Verificaciones específicas
- [ ] **Determinísticas**: Siempre mismo resultado
- [ ] **Rápidas**: < 1 segundo por prueba
- [ ] **Mantenibles**: Código claro y legible

---

## 🐛 Debugging de Pruebas

### En Visual Studio:

1. **Click derecho** en la prueba → **Debug**
2. Usa **breakpoints** (F9)
3. **Step over** (F10) o **step into** (F11)
4. Inspecciona variables en la **ventana Debug**

### En Terminal:

```bash
dotnet test --verbosity detailed
```

---

## 📝 Ejemplos de Pruebas

### Ejemplo 1: GET - Caso Exitoso

```csharp
[Fact]
public async Task GetOrder_WithValidId_ReturnsOkResult()
{
    // Arrange
    var orderId = Guid.NewGuid();
    var orderDto = new OrderDto { OrderId = orderId };

    _mockService
        .Setup(s => s.GetOrderAsync(orderId))
        .ReturnsAsync(orderDto);

    // Act
    var result = await _controller.GetOrder(orderId);

    // Assert
    Assert.IsType<OkObjectResult>(result.Result);
}
```

### Ejemplo 2: GET - Caso Error

```csharp
[Fact]
public async Task GetOrder_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    var invalidId = Guid.NewGuid();

    _mockService
        .Setup(s => s.GetOrderAsync(invalidId))
        .ReturnsAsync((OrderDto?)null);

    // Act
    var result = await _controller.GetOrder(invalidId);

    // Assert
    Assert.IsType<NotFoundObjectResult>(result.Result);
}
```

### Ejemplo 3: POST - Validación

```csharp
[Fact]
public async Task CreateOrder_WithEmptyCustomerId_ReturnsBadRequest()
{
    // Arrange
    var dto = new CreateOrderDto { CustomerId = Guid.Empty };

    // Act
    var result = await _controller.CreateOrder(dto);

    // Assert
    Assert.IsType<BadRequestObjectResult>(result.Result);
}
```

### Ejemplo 4: Excepción Esperada

```csharp
[Fact]
public async Task CreateOrderAsync_WithInvalidCustomer_ThrowsException()
{
    // Arrange
    var dto = new CreateOrderDto { CustomerId = Guid.NewGuid() };
    _mockService.Setup(...).ThrowsAsync(new InvalidOperationException());

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _service.CreateOrderAsync(dto));
}
```

---

## 🤝 Buenas Prácticas

✅ **DO:**
- Usar nombres descriptivos
- Una aserción principal (+ setup)
- Pruebas independientes
- Usar builders para objetos complejos
- Verificar excepciones esperadas

❌ **DON'T:**
- Pruebas interdependientes
- Múltiples acciones en una prueba
- Dependencias reales (HTTP, DB)
- Tests aleatorios/no determinísticos
- Ignorar pruebas fallidas

---

## 📚 Recursos Adicionales

- **xUnit Docs**: https://xunit.net/
- **Moq Wiki**: https://github.com/moq/moq4/wiki
- **Unit Testing Best Practices**: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

---

## 🚦 Próximos Pasos

1. ✅ Ejecutar todas las pruebas
2. ✅ Analizar la cobertura
3. ✅ Crear más pruebas para casos edge
4. ✅ Integrar en CI/CD
5. ✅ Mantener cobertura > 80%

---

**Last Updated**: 2024
**Framework**: xUnit + Moq
**Target**: .NET 7.0
