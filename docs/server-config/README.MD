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

- Se parsean archivos INI y LUA en formato clave-valor.
- Los valores se mapean a un `ViewModel` intermedio que puede convertirse nuevamente en archivo de texto.

## Próximos pasos

- Añadir validación para tipos de datos.
- Sincronización con la configuración real del servidor en tiempo real.
