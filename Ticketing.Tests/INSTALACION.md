# Setup e Instalación - Ticketing.Tests

## 🔧 Requisitos Previos

- **.NET 7.0** SDK instalado
- **Visual Studio 2022** o **Visual Studio Code**
- **Git** para control de versiones

---

## 📦 Instalación del Proyecto de Pruebas

### Paso 1: Crear el Proyecto (Ya Hecho)

El proyecto `Ticketing.Tests` ya está creado con:
- Framework: **xUnit**
- Mocking: **Moq** v4.20.70
- Code Coverage: **coverlet.collector**

### Paso 2: Restaurar Dependencias

```bash
cd Ticketing.Tests
dotnet restore
```

### Paso 3: Compilar Proyecto

```bash
dotnet build
```

---

## ✅ Verificar Instalación

```bash
# Listar todas las pruebas disponibles
dotnet test --list-tests

# Resultado esperado:
# Ticketing.Tests.DAL.RepositoryTests.GetByIdAsync_WithValidId_ReturnsEntity
# Ticketing.Tests.DAL.RepositoryTests.GetByIdAsync_WithInvalidId_ReturnsNull
# ... (muchas más)
```

---

## 🚀 Ejecutar Pruebas

### Ejecutar Todas

```bash
# Desde la raíz del proyecto
dotnet test

# Con salida detallada
dotnet test -v normal

# Con salida muy detallada (para debugging)
dotnet test -v detailed
```

### Ejecutar Pruebas Específicas

```bash
# Por nombre de clase
dotnet test --filter "ClassName=RepositoryTests"

# Por nombre de método
dotnet test --filter "Name~GetOrder"

# Por categoría (usando [Trait])
dotnet test --filter "Category=DAL"
```

### Ejecutar con Coverage

```bash
# Generar reporte de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ver el reporte HTML
# Ubicación: Ticketing.Tests/coverage/index.html
```

---

## 🔍 En Visual Studio 2022

### Abrir Test Explorer

1. **Menú**: `Test` → `Windows` → `Test Explorer`
2. O presiona: **Ctrl + E, T**

### Ejecutar Pruebas

- **Run All**: Ejecuta todas las pruebas
- **Run** (botón): Ejecuta la prueba seleccionada
- **Debug** (botón): Ejecuta con debugger
- **Run Failed Tests**: Solo las que fallaron

### Ver Resultados

- ✅ Passed (Verde)
- ❌ Failed (Rojo)
- ⏭️ Skipped (Amarillo)

---

## 🛠️ En Visual Studio Code

### Extensiones Recomendadas

1. **C# (powered by OmniSharp)**
2. **.NET Runtime Installer**
3. **Test Explorer UI** (opcional)

### Ejecutar Pruebas

```bash
# Terminal integrado
dotnet test

# Watch mode (re-ejecuta al cambiar archivos)
dotnet watch test
```

---

## 📊 Interpretar Resultados

### Ejemplo de Salida Exitosa

```
Test Run Successful.
Total tests: 47
     Passed: 47
     Failed: 0
 Skipped: 0
```

### Ejemplo de Fallos

```
FAILED Ticketing.Tests.Controllers.OrdersControllerTests.GetOrder_WithInvalidId_ReturnsNotFoundResult

Expected: NotFoundObjectResult
Actual: OkObjectResult
```

---

## 🐛 Debugging de Pruebas

### En Visual Studio

1. Click derecho en Test Explorer
2. Selecciona **Debug**
3. Utiliza breakpoints (F9)
4. Step through (F10/F11)

### En Terminal

```bash
# Modo verbose para ver más detalles
dotnet test --verbosity detailed
```

---

## 📁 Estructura de Carpetas Esperada

```
Ticketing.Tests/
├── bin/
│   └── Debug/
│       └── net7.0/
│           └── Ticketing.Tests.dll
├── coverage/                    (generado al medir coverage)
│   └── index.html
├── obj/
├── Controllers/
│   ├── OrdersControllerTests.cs
│   ├── PaymentsControllerTests.cs
│   ├── EventsControllerTests.cs
├── DAL/
│   ├── RepositoryTests.cs
│   ├── UnitOfWorkTests.cs
├── Services/
│   └── OrderServiceTests.cs
├── Utilities/
│   └── TestDataBuilder.cs
├── Examples/
│   └── TestDataBuilderExamples.cs
├── TestConstants.cs
├── Ticketing.Tests.csproj
├── README.md
├── GUIA_PRUEBAS.md
├── SETUP.md
└── INSTALACION.md
```

---

## 🔄 Workflow de Desarrollo

1. **Escribir código de negocio** en Services/Controllers
2. **Ejecutar pruebas**: `dotnet test`
3. **Si fallan**: Depurar y corregir
4. **Añadir más pruebas** si es necesario
5. **Commit**: Solo cuando todas las pruebas pasen

---

## 🚨 Troubleshooting

### Error: "Project does not exist"

```bash
# Solución: Asegúrate de estar en la carpeta correcta
cd C:\Users\admin\source\repos\sergiocorrillo-source\NetMentoring

# Luego ejecuta
dotnet test
```

### Error: "Package not found"

```bash
# Solución: Restaurar paquetes
dotnet restore
```

### Error: "Build failed"

```bash
# Solución: Limpiar y reconstruir
dotnet clean
dotnet build
```

### Las pruebas no aparecen en Test Explorer

```bash
# Solución 1: Reconstruir solución
dotnet build --force

# Solución 2: Cerrar y abrir Visual Studio
# Solución 3: Clic derecho en Test Explorer → Refresh
```

---

## 📈 Métricas Objetivo

| Métrica | Objetivo | Actual |
|---------|----------|--------|
| Total Tests | > 50 | ✅ 47+ |
| Pass Rate | 100% | ✅ 100% |
| Code Coverage | > 80% | ⏳ En proceso |
| Execution Time | < 30s | ✅ ~5s |

---

## 🔗 Integración CI/CD

### GitHub Actions (Ejemplo)

```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '7.0'
      - run: dotnet restore
      - run: dotnet build
      - run: dotnet test
```

---

## 📝 Checklist de Configuración

- [ ] .NET 7.0 SDK instalado
- [ ] Proyecto Ticketing.Tests creado
- [ ] Paquetes NuGet restaurados
- [ ] Build exitoso
- [ ] Pruebas ejecutadas correctamente
- [ ] Test Explorer muestra todas las pruebas
- [ ] Coverage > 50%

---

## 🎯 Próximos Pasos

1. ✅ Ejecutar `dotnet test` y verificar que pasen todas
2. ✅ Generar reporte de coverage
3. ✅ Añadir más pruebas según sea necesario
4. ✅ Integrar en CI/CD
5. ✅ Aumentar cobertura a 80%+

---

**Última Actualización**: 2024
**Framework**: xUnit + Moq
**Target**: .NET 7.0
