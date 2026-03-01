# Plan de Expansión: Agente de IA - Inteligencia Profunda y Adquisición de Datos

Este plan expande la capacidad del Agente de IA de un "analista de metadatos" a un "experto en diagnóstico profundo" con ojos en el sistema de archivos y los logs en tiempo real.

## User Review Required

> [!IMPORTANT]
> El sistema de IA tendrá la capacidad de **leer el contenido de archivos LUA y TXT** dentro de las carpetas de los mods para detectar conflictos de sobrescritura de funciones.
> ¿Quieres que el Agente tenga permiso para leer cualquier archivo dentro del directorio del servidor o solo archivos específicos relacionados con mods?

## Proposed Changes

### 1. Engine: Context Discovery (MCP-Style)
Daremos a la IA la capacidad de "pedir" más datos si el contexto inicial no es suficiente.

#### [MODIFY] [AiService.cs](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Services/AiService.cs)
- Implementar un bucle de razonamiento de hasta 2-3 pasos.
- La IA puede devolver un comando especial `RequestFile(path)` o `RequestLogTail(lines)`.
- El servicio ejecutará estas peticiones y re-enviará el prompt con la nueva información.

### 2. Conflict Matrix: File-Level Scan
Detección de conflictos de archivos que el juego no reporta explícitamente pero causan errores lógicos.

#### [MODIFY] [ModDiscoveryService.cs](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Services/ModDiscoveryService.cs)
- `ScanDeepFileConflictAsync()`: Escanea todos los nombres de archivos dentro de los mods activos.
- Generar un reporte de "Sobrescritura de Archivos" (ej: dos mods cargando el mismo `ISUI/InventoryPane.lua`).

### 3. Monitoring: Real-time Log Observer
Un servicio en segundo plano que actúa como "Ojos de la IA".

#### [NEW] [PzLogObserver.cs](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Services/PzLogObserver.cs)
- `BackgroundService` que realiza un `tail -f` (simulado en C#) del archivo `server-console.txt`.
- Si detecta `Exception` o `StackOverflow`, dispara automáticamente un diagnóstico silencioso y notifica al dashboard.

### 4. UI Layer Enhancements

#### [MODIFY] [ModManager.razor](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Components/Pages/ModManager.razor)
- **Log de Razonamiento IA**: Ver los pasos que tomó la IA (ej: "Leyendo mod.info...", "Analizando InventoryPane.lua...").
- **Alertas Proactivas**: Badge rojo en la pestaña de IA cuando el `LogObserver` detecta fallos vivos.

## Verification Plan

### Automated Tests
- Simular dos mods que contienen el mismo archivo Lua y verificar que `ScanDeepFileConflictAsync` lo detecta.
- Simular una excepción en un log dummy y verificar que el `PzLogObserver` dispara el evento de análisis.

### Manual Verification
- Cargar un mod de mapa y uno de UI que sobrescriben la misma función base y validar el informe de la IA.
