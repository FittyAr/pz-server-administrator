# Roadmap de Desarrollo: Project Zomboid Server Administrator

Este documento resume los hitos alcanzados y la visión futura del proyecto.

## 🏁 Hitos Alcanzados

### Fase 1: Gestión de Mods y Workshop (Completado)
- Escaneo y descubrimiento robusto de mods locales.
- Integración con Steam Workshop (Web Scraping).
- Gestión de categorías técnicas (Frameworks, Mapas, etc.).

### Fase 2: Inteligencia Artificial y Diagnóstico (Completado)
- Análisis de conflictos basado en heurísticas.
- Integración con Google Gemini para interpretación de logs de error.
- **AI Agentic Flow**: Capacidad de auto-corrección y armonización automática de órdenes de carga.

### Fase 3: Ecosistema Comunitario (Completado)
- Sistema de Presets locales y en la nube.
- Sincronización de perfiles vía API Key.
- Repuesta ágil a actualizaciones de mods (Detección de mods outdated).

---

## 🚀 Próximos Pasos (Vision 2026)

### 🟠 Q2 2026: Diagnóstico de Hardware y Red
- **IA Performance Monitor**: Analizar logs de GC (Garbage Collection) y proponer ajustes de memoria RAM en el `.ini`.
- **RCON Dashboard**: Interfaz en tiempo real para comandos de administración masiva (Broadcasting, Bans, Kicks).

### 🟠 Q3 2026: Gestión de Mapas y World Data
- **Map Conflict Visualizer**: Herramienta visual para ver solapamientos de celdas entre mods de mapas.
- **Entity Explorer**: Buscador avanzado de objetos y vehículos en los archivos de la partida (`zpop_*`).

### 🟠 Q4 2026: Automatización de SteamCMD
- **Integrated SteamCMD**: Descarga y actualización de mods directamente desde la aplicación sin depender de scripts externos.
- **Backups Inteligentes**: Rotación de copias de seguridad basada en eventos (ej: backup automático antes de una actualización masiva de mods).

---

## 💡 Filosofía del Proyecto
El objetivo es reducir la carga cognitiva del administrador de servidores mediante el uso de **Inteligencia Artificial Especializada**, permitiendo que incluso usuarios sin conocimientos técnicos avanzados puedan gestionar infraestructuras de mods complejas con estabilidad profesional.
