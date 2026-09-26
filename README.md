# Project Zomboid Server Administrator

⚠️ **PROYECTO EN DESARROLLO ACTIVO** ⚠️

**IMPORTANTE**: Este proyecto está actualmente en desarrollo y **NO ES UTILIZABLE** para uso en producción. La aplicación está siendo desarrollada y probada. Este README será actualizado cuando el proyecto esté listo para uso general.


# Zomboid Server Manager - Blazor .NET 10

## Descripción general

Aplicación web construida con **Blazor Server en .NET 10** para administrar servidores dedicados del juego **Project Zomboid (versión 41)**. La app permitirá seleccionar, visualizar y editar la configuración de servidores, ejecutar comandos por RCON, consultar estadísticas desde la base de datos SQLite del juego y, en el futuro, gestionar mods.

---

## 🎯 Objetivos

- Permitir a administradores de servidores Zomboid configurar y administrar múltiples instancias de servidor desde una interfaz moderna.
- Simplificar la edición de archivos de configuración `.ini` y `.lua` mediante UI amigable y/o edición directa de texto.
- Habilitar control remoto del servidor mediante RCON.
- Visualizar estadísticas e información persistente almacenada en SQLite.
- Preparar la base para futuras extensiones como gestión de mods.

---

## 🗂️ Estructura del Proyecto

```plaintext
pz-server-administrator/
│
├── pz-server-administrator/               # Proyecto principal Blazor Server
│   ├── Pages/
│   ├── Components/
│   ├── Services/
│   ├── Models/
│   ├── Helpers/
│   ├── Data/
│   ├── ViewModels/
│   ├── App.razor
│   └── Program.cs
│
├── config/                             # Configuración propia de la app
│   └── appsettings.zsm.json            # Configuración personalizada
│
├── docs/                               # Documentación detallada
│   ├── README.md                       # (Este archivo)
│   ├── server-config/README.md         # Explicación de configuración ini/lua
│   ├── rcon/README.md                  # Documentación del módulo RCON
│   ├── database/README.md              # Exploración de SQLite
│   ├── mods/README.md                  # Plan futuro para la gestión de mods
│   └── ui-layout/README.md             # Diseño de la interfaz
│
├── wwwroot/
│
└── pz-server-administrator.sln
```

---

## 🧠 Comportamiento de la App

### Al iniciar la aplicación:

- Se escaneará una **carpeta configurada** que contiene las carpetas de los servidores.
- Se listarán todos los servidores configurados.
- Se seleccionará automáticamente como *activo* el servidor marcado previamente (configuración persistente).
- Si no hay uno seleccionado, las secciones del menú aparecerán deshabilitadas hasta que el usuario seleccione uno.

---

## 🖥️ Layout de la App

- **Menú lateral izquierdo** con secciones y submenús desplegables.
- **Cabecera** con:
  - Nombre de la app.
  - Sección activa.
  - Nombre del servidor actualmente administrado.

### Modos de acceso:

- **Invitado**: Solo lectura de configuración y estadísticas, no requiere autenticación.
- **Moderador**: Acceso limitado a funciones de administración como RCON, bloqueo y desbloqueo de jugadores. No puede modificar archivos de configuración o cambiar parámetros globales.
- **Administrador**: Acceso total, modificación de configuraciones, control RCON, configuración global.

> ⚠️ Todos los datos de roles, permisos y credenciales serán almacenados y gestionados desde el archivo `config/appsettings.zsm.json`.

---

## 📂 Menús de la Aplicación

### `Server Config`

Submenús:
- `Ini Config`
- `Sandbox Vars`
- `Spawn Region`
- `Spawn Points`

#### Comportamiento:

- Cada archivo se puede editar en dos modos:
  - **Modo avanzado**: Edición directa del archivo de texto.
  - **Modo simple**: Interfaz con controles por parámetro.
- Parámetros no presentes en el archivo se mostrarán con **texto tenue** y serán editables.
- Al editar, el texto se mostrará con color normal, indicando que el parámetro fue agregado.
- Cada parámetro contará con un **icono de ayuda contextual**, que mostrará una descripción emergente.

---

## ⚙️ Módulo RCON

> **Estado**: En planificación.

### Funcionalidad esperada:

- Conexión al servidor por RCON (host, puerto, contraseña configurables).
- Envío de comandos como:
  - Estado del servidor.
  - Mensajes a jugadores.
  - Reinicio/apagado del servidor.
  - Administración de usuarios.
- Consola embebida con historial de comandos.
- Visualización de logs de respuesta.

> 🔧 Investigar compatibilidad con librerías RCON existentes o implementación manual del protocolo utilizado por Project Zomboid.

---

## 🗃️ Explorador de Base de Datos SQLite

### Funcionalidad:

- Conexión de solo lectura inicial al archivo `.db` generado por el servidor.
- Vistas planificadas:
  - Información de jugadores (posición, nombre, inventario, conexión).
  - Estadísticas generales del servidor.
  - Log de eventos.
- Evaluar posibilidad de escritura o edición en futuras versiones.

> 🔍 Se requiere análisis del esquema del archivo `.sqlite` para determinar qué datos se pueden utilizar y cómo interpretarlos.

---

## 📦 Futuro: Gestión de Mods

Sección a implementar una vez que la funcionalidad básica esté completa.

### Funcionalidad planificada:

- Listado de mods activos en el servidor.
- Información detallada de cada mod.
- Activación / desactivación.
- Posible integración con Steam Workshop.

---

## ⚙️ Configuración de la App

### Sección visible solo para administradores.

Permite:

- Cambiar la ubicación de la carpeta de servidores.
  - Este cambio reinicia el estado de la app y recarga todos los datos.
- Configurar parámetros de conexión RCON.
- Alternar entre modo administrador, moderador e invitado.
- Configurar apariencia visual o idioma (si se desea incluir más adelante).

---

## 🧱 Buenas prácticas técnicas

- Estructura **modular** con separación por funcionalidad.
- Uso de:
  - `IConfiguration`
  - `ILogger<T>`
  - `Dependency Injection`
- Servicios inyectados vía constructor.
- Documentación XML en todos los componentes.
- Lógica de UI separada de la lógica de negocio (por `Services`, `Models`, `ViewModels`).
- Persistencia de configuración con `appsettings.zsm.json` bajo la carpeta `config/`.

---

## 🛠️ Requisitos técnicos

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Blazor Server
- Visual Studio 2022 o Visual Studio Code
- Permisos de lectura/escritura sobre la carpeta donde se almacenan los servidores
- Acceso al puerto RCON (si el servidor lo tiene habilitado)

---

## ✅ Próximos pasos

1. Crear solución y proyecto base Blazor Server.
2. Implementar escaneo de carpeta de servidores.
3. Crear layout base: menú lateral y cabecera.
4. Implementar módulos de configuración (modo simple y avanzado).
5. Añadir sistema de autenticación básica con tres niveles: invitado, moderador y administrador.
6. Desarrollar el módulo RCON (investigación y conexión).
7. Leer y presentar datos del archivo SQLite.
8. Documentar cada módulo en `docs/`.
9. Implementar sección de configuración general.
10. Planificar e integrar el módulo de mods en una fase futura.

---
