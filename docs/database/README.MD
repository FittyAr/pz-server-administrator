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

- Uso de `Microsoft.Data.Sqlite` inyectado vía `ISqliteService`.
- Exploración dinámica de tablas y carga de datos limitando el impacto en memoria.
- Visualización en tablas con scroll y diseño responsivo.

## Próximos pasos

- Implementar edición de filas específicas para administradores.
- Vistas predefinidas para `players` y `logs` con formateo especial.
