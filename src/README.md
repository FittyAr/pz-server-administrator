# Project Zomboid Server Administrator

⚠️ **PROYECTO EN DESARROLLO ACTIVO** ⚠️

**IMPORTANTE**: Este proyecto está actualmente en desarrollo y **NO ES UTILIZABLE** para uso en producción. La aplicación está siendo desarrollada y probada. Este README será actualizado cuando el proyecto esté listo para uso general.

Una aplicación de escritorio para administrar servidores dedicados de Project Zomboid, desarrollada con el framework Flet.

## Características

### 🖥️ Control del Servidor
- Iniciar, detener y reiniciar el servidor
- Monitoreo en tiempo real del estado del servidor
- Visualización de estadísticas (CPU, RAM, tiempo de actividad)
- Información de jugadores conectados

### ⚙️ Gestión de Configuraciones
- Editor integrado para archivos de configuración
- Soporte para múltiples formatos:
  - **INI**: Configuración principal del servidor (`servertest.ini`)
  - **LUA**: Variables sandbox y regiones de spawn
  - **JSON**: Reglas del servidor y configuraciones personalizadas
- Validación de sintaxis en tiempo real
- Respaldo automático antes de guardar cambios

### 👥 Administración de Jugadores
- Lista de jugadores conectados y offline
- Estadísticas de jugadores (nivel, tiempo de juego)
- Herramientas de moderación:
  - Expulsar jugadores
  - Banear/desbanear usuarios
  - Envío de mensajes privados y globales

### 📋 Visualización de Logs
- Logs del servidor en tiempo real
- Filtrado por tipo (servidor, chat, administración, errores)
- Exportación de logs
- Auto-actualización configurable

### 💾 Sistema de Respaldos
- Creación de respaldos manuales y automáticos
- Programación de respaldos periódicos
- Restauración de respaldos anteriores
- Gestión de espacio de almacenamiento

## Estructura del Proyecto

```
src/
├── app.py                 # Punto de entrada principal
├── main.py               # Punto de entrada alternativo
├── layouts/              # Layouts de la interfaz
│   ├── __init__.py
│   └── main_layout.py    # Layout principal con NavigationRail
├── controls/             # Controles de la interfaz
│   ├── __init__.py
│   ├── server_control.py    # Control del servidor
│   ├── config_control.py    # Gestión de configuraciones
│   ├── players_control.py   # Administración de jugadores
│   ├── logs_control.py      # Visualización de logs
│   └── backup_control.py    # Sistema de respaldos
├── utils/                # Utilidades y lógica de negocio
│   ├── __init__.py
│   ├── config_manager.py    # Gestor de archivos de configuración
│   └── server_manager.py    # Gestor del proceso del servidor
└── assets/              # Recursos (iconos, imágenes)
    ├── icon.png
    └── splash_android.png
```

## Instalación y Uso

### Prerrequisitos
- Python 3.8 o superior
- Project Zomboid Server instalado

### Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone <repository-url>
   cd pz-server-administrator
   ```

2. **Crear y activar entorno virtual:**
   ```bash
   # En Windows
   start_venv.bat
   
   # En Linux/Mac
   python -m venv venv
   source venv/bin/activate
   ```

3. **Instalar dependencias:**
   ```bash
   pip install -r requirements.txt
   ```

### Ejecución

```bash
# Desde el directorio src/
cd src
python app.py

# O alternativamente
python main.py
```

## Configuración

### Primera Configuración

1. **Configurar ruta del servidor:**
   - Especificar la ruta de instalación de Project Zomboid Server
   - Configurar la ruta de Java (si no está en PATH)

2. **Configurar respaldos:**
   - Establecer directorio de respaldos
   - Configurar frecuencia de respaldos automáticos

3. **Personalizar configuraciones:**
   - Editar archivos de configuración según necesidades
   - Establecer reglas del servidor

## Arquitectura

### Patrón de Diseño
- **Separación de responsabilidades**: Cada control maneja una funcionalidad específica
- **Modularidad**: Componentes independientes y reutilizables
- **Gestión centralizada**: Utilidades compartidas para operaciones comunes

### Componentes Principales

1. **MainLayout**: Layout principal con navegación lateral
2. **Controls**: Módulos independientes para cada funcionalidad
3. **Utils**: Lógica de negocio y operaciones del sistema
4. **Assets**: Recursos estáticos

### Tecnologías Utilizadas
- **[Flet](https://flet.dev/)**: Framework de interfaz gráfica basado en Flutter
- **[psutil](https://psutil.readthedocs.io/)**: Monitoreo de procesos y sistema
- **configparser**: Manejo de archivos INI
- **json**: Procesamiento de archivos JSON

## Desarrollo

### Agregar Nuevas Funcionalidades

1. **Crear nuevo control:**
   ```python
   # En controls/nuevo_control.py
   class NuevoControl:
       def build(self):
           return ft.Container(...)  # Interfaz del control
   ```

2. **Registrar en MainLayout:**
   ```python
   # En layouts/main_layout.py
   from controls.nuevo_control import NuevoControl
   
   # Agregar a destinations y _update_content
   ```

3. **Agregar utilidades si es necesario:**
   ```python
   # En utils/nueva_utilidad.py
   class NuevaUtilidad:
       def metodo(self):
           pass
   ```

### Convenciones de Código
- Nombres de clases en PascalCase
- Nombres de funciones y variables en snake_case
- Documentación en español para comentarios
- Nombres de código en inglés
- Código legible y bien documentado

## Contribución

1. Fork del proyecto
2. Crear rama para nueva funcionalidad
3. Implementar cambios siguiendo las convenciones
4. Crear pull request con descripción detallada

## Licencia

Este proyecto está bajo la licencia especificada en el archivo LICENSE.

## Soporte

Para reportar bugs o solicitar funcionalidades, crear un issue en el repositorio del proyecto.