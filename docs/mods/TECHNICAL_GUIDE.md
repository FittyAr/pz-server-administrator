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
  - `ApiKey` (string): Identificador para sincronización con la API externa.
  - `AutoSync` (bool): Habilitar reporte automático de configuraciones.

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
  - **Bloqueo Preventivo**: Si un mod se marca como incompatible con otro ya activo, la interfaz de usuario bloqueará preventivo su activación.

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
- [x] **Infraestructura de Datos**: Creación de `Mods.db`, `ModsContext` y modelos relacionales (`WorkshopItem`, `ModInstance`, `CloudProfile`).
- [x] **Motor de Descubrimiento (`ModDiscoveryService`)**: 
  - Escaneo recursivo y robusto de carpetas de Workshop.
  - Sincronización automática de IDs y nombres desde archivos `mod.info`.
  - Detección de estado **Activo/Inactivo** leyendo el archivo `.ini` del servidor.
- [x] **Web Scraping**: Extracción de títulos de Steam y miniaturas mediante `IHttpClientFactory`.
- [x] **Interfaz Base**: Página `ModManager.razor` funcional con grid responsivo y sistema de notificaciones.
- [x] **Localización**: Soporte completo para inglés y español en el módulo de gestión.

### ⏳ En Progreso (Sprint 2: Ordenamiento y Persistencia)
- [ ] **Mod List Ordering**: Interfaz de arrastrar y soltar para definir el orden de carga técnico.
- [ ] **Escritura en `.ini`**: Lógica para actualizar las claves `Mods=` y `WorkshopItems=` preservando el orden manual del usuario.
- [ ] **Gestión de Categorías**: Implementar el motor de categorización automática (Enum `ModCategory`).

### 🚀 Pendiente (Sprint 3: IA e Integración Cloud)
- [ ] **IA Conflict Resolver**: Servicio para analizar incompatibilidades y sugerir órdenes óptimos mediante LLMs.
- [ ] **Cloud Presets**: Telemetría y carga/descarga de configuraciones de mods vía API Key.
- [ ] **Version Control**: Sistema de hashes para detectar cambios en los archivos de mods y notificar actualizaciones.
