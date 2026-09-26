# Guía del Sistema - Project Zomboid Server Administrator

## 1. Descripción General

**Project Zomboid Server Administrator** es una solución integral y moderna construida en **Blazor Server (.NET 10)** para administrar servidores dedicados del juego **Project Zomboid**.

El proyecto está diseñado bajo una arquitectura modular y desacoplada que:
1. **Utiliza como motor base por defecto** el servidor dedicado en contenedor de [indifferentbroccoli/projectzomboid-server-docker](https://github.com/indifferentbroccoli/projectzomboid-server-docker). Nuestro repositorio **no clona ni modifica su código**, sino que consume directamente su imagen Docker oficial a través de la red y volúmenes compartidos.
2. **Es extensible y configurable para cualquier otro servidor dedicado** (servidores nativos en Windows/Linux, otros contenedores de SteamCMD, etc.), permitiendo cambiar rutas, nombres de servidor y credenciales mediante variables de entorno o desde la propia interfaz web.
3. **Incluye un entorno `docker-compose` de despliegue automático** que levanta en un único paso:
   - El servidor de juego Project Zomboid (`indifferentbroccoli`).
   - El panel web de administración (`pz-admin`).
   - El agente de túneles [Playit.gg](https://playit.gg) para permitir conexión multijugador sin necesidad de abrir puertos en el router (Zero Port-Forwarding).

---

## 2. Arquitectura de Servicios

```
                           [ Red Externa / Internet ]
                                       │
                         ┌─────────────┴─────────────┐
                         │   Playit.gg Tunnel Cloud  │
                         └─────────────┬─────────────┘
                                       │ (Túnel cifrado)
┌──────────────────────────────────────┼───────────────────────────────────────────┐
│ Red Docker: pz-network               ▼                                           │
│                         ┌─────────────────────────┐                              │
│                         │      playit-agent       │                              │
│                         │  (ghcr.io/playit-cloud) │                              │
│                         └────┬───────────────┬────┘                              │
│                              │               │                                   │
│           (UDP 16261/16262)  │               │ (TCP 8080)                        │
│                              ▼               ▼                                   │
│       ┌────────────────────────────┐    ┌─────────────────────────────────┐      │
│       │   projectzomboid           │    │   pz-admin                      │      │
│       │   (indifferentbroccoli)    │    │   (Panel Blazor .NET 10)        │      │
│       │                            │◄───┤                                 │      │
│       │   - PZ Dedicated Server    │RCON│   - Consola RCON                │      │
│       │   - SteamCMD Updater       │27015   - Editor .INI y .LUA          │      │
│       │   - Saves & SQLite DB      │    │   - Explorador de Base de Datos │      │
│       │   - Game Logs              │    │   - Gestor de Mods con IA       │      │
│       └──────────────┬─────────────┘    └────────────────┬────────────────┘      │
│                      │                                   │                       │
└──────────────────────┼───────────────────────────────────┼───────────────────────┘
                       ▼                                   ▼
         ┌───────────────────────────┐       ┌───────────────────────────┐
         │ Volúmenes del Servidor    │       │ Volúmenes del Admin       │
         │ - ./server-data (Config/DB)│◄──────┤ (Lectura y Escritura      │
         │ - ./server-files (Binarios)│◄──────┤ de Config, DB y Logs)     │
         └───────────────────────────┘       │ - ./admin-config          │
                                             │ - ./playit-data           │
                                             └───────────────────────────┘
```

---

## 3. Despliegue Rápido (Quick Start)

### Paso 1: Configurar el archivo `.env`
Copia la plantilla `.env.example` en la raíz del repositorio a un nuevo archivo `.env`:

```bash
cp .env.example .env
```

Abre `.env` con tu editor preferido y define tus contraseñas seguras:
- `ADMIN_PASSWORD`: Contraseña del usuario administrador.
- `RCON_PASSWORD`: Contraseña para la consola remota RCON.

*(Opcional)* Si cuentas con una clave secreta de [playit.gg](https://playit.gg), asígnala en `PLAYIT_SECRET_KEY`.

### Paso 2: Iniciar la infraestructura
Ejecuta Docker Compose desde la raíz del proyecto:

```bash
docker compose up -d
```

### Paso 3: Acceder al Panel de Administración
Una vez iniciado el contenedor:
- Ingresa desde tu navegador a: **`http://localhost:8080`** (o la IP de tu servidor en el puerto 8080).
- Inicia sesión con el usuario `admin` y la contraseña configurada en `ADMIN_PASSWORD`.
- La aplicación se vinculará de inmediato al servidor `projectzomboid` a través de la red interna `pz-network`.

---

## 4. Estructura de Volúmenes y Persistencia

| Directorio en Host | Punto de Montaje en Contenedor | Propósito |
|--------------------|--------------------------------|-----------|
| `./server-data` | `/project-zomboid-config` (`projectzomboid` y `pz-admin`) | **Datos vitales del servidor PZ:** Contiene `Server/<SERVER_NAME>.ini`, `Server/<SERVER_NAME>_SandboxVars.lua`, `Server/<SERVER_NAME>_spawnregions.lua`, `db/<SERVER_NAME>.sqlite`, `Saves/Multiplayer/` y `Logs/`. Compartido en lectura/escritura con el panel para su edición directa. |
| `./server-files` | `/project-zomboid` (`projectzomboid`)<br>`/project-zomboid-files` (`pz-admin`: solo lectura) | **Archivos del juego y mods:** Instalación de Project Zomboid descargada vía SteamCMD y contenido del Workshop descargado en `steamapps/workshop/content/108600`. |
| `./admin-config` | `/app/config` (`pz-admin`) | **Configuración propia del panel:** Archivo `config.json` con los usuarios, permisos de roles, llaves de API para IA (Gemini/OpenAI/Ollama/Anthropic) y llaves de protección de datos de sesión. |
| `./playit-data` | `/etc/playit` (`playit-agent`) | **Credenciales de Playit.gg:** Archivos de autenticación y estado del agente de túneles. |

---

## 5. Configuración para Otros Servidores Dedicados

Aunque el sistema viene preconfigurado de fábrica para el contenedor de **indifferent broccoli**, está completamente desacoplado y puede administrarse cualquier servidor dedicado de Project Zomboid (por ejemplo, servidores dedicados nativos en Linux/Windows o contenedores alternativos).

### Opciones de personalización:

1. **Vía Variables de Entorno (en `.env` o en el host):**
   - `PZ_SERVER_DIR`: Ruta a la carpeta que contiene los archivos de configuración (por ejemplo, `/ruta/a/Zomboid/Server`).
   - `PZ_ZOMBOID_DIR`: Carpeta raíz de Zomboid (donde están `db/`, `Saves/`, `Logs/`).
   - `PZ_ACTIVE_SERVER` o `SERVER_NAME`: Nombre del servidor (prefijo del archivo `.ini`, por defecto `pzserver` o `servertest`).
   - `PZ_RCON_HOST`: Host o dirección IP del servidor RCON (por defecto `projectzomboid` en Docker, o `127.0.0.1` en nativo).
   - `PZ_RCON_PORT`: Puerto de conexión RCON (por defecto `27015`).
   - `PZ_RCON_PASSWORD`: Contraseña RCON en caso de no estar especificada en el archivo `.ini`.

2. **Vía Interfaz Web:**
   - En la sección **Configuración Global (`/settings`)** o **App Config (`/app-config`)**, el administrador puede cambiar las rutas a archivos `.ini`, `.lua`, bases de datos `.sqlite` y bases de datos `players.db` y `vehicles.db` en cualquier momento.
   - El botón **Escanear archivos de Servidor (Deep Scan)** busca de forma recursiva archivos de configuración en las rutas comunes (`/project-zomboid-config`, `/data`, `/home/steam/Zomboid`, `~/Zomboid`, `./server-data`).

---

## 6. Configuración de Red y Playit.gg (Zero Port-Forwarding)

El servicio `playit` corre dentro de la misma red virtual `pz-network`, lo que permite mapear tráfico directamente a los nombres de host de los contenedores sin exponer puertos al router:

1. **Obtener la cuenta y agente:**
   - Si no definiste `PLAYIT_SECRET_KEY` en tu `.env`, al levantar los contenedores ejecuta:
     ```bash
     docker compose logs playit
     ```
   - Verás un enlace de tipo `https://playit.gg/claim/...`. Ábrelo en tu navegador para reclamar el agente en tu cuenta de Playit.
2. **Crear túnel para el Servidor de Juego:**
   - En el dashboard de Playit.gg, agrega un túnel de tipo **UDP**.
   - Host interno: `projectzomboid`
   - Puerto interno: `16261` (y otro para `16262` si tu versión de PZ lo requiere).
   - Comparte la dirección IP pública y puerto asignado por Playit con tus jugadores para que puedan conectarse sin configurar puertos en tu router.
3. **Crear túnel para el Panel Web (Opcional):**
   - Si deseas acceder a este panel de administración desde fuera de tu red local, agrega un túnel de tipo **HTTP** o **TCP** apuntando a `pz-admin` en el puerto `8080`.

---

## 7. Módulos y Funcionalidades del Panel

- **Editor Visual de Configuración:** Modificación asistida con tipado y descripciones de todos los parámetros de `<SERVER_NAME>.ini` y `<SERVER_NAME>_SandboxVars.lua`.
- **Consola RCON en Vivo:** Ejecución remota de comandos de administración en tiempo real con selector de host inteligente (`projectzomboid`, red local, host nativo o IP personalizada).
- **Explorador de Base de Datos SQLite:** Visualización y filtrado de tablas de whitelist, roles, capabilities, tickets, logs de usuarios, baneos de IP/SteamID, y cuentas de jugadores (`players.db`).
- **Gestión Avanzada de Mods e IA:** Escaneo de mods instalados, cálculo de orden de carga de mods, dependencias de Steam Workshop y asistente impulsado por IA (Gemini, OpenAI, Ollama, Anthropic) para resolver conflictos entre mods.
- **Auditoría de Logs:** Visor integrado para explorar los archivos de `server-console.txt` y los registros cronológicos en `Logs/`.
- **Seguridad y Permisos:** Control de acceso basado en roles con soporte para perfiles Administrador, Moderador e Invitado.

---

## 8. Mantenimiento y Comandos Útiles

```bash
# Ver el estado de todos los servicios
docker compose ps

# Ver registros en tiempo real del servidor de juego
docker compose logs -f projectzomboid

# Ver registros en tiempo real del panel web
docker compose logs -f pz-admin

# Reiniciar el servidor de juego manteniendo el panel activo
docker compose restart projectzomboid

# Detener todos los servicios
docker compose down

# Actualizar las imágenes de contenedores
docker compose pull && docker compose up -d
```
