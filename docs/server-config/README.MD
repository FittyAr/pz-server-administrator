# Configuración del Servidor

Este módulo permite al usuario visualizar y modificar los archivos de configuración principales del servidor de Project Zomboid.

## Archivos Soportados

- `NombreServer.ini`
- `NombreServer_SandboxVars.lua`
- `NombreServer_spawnregions.lua`
- `NombreServer_spawnpoints.lua`

## Modos de Edición

- **Avanzado**: vista de texto plano editable del archivo.
- **Simple**: controles por parámetro (checkboxes, dropdowns, textboxes).

## Características

- Parámetros no presentes en el archivo se muestran tenues y se activan al editarlos.
- Cada parámetro tiene tooltip de ayuda con descripción.
- Cambios se reflejan en tiempo real (previa confirmación o guardado).

## Consideraciones Técnicas

- Se utilizan servicios inyectados `IPzServerService` para el manejo de archivos.
- Los archivos se editan "al vuelo" con persistencia inmediata al guardar.
- El proyecto utiliza .NET 10 y FluentUI Blazor.

## Próximos pasos

- Refinar el sistema de tooltips con descripciones detalladas.
- Añadir validaciones avanzadas para tipos de datos complejos.
