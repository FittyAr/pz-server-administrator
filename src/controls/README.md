# Controls - Estructura de Controles de la Aplicación

Este directorio contiene todos los controles (componentes) de la interfaz de usuario de PZ Server Administrator. Cada control es una clase independiente que maneja una funcionalidad específica de la aplicación.

## Arquitectura de Controles

### Patrón de Diseño
Los controles siguen un patrón de diseño modular donde:
- **Cada control es una clase independiente** que encapsula su lógica y UI
- **Separación de responsabilidades** - cada control maneja una funcionalidad específica
- **Comunicación mediante callbacks** - los controles se comunican a través de funciones de callback
- **Estado centralizado** - el estado del servidor seleccionado se propaga a todos los controles

### Estructura Base de un Control
```python
class ExampleControl:
    def __init__(self):
        # Inicialización de estado
        self.current_server_id = None
        # Elementos de UI
        
    def set_server(self, server_id):
        """Establece el servidor actual para este control"""
        self.current_server_id = server_id
        # Actualizar contenido específico del servidor
        
    def build(self):
        """Construye y retorna la interfaz del control"""
        return ft.Container(...)  # Retorna componente Flet
```

## Controles Principales

### 🖥️ `server_control.py` - Control del Servidor
**Responsabilidad:** Gestión principal del servidor (inicio, parada, estado)
- Maneja el estado del servidor (corriendo/detenido)
- Integra el `ServerSelectorControl` para selección de servidores
- Utiliza `ServerManager` para operaciones del servidor
- **Dependencias:** `ServerSelectorControl`, `ServerManager`, `config_loader`

### 🎯 `server_selector_control.py` - Selector de Servidores
**Responsabilidad:** Selección y gestión de servidores detectados
- Escanea y detecta servidores automáticamente
- Permite marcar servidores como favoritos
- Maneja la selección activa de servidor
- **Características especiales:** Auto-detección, gestión de favoritos

### ⚙️ `config_manager_control.py` - Gestión de Configuración
**Responsabilidad:** Control principal para gestión de archivos de configuración del servidor
- Coordinación entre controles especializados de edición
- Gestión de tipos de archivo (INI, Lua, JSON)
- Interfaz unificada para edición de configuraciones
- **Tipos de archivo:** server_settings, sandbox_vars, spawn_regions, server_rules

### 👥 `players_control.py` - Gestión de Jugadores
**Responsabilidad:** Administración de jugadores del servidor
- Lista de jugadores conectados/desconectados
- Gestión de bans y kicks
- Estadísticas de jugadores
- **Estado:** Implementación básica con datos de ejemplo

### 📋 `logs_control.py` - Visualización de Logs
**Responsabilidad:** Monitoreo y visualización de logs del servidor
- Diferentes tipos de logs (servidor, chat, admin)
- Auto-refresh opcional
- Filtrado y búsqueda en logs
- **Estado:** Implementación básica con datos de ejemplo

### 💾 `backup_control.py` - Gestión de Respaldos
**Responsabilidad:** Creación y gestión de respaldos del servidor
- Respaldos manuales y automáticos
- Configuración de intervalos de respaldo
- Restauración de respaldos
- **Estado:** Implementación básica con datos de ejemplo

### 🔧 `app_config_control.py` - Configuración de la Aplicación
**Responsabilidad:** Gestión de la configuración global de la aplicación
- Editor JSON para configuración avanzada
- Gestión de secciones de configuración
- Validación y guardado de configuración

### 📁 `path_config_control.py` - Configuración de Rutas
**Responsabilidad:** Gestión de rutas y directorios del sistema
- Configuración de rutas de servidores
- Configuración de rutas de juego
- Validación de directorios

### 🎛️ Controles Auxiliares

#### `config_file_buttons_control.py`
**Responsabilidad:** Botones de acceso rápido a archivos de configuración
- Botones contextuales según el servidor seleccionado
- Estados habilitado/deshabilitado según disponibilidad de archivos
- Resaltado visual del archivo seleccionado

#### `edit_mode_control.py`
**Responsabilidad:** Control del modo de edición (simple/avanzado)
- Toggle entre modos de edición
- Comunicación con `ConfigManagerControl`

#### `ini_simple_editor_control.py`
**Responsabilidad:** Editor simplificado para archivos .ini
- Interfaz amigable para usuarios no técnicos
- Campos estructurados en lugar de texto plano

#### `advanced_text_editor_control.py`
**Responsabilidad:** Editor avanzado de texto plano para todos los tipos de archivo
- Edición de texto con sintaxis resaltada
- Plantillas por tipo de archivo
- Manejo de diferentes codificaciones
- Funciones de guardado y restauración

## Flujo de Comunicación

### 1. Selección de Servidor
```
ServerSelectorControl → MainLayout → Todos los controles
```
Cuando se selecciona un servidor, se propaga a todos los controles mediante `set_server(server_id)`

### 2. Inicialización de Favoritos
```
config_loader.initialize_selected_server() → ServerControl → ServerSelectorControl
```
Al iniciar la aplicación, se carga automáticamente el servidor favorito

### 3. Edición de Configuración (Refactorizada)
```
Usuario → ConfigFileButtonsControl → ConfigManagerControl → EditModeControl
                                                        ↓
                                 IniSimpleEditorControl ← → AdvancedTextEditorControl
```

### 4. Guardado de Configuración
```
ConfigManagerControl → Editor Específico → config_loader → Persistencia
```
Los cambios se guardan automáticamente en el archivo de configuración

## Dependencias Principales

- **Flet (ft):** Framework de UI para todos los controles
- **config_loader:** Gestión centralizada de configuración
- **ServerManager:** Operaciones del servidor (inicio/parada)
- **platform_utils:** Utilidades específicas del sistema operativo

## Patrones de Implementación

### Estado del Servidor
Todos los controles que dependen del servidor implementan:
```python
def set_server(self, server_id):
    self.current_server_id = server_id
    # Lógica específica de actualización
```

### Construcción de UI
Los controles complejos implementan:
```python
def build(self):
    return ft.Container(...)  # Componente principal
```

### Callbacks
Para comunicación entre controles:
```python
def __init__(self, on_change_callback=None):
    self.on_change = on_change_callback
```

## Extensibilidad

Para agregar un nuevo control:
1. Crear clase que herede el patrón base
2. Implementar `set_server()` si depende del servidor
3. Implementar `build()` para la UI
4. Registrar en `MainLayout` para integración
5. Agregar comunicación mediante callbacks si es necesario

### Patrón de Refactorización Aplicado

**Separación de Responsabilidades:**
- **ConfigManagerControl:** Coordinación y gestión general
- **EditModeControl:** Selección de modo de edición
- **IniSimpleEditorControl:** Edición simple específica para INI
- **AdvancedTextEditorControl:** Edición avanzada para todos los tipos
- **ConfigFileButtonsControl:** Navegación entre archivos

**Beneficios:**
- ✅ Código más modular y mantenible
- ✅ Responsabilidades claramente definidas
- ✅ Reutilización de componentes
- ✅ Facilidad para agregar nuevos tipos de editores

## Estado de Implementación

- ✅ **Completamente implementado:** ServerControl, ServerSelectorControl, ConfigManagerControl, AppConfigControl
- 🚧 **Implementación básica:** PlayersControl, LogsControl, BackupControl
- ⚙️ **Auxiliares:** ConfigFileButtonsControl, EditModeControl, IniSimpleEditorControl

Los controles marcados como "implementación básica" tienen la estructura y UI definida, pero requieren integración con las APIs reales del servidor de Project Zomboid.