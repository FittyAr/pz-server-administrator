# Project Zomboid Server Administrator

⚠️ **PROYECTO EN DESARROLLO ACTIVO** ⚠️

**IMPORTANTE**: Este proyecto está actualmente en desarrollo y **NO ES UTILIZABLE** para uso en producción. La aplicación está siendo desarrollada y probada. Este README será actualizado cuando el proyecto esté listo para uso general.

## Estado Actual del Proyecto

La aplicación está en fase de desarrollo temprano con las siguientes funcionalidades implementadas:
- ✅ Interfaz básica con navegación
- ✅ Detección automática de servidores de Project Zomboid
- ✅ Sistema de configuración básico
- ✅ Alternancia entre tema claro y oscuro
- 🚧 Control de servidores (en desarrollo)
- 🚧 Gestión de jugadores (en desarrollo)
- 🚧 Sistema de respaldos (en desarrollo)
- 🚧 Visualización de logs (en desarrollo)

## Pruebas de Desarrollo (Solo para Desarrolladores)

**NOTA**: Estas instrucciones son únicamente para desarrolladores que deseen probar el estado actual del proyecto. La aplicación NO está lista para uso final.

### Requisitos
- Python 3.8 o superior
- Project Zomboid instalado (para detectar servidores)
- Windows o Linux

### Configuración del Entorno de Desarrollo

#### Windows
```bash
# Clonar el repositorio
git clone <repository-url>
cd pz-server-administrator

# Crear entorno virtual
python -m venv venv
venv\Scripts\activate

# Instalar dependencias
pip install -r requirements.txt

# Ejecutar la aplicación
cd src
python app.py
```

#### Linux
```bash
# Clonar el repositorio
git clone <repository-url>
cd pz-server-administrator

# Crear entorno virtual
python3 -m venv venv
source venv/bin/activate

# Instalar dependencias
pip install -r requirements.txt

# Ejecutar la aplicación
cd src
python app.py
```
For more details on running the app, refer to the [Getting Started Guide](https://flet.dev/docs/getting-started/).

## Compilación y Distribución

⚠️ **NO DISPONIBLE ACTUALMENTE** ⚠️

La compilación y distribución de la aplicación **NO está disponible** en el estado actual del desarrollo. Estas funcionalidades serán habilitadas cuando el proyecto esté más maduro y estable.

### Futuras Opciones de Compilación (Planificadas)

- 📋 **Linux**: Paquetes .deb y .rpm
- 📋 **Windows**: Ejecutable .exe e instalador .msi

**Nota**: Por ahora, solo se puede ejecutar desde el código fuente siguiendo las instrucciones de desarrollo arriba.

## Contribución al Desarrollo

Si eres desarrollador y deseas contribuir:

1. Fork del repositorio
2. Crear rama para nueva funcionalidad
3. Seguir las convenciones de código del proyecto
4. Crear pull request con descripción detallada

## Soporte y Reportes

Para reportar bugs o problemas durante las pruebas de desarrollo, crear un issue en el repositorio del proyecto.

**Recordatorio**: Este proyecto está en desarrollo activo y no debe usarse en entornos de producción.