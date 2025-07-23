# Explorador de Base de Datos SQLite

Este módulo permite acceder a la base de datos `.db` generada por el servidor para visualizar información de estado.

## Tablas esperadas (a confirmar)

- `Players`
- `PlayerStats`
- `WorldEvents`
- `ServerLogs`

## Funcionalidad

- Modo lectura: ver registros en tablas mediante DataGrid.
- Filtros y ordenamiento.
- (Futuro) Posibilidad de modificar datos por parte del administrador.

## Consideraciones Técnicas

- Uso de `Microsoft.Data.Sqlite` o `System.Data.SQLite`.
- Acceso concurrente controlado (base de datos puede estar en uso por el servidor).
- Visualización en tablas con paginación.

## Próximos pasos

- Obtener esquema real de la base de datos PZ.
- Diseñar vistas específicas para cada tabla relevante.
