# Configuración de PZ Server Administrator

Este documento explica la estructura y uso del archivo `config.json` para la aplicación PZ Server Administrator.

## Ubicación del Archivo

El archivo de configuración se encuentra en la raíz del proyecto:
```
e:\GitHub\pz-server-administrator\config.json
```

## Estructura de Configuración

### 1. Configuración de la Aplicación (`app_config`)

```json
"app_config": {
  "app_name": "PZ Server Administrator",
  "version": "1.0.0",
  "language": "es",
  "theme": "dark",
  "auto_save_config": true,
  "backup_retention_days": 30
}
```

- **app_name**: Nombre de la aplicación
- **version**: Versión actual de la aplicación
- **language**: Idioma de la interfaz ("es" para español, "en" para inglés)
- **theme**: Tema visual ("dark" o "light")
- **auto_save_config**: Guardar automáticamente los cambios de configuración
- **backup_retention_days**: Días para mantener los respaldos automáticos

### 2. Configuración del Servidor (`server_config`)

#### Servidor Predeterminado
```json
"default_server": "main_server"
```
Define qué servidor se carga por defecto al iniciar la aplicación.

#### Configuración de Servidores
```json
"servers": {
  "main_server": {
    "name": "Servidor Principal",
    "description": "Servidor principal de Project Zomboid",
    "server_path": "C:\\Users\\%USERNAME%\\Zomboid\\Server",
    "executable_path": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid\\ProjectZomboid64.exe"
  }
}
```

- **name**: Nombre descriptivo del servidor
- **description**: Descripción del servidor
- **server_path**: Ruta donde se encuentran los archivos de configuración del servidor
- **executable_path**: Ruta del ejecutable de Project Zomboid

#### Archivos de Configuración
```json
"config_files": {
  "server_settings": "servertest.ini",
  "sandbox_vars": "servertest_SandBoxVars.lua",
  "spawn_regions": "servertest_spawnregions.lua",
  "spawn_points": "servertest_spawnpoints.lua"
}
```

Define los nombres de los archivos de configuración principales del servidor.

#### Configuración RCON
```json
"rcon": {
  "enabled": true,
  "host": "127.0.0.1",
  "port": 27015,
  "password": ""
}
```

- **enabled**: Habilitar conexión RCON
- **host**: Dirección IP del servidor RCON
- **port**: Puerto RCON
- **password**: Contraseña RCON (se recomienda configurar por separado por seguridad)

#### Configuración del Servidor de Juego
```json
"game_server": {
  "host": "0.0.0.0",
  "port": 16261,
  "udp_port": 16262
}
```

### 3. Configuración de Respaldos (`backup_config`)

```json
"backup_config": {
  "backup_path": "./backups",
  "auto_backup": {
    "enabled": true,
    "interval_hours": 6,
    "max_backups": 10
  }
}
```

- **backup_path**: Directorio donde se almacenan los respaldos
- **auto_backup.enabled**: Habilitar respaldos automáticos
- **auto_backup.interval_hours**: Intervalo en horas entre respaldos automáticos
- **auto_backup.max_backups**: Número máximo de respaldos a mantener

### 4. Configuración de Logs (`logging_config`)

```json
"logging_config": {
  "log_level": "INFO",
  "log_files": {
    "server_log": "console.txt",
    "chat_log": "chat.txt",
    "admin_log": "admin.txt",
    "error_log": "error.txt"
  }
}
```

Define los archivos de log del servidor y el nivel de logging.

### 5. Gestión de Jugadores (`player_management`)

```json
"player_management": {
  "whitelist_enabled": false,
  "admin_users": [],
  "banned_users": [],
  "kick_timeout_minutes": 5,
  "ban_duration_hours": 24
}
```

### 6. Monitoreo (`monitoring`)

```json
"monitoring": {
  "performance_monitoring": true,
  "alert_thresholds": {
    "cpu_usage_percent": 85,
    "memory_usage_percent": 90,
    "disk_space_gb": 5
  }
}
```

Configura los umbrales de alerta para el monitoreo del servidor.

### 7. Seguridad (`security`)

```json
"security": {
  "admin_password_required": true,
  "session_timeout_minutes": 60,
  "max_login_attempts": 3,
  "lockout_duration_minutes": 15
}
```

### 8. Preferencias de UI (`ui_preferences`)

```json
"ui_preferences": {
  "window_size": {
    "width": 1200,
    "height": 800
  },
  "navigation_rail_extended": true,
  "show_tooltips": true
}
```

## Uso y Personalización

### Configuración Inicial

1. **Ruta del Servidor**: Modifica `server_path` para apuntar a tu directorio de servidor de Project Zomboid
2. **Ejecutable**: Actualiza `executable_path` con la ruta correcta de tu instalación de PZ
3. **RCON**: Configura los parámetros RCON según tu servidor
4. **Respaldos**: Ajusta la ruta y frecuencia de respaldos según tus necesidades

### Múltiples Servidores

Puedes agregar múltiples configuraciones de servidor en la sección `servers`:

```json
"servers": {
  "main_server": { /* configuración servidor 1 */ },
  "test_server": { /* configuración servidor 2 */ },
  "event_server": { /* configuración servidor 3 */ }
}
```

### Variables de Entorno

El archivo soporta variables de entorno como `%USERNAME%` que se expandirán automáticamente.

### Seguridad

⚠️ **Importante**: 
- No incluyas contraseñas sensibles directamente en el archivo JSON
- Considera usar variables de entorno para datos sensibles
- Mantén este archivo fuera del control de versiones si contiene información sensible

### Validación

La aplicación validará automáticamente la configuración al inicio y mostrará errores si hay problemas con:
- Rutas de archivos inexistentes
- Puertos en uso
- Configuraciones inválidas

## Respaldo de Configuración

Se recomienda hacer respaldos regulares del archivo `config.json`, especialmente antes de realizar cambios importantes.

## Soporte

Para problemas con la configuración, consulta los logs de la aplicación o revisa la documentación del proyecto.