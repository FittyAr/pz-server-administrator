# Módulo de Gestión de Mods (V2.0)

Este módulo permite la administración avanzada de mods y artículos del Steam Workshop para servidores de Project Zomboid.

## Características Implementadas 🚀

- **Descubrimiento Inteligente**: Escaneo recursivo y robusto de carpetas `Workshop` para detectar archivos `mod.info`.
- **Sincronización Bidireccional**: Lee y escribe directamente en el archivo `.ini` del servidor (`Mods=` y `WorkshopItems=`).
- **Gestión de Orden**: Interfaz para reordenar la carga de mods, vital para la estabilidad y conflictos de mapas.
- **Categorización Automática**: Clasifica mods en *Maps*, *Vehicles*, *Frameworks*, *Localization*, etc.
- **Integración con Steam**: Web scraping automático para obtener títulos y miniaturas oficiales.
- **Interfaz Premium**: Diseñada con Fluent UI, incluye búsqueda en tiempo real, pestañas de gestión y notificaciones interactivas.

## Documentación Detallada

- [Guía Técnica de Implementación](TECHNICAL_GUIDE.md): Detalles sobre arquitectura de datos, IA y roadmap.

## Próximos pasos (Sprint 3)

- **IA Conflict Resolver**: Diagnóstico de incompatibilidades basado en inteligencia artificial.
- **Cloud Metadata**: Perfiles de configuración compartidos en la nube.
- **Detección de Versiones**: Notificación automática cuando un mod en el servidor está desactualizado respecto a Steam.
