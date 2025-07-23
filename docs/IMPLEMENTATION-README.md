# Zomboid Server Manager

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
│   ├── appsettings.zsm.json            # Configuración personalizada
│   ├── ini-help.json                   # Descripciones de parámetros del INI
│   ├── sandboxvars-help.json           # Descripciones de parámetros SandboxVars
│   ├── spawnregions-help.json          # Descripciones de parámetros Spawn Regions
│   └── spawnpoints-help.json           # Descripciones de parámetros Spawn Points
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

## 📅 Plan de Desarrollo Modular

### 🔧 Módulo 1: Inicialización del Proyecto
- Crear solución Blazor Server en .NET 9
- Estructurar carpetas (`Pages`, `Services`, `Components`, `Models`, etc.)
- Crear layout principal: menú lateral y cabecera
- Configurar inyección de dependencias y carga de configuración desde `appsettings.zsm.json`

### 📁 Módulo 2: Sistema de Roles y Autenticación
- Definir modelo de usuario y estructura de roles: Invitado, Moderador, Administrador
- Crear servicio `AuthService`
- Implementar autenticación básica local basada en archivo JSON
- Validación de credenciales y control de permisos desde los roles definidos
- Mostrar contenido condicional según el rol

### 📂 Módulo 3: Explorador de Servidores
- Escaneo de carpeta de servidores al iniciar la app
- Cargar lista de servidores disponibles
- Detectar servidor activo y actualizar estado en `appsettings.zsm.json`
- Crear componente de selección de servidor activo

### ⚙️ Módulo 4: Configuración de Archivos del Servidor
Submódulos:
- **4.1 INI Config**
- **4.2 SandboxVars.lua**
- **4.3 SpawnRegion.lua**
- **4.4 SpawnPoints.lua**

Para cada uno:
- Implementar vista modo avanzado (texto editable)
- Implementar vista modo simple con controles individuales
- Detectar parámetros faltantes y mostrar tenues
- Agregar sistema de tooltips por parámetro (tooltipService)
- Leer definiciones desde JSONs de ayuda ubicados en `config/*.json`
- Guardar cambios con persistencia

### 🧠 Módulo 5: Sistema de Ayuda Contextual
- Componente Tooltip con descripciones de cada parámetro
- Archivos JSON:
  - `ini-help.json`
  - `sandboxvars-help.json`
  - `spawnregions-help.json`
  - `spawnpoints-help.json`
- Mostrar info al pasar el mouse sobre íconos de ayuda

### 🔌 Módulo 6: Consola RCON (modo Moderador/Admin)
- Submódulo: conexión a servidor (host/port/password)
- Submódulo: envío de comandos
- Submódulo: renderizado de consola tipo terminal
- Submódulo: historial de comandos enviados y respuestas
- Agregar controles de acceso según rol

### 🗃️ Módulo 7: Explorador de Base de Datos
- Conexión segura a archivo SQLite del servidor
- Lectura de tablas conocidas: `players`, `logs`, etc.
- Componente DataGrid con filtros y orden
- Control de acceso por rol (lectura para moderador, edición solo admin en el futuro)

### 🧰 Módulo 8: Configuración Global de la App
- Interfaz de configuración:
  - Ruta de servidores
  - Parámetros RCON
  - Cambio de modo de acceso
- Guardar cambios en `appsettings.zsm.json`
- Forzar recarga de app si se cambia la ruta de servidores

### 📦 Módulo 9: Gestión de Mods (futuro)
- Detectar mods activos
- Mostrar descripción, ID y estado
- Permitir activación/desactivación
- Posible integración con Steam Workshop

### 🧪 Módulo 10: Sistema de Logs y Debug
- Implementar `ILogger<T>` en todos los servicios
- Crear vista opcional para ver logs (solo para admin)

### 🧼 Módulo 11: UI y UX
- Theming opcional (oscuro/claro)
- Responsive design
- Componentes reutilizables para inputs, alerts, switches
- Accesibilidad básica

### 📚 Módulo 12: Documentación
- `README.md` principal
- Documentación en `docs/` para cada módulo
- Documentación XML en los componentes y servicios principales

### 🧪 Módulo 13: Testing
- Crear pruebas unitarias para servicios (AuthService, RCONService, ServerService)
- (Opcional) Pruebas de integración o e2e con Playwright para Blazor

---