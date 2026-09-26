# Reglas del Proyecto (pz-server-administrator)

Este documento define las directrices arquitectónicas, estándares de desarrollo y restricciones que cualquier agente o desarrollador debe seguir en este repositorio.

---

## 1. UI y Componentes Visuales

- **FluentUI Blazor**: Utiliza siempre que sea posible componentes de **FluentUI Blazor**.
  - Documentación de referencia: [FluentUI Blazor Documentation](https://www.fluentui-blazor.net/).
  - Evita reinventar componentes o recurrir a controles HTML estándar cuando exista un componente equivalente en FluentUI Blazor.
- **Restricción Estricta de JavaScript**:
  - **NO** utilices JavaScript bajo ninguna circunstancia por defecto.
  - Si en un caso extremo o excepcional se considera imprescindible el uso de JavaScript (o JSInterop), es **obligatorio informar previamente al usuario y solicitar confirmación explícita** antes de escribir o modificar código con JS.

---

## 2. Arquitectura y Modularidad

- **Modularidad y Extensibilidad**:
  - El sistema debe ser completamente **modular**.
  - Debe ser posible agregar nuevos componentes y funcionalidades sin modificar el código existente (Principio Abierto/Cerrado - Open/Closed).
  - Estructura modular con clara separación por funcionalidad y dominio.
- **Separación de Responsabilidades (MVVM / Capas)**:
  - La lógica de UI debe estar estrictamente separada de la lógica de negocio:
    - **`Components/`**: Únicamente presentación visual, plantillas Razor y enlace de datos (data-binding).
    - **`ViewModels/`**: Estado de presentación, gestión de eventos visuales y comandos de UI.
    - **`Services/`**: Lógica de negocio, reglas de dominio, comunicación externa y operaciones del sistema.
    - **`Models/`**: Clases de dominio, entidades y DTOs (Data Transfer Objects).
    - **`BackgroundServices/`**: Servicios de ejecución en segundo plano y workers periódicos.
- **Inyección de Dependencias**:
  - Todos los servicios y dependencias deben ser inyectados vía constructor (Constructor Injection).
  - Evitar el acoplamiento directo o instanciación manual (`new`) de servicios.

---

## 3. Configuración y Persistencia

- **Gestión de Configuración**:
  - La persistencia y lectura de configuración debe realizarse con `appsettings.json` ubicado bajo la carpeta `Resources/` (`Resources/appsettings.json`).
  - No dispersar configuraciones fijas o hardcodeadas en el código fuente.

---

## 4. Documentación y Calidad de Código

- **Documentación XML**:
  - Es mandatoria la documentación XML (`/// <summary>`) en **todos** los componentes, clases, métodos públicos, propiedades e interfaces.
  - Asegurar que los comentarios expliquen claramente el propósito y comportamiento de los parámetros y retornos.
- **Calidad de C# y .NET**:
  - Mantener un código limpio, fuertemente tipado, con adecuado manejo de nulabilidad y gestión robusta de excepciones.
