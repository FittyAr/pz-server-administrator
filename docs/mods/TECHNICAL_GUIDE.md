# Especificación Técnica: Gestión Avanzada de Mods (v2.0)

Este documento sirve como la guía de implementación oficial para el módulo de Gestión de Mods del Project Zomboid Server Administrator.

---

## 🏗️ 1. Arquitectura de Datos y Persistencia

### Base de Datos: `Mods.db` (SQLite + EF Core)
Se utilizará una base de datos independiente para no interferir con las bases de datos originales del juego.

#### Entidades Principales:
- **`WorkshopItem`**: 
  - `SteamId` (string): PK única de Steam.
  - `Title`, `Description` (string): Metadatos extraídos de la web.
  - `ThumbnailPath` (string): Ruta local a la imagen en caché.
  - `VersionHash` (string): MD5 de los archivos críticos para detectar cambios.
- **`ModInstance`**:
  - `ModId` (string): ID interno del mod (ej. "Hydrocraft").
  - `Name` (string): Nombre legible para humanos.
  - `Category` (Enum): Categoría de ordenamiento (Framework, Map, Vehicle, etc.).
  - `IsActive` (bool): Estado actual en el archivo `.ini`.
- **`CloudProfile`**:
  - `ApiKey` (string): Identificador para sincronización y servicios de IA.
  - `IsApiKeyValid` (bool): Estado de validación tras probar la conexión.
  - `CloudSyncEnabled` (bool): Habilitar reporte automático de configuraciones.
  - `AiAutoFixEnabled` (bool): Permiso para que el Agente aplique cambios.

---

## 🔍 2. Proceso de Descubrimiento y Sincronización

### Motor de Escaneo (Discovery Engine)
1. **Fase de Directorio**: Localizar recursivamente archivos `mod.info` en la carpeta `workshop/content/108600/`.
2. **Fase de Mapeo**: Un solo `WorkshopItem` puede generar múltiples `ModInstance`. El motor debe separar estas instancias para permitir su activación individual.
3. **Fase Web (Scraping/API)**: Si se detecta una conexión activa, obtener la imagen de portada y descripción extendida de Steam Workshop.

---

## 🧠 3. Inteligencia Artificial y Diagnóstico

### Integración de Modelos (LLM Integration)
- **Interfaz**: `IAiService` abstracto para soportar múltiples proveedores (Gemini, ChatGPT, Ollama).
- **Funcionalidades**:
  - **Detección de Incompatibilidades**: Enviar a la IA el nombre, descripción y archivos modificados de los mods para identificar solapamientos críticos.
  - **Sugerencias de Orden**: Basado en el tipo de archivos (LUA vs Scripts), la IA sugerirá un orden óptimo.
  - **Bloqueo Preventivo**: Si un mod se marca como incompatible con otro ya activo, la interfaz de usuario bloqueará su activación.
  - **Modo Agéntico (Auto-Fix)**: Configuración opcional para permitir que la IA aplique cambios de orden y activación sin intervención humana. Consultar [Guía del Agente de IA](file:///d:/GitHub/pz-server-administrator/docs/mods/AI_AGENT_GUIDE.md).

---

## 📋 4. Motor de Ordenamiento (Ordering Engine)

El sistema inyectará la lista final de `ModIds` en la propiedad `Mods=` del `.ini`, respetando la jerarquía técnica:

1. **Framework/API/Tweak mods**: Librerías base (ej: *Moodles Framework*).
2. **Resources/Textures**: Cambios puramente estéticos o de sonido.
3. **Maps/Locations**: Inyecciones de celdas y mapas (el orden determina qué mapa sobrescribe a cuál).
4. **Vehicles (Mod-Based)**: Nuevos vehículos añadidos por la comunidad.
5. **Code-only mods**: Mods de lógica pura que no añaden assets.
6. **Clothing/Interface**: Equipamiento adicional y cambios en el HUD.
7. **Other mods**: Mods generales no categorizados.
8. **Localization**: Traducciones que deben cargar al final para sobreescribir textos de otros mods.

---

## 🌐 5. Telemetría y Ecosistema Centralizado

### API de Comunidad (Futuro)
- **Reporte Anónimo**: Estadísticas de mods más usados para generar "Listas de Recomendados".
- **Identificación por API Key**: Permite a los administradores experimentados subir sus "Presets" de mods con sus órdenes de carga validados.
- **Sincronización Cloud**: Recuperar configuraciones enteras de servidores previos simplemente vinculando la cuenta.

---

## 🎨 6. Interfaz de Usuario (UI/UX)

1. **Explorador de Mods (Vista Grid)**: Tarjetas con imágenes de Steam y badges de estado.
2. **Gestión de Carga (Vista Lista)**: Interfaz de arrastrar y soltar (Drag & Drop) para definir el orden final.
3. **Centro de IA**: Panel lateral para solicitar "Diagnóstico de Conflictos" y ver explicaciones detalladas de la IA.
4. **Filtros Avanzados**: Por categoría, por estado (activo/inactivo) y por "Requiere Actualización".

---

## 🚀 7. Estado de la Implementación (Roadmap)

### ✅ Finalizado (Sprint 1: Cimientos y Descubrimiento)
- [x] **Infraestructura de Datos**: Creación de `Mods.db`, `ModsContext` y modelos relacionales (`WorkshopItem`, `ModInstance`).
- [x] **Motor de Descubrimiento (`ModDiscoveryService`)**: 
  - Escaneo recursivo y robusto de carpetas de Workshop.
  - Sincronización automática de IDs y nombres desde archivos `mod.info`.
  - Detección de estado **Activo/Inactivo** leyendo el archivo `.ini` del servidor.
- [x] **Web Scraping**: Extracción de títulos de Steam y miniaturas mediante `IHttpClientFactory`.
- [x] **Interfaz Base**: Página `ModManager.razor` funcional con grid responsivo y sistema de notificaciones.
- [x] **Localización**: Soporte completo para inglés y español en el módulo de gestión.

### ✅ Finalizado (Sprint 2: Ordenamiento y Persistencia)
- [x] **Mod List Ordering**: Interfaz de pestañas y botones de movimiento para definir el orden de carga técnico.
- [x] **Escritura en `.ini`**: Lógica robusta para actualizar las claves `Mods=` y `WorkshopItems=` preservando el orden manual.
- [x] **Gestión de Categorías**: Implementación del motor de categorización automática (Enum `ModCategory`) basado en heurísticas.
- [x] **Búsqueda y Filtrado**: Filtro en tiempo real por título o ID para gestionar grandes listas de mods.

### ✅ Finalizado (Sprint 3: IA e Integración Inteligente)
- [x] **IA Conflict Resolver**: Implementación del servicio `IAiService` con diagnóstico estructural heurístico (jerarquías, mapas, frameworks).
- [x] **IA Diagnostics UI**: Pestaña dedicada para solicitar informes de salud y **análisis de logs de servidor** (`server-console.txt`).
- [x] **Cloud Management**: Interfaz para gestionar API Key y preferencias de sincronización (`CloudProfile`).
  - [x] **API Key Validation**: Mecanismo de prueba ('Ping') para validar claves antes de activar la IA o Cloud Sync.
- [x] **Refinamiento de UI**: Sistema de pestañas completo (Explorador, Orden, IA, Ajustes).

### ✅ Finalizado (Sprint 4: Telemetría y Ecosistema)
- [x] **Community API**: Implementación de `ICommunityService` para el reporte de telemetría y obtención de recomendaciones globales.
- [x] **Version Control**: Sistema de hashing SHA-256 en `ModDiscoveryService` para detectar cambios en archivos locales.
- [x] **Infraestructura Cloud**: Persistencia de perfiles de usuario y sincronización de metadatos Steam mejorada.

### ✅ Finalizado (Sprint 5: Automatización y IA Avanzada)
- [x] **Mod Presets**: Sistema de perfiles (`ModPresetService`) para guardar y cargar configuraciones de mods en archivos JSON.
- [x] **Gestión de Perfiles UI**: Interfaz para crear, aplicar y eliminar perfiles de mods.
- [x] **Optimización IA**: Integración real con Google Gemini Pro para análisis semántico.
- [x] **Comportamiento Agéntico**: Generación y ejecución de planes de acción (`AiAction`) con modo automático opcional.

### ✅ Finalizado (Sprint 6: Automatización de Infraestructura)
- [x] **Cloud Sync Avanzado**: Capacidad de compartir y descargar perfiles (`ModPreset`) desde la nube de la comunidad.
- [x] **Explorador Comunitario**: UI integrada para descubrir presets recomendados.
- [x] **Auto-Updater Engine**: Lógica de disparo de actualizaciones vía SteamCMD integrada en el servicio de descubrimiento.

### ✅ Finalizado (Sprint 7: Estabilidad y UX Final)
- [x] **Notificaciones de Actualización**: Alerta visual ("Sincronización Disponible") cuando Steam tiene una versión más reciente que la local (basado en fechas).
- [x] **Acciones de Update**: Botón "Forzar Update" para disparar la descarga vía SteamCMD.
- [x] **Atajo de Reinicio RCON**: Botón manual para enviar el comando `quit` al servidor tras actualizaciones, evitando reinicios automáticos no deseados.
