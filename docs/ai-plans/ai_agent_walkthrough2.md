# Walkthrough: Agente de IA - Mod Harmony & Auto-Fix

Hemos elevado el módulo de IA de un simple analizador a un **Agente de Sistemas autónomo** capaz de realizar cambios técnicos para estabilizar el servidor.

## 🚀 Logros Principales

### 1. Motor Agéntico (Engineer Brain)
- **Rol Senior**: La IA ahora actúa con el contexto de un Ingeniero de Sistemas especializado en Project Zomboid.
- **Salida Estructurada**: El servicio genera planes de acción en formato JSON, permitiendo una integración programática precisa.
- **Contexto Extendido**: El agente analiza IDs de workshop, categorías, dependencias y logs críticos para tomar decisiones.

### 2. Auto-Corrección (The "Fixer")
Implementamos [BatchApplyAiActionsAsync](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Services/ModDiscoveryService.cs#517-552) que permite al administrador aplicar múltiples correcciones con un solo clic:
- **Desactivación de Mods Conflictivos**: Identifica mods que causan crashes en el log y los deshabilita.
- **Reordenamiento Inteligente**: Ajusta el [Order](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/Services/AiService.cs#94-105) de carga para cumplir con las jerarquías técnicas (Frameworks > Maps > Localization).
- **Persistencia**: Todos los cambios se guardan en la base de datos y se escriben automáticamente en los archivos `.ini` y `.lua` del servidor.

### 3. Centro de Comando de IA (UI)
- **Panel de Acciones**: Las recomendaciones de la IA se presentan como tarjetas de acción con nivel de confianza y justificación técnica.
- **Ejecución por Lotes**: Botón "Aplicar Armonización Automática" para resolver todos los problemas detectados de una vez.

## 🛠️ Cómo Probarlo
1. Ve a la pestaña **Diagnóstico IA** en el Gestor de Mods.
2. Haz clic en **Analyze with AI**.
3. Si hay conflictos (ej: Frameworks cargando tarde), verás el **Plan de Acción Recomendado**.
4. Haz clic en **Aplicar Armonización Automática**.
5. Verifica que los mods se han reordenado o desactivado según el plan y que la configuración del servidor se ha actualizado.

## 🛡️ Verificación Técnica
- Se validó el parser de JSON de IA para manejar respuestas consistentes.
- Se corrigieron los errores de inyección del repositorio y acceso a contextos de base de datos.
- Se verificó que la persistencia al archivo `.ini` se dispara tras la armonización.

## 🧠 Expansión Agéntica (Ciclos de Razonamiento)
El Agente de IA ahora tiene la capacidad de **"Razonar y Solicitar"**, rompiendo la barrera de las respuestas de disparo único.

### 4. Bucle Cognitivo (Reasoning Loop)
- **Deep Scan a Petición**: Si la IA considera que los metadatos de los mods no son suficientes para arreglar un problema, puede emitir la acción `RequestDeepScan`. El sistema escanea todos los archivos LUA e INI y le devuelve a la IA los que están sobreescritos en más de un mod.
- **Lectura de Archivos**: La IA puede solicitar la lectura de archivos específicos usando `RequestFile("ruta")` obteniendo su contenido en su contexto antes de emitir un veredicto.
- **Progreso en Tiempo Real**: La interfaz de usuario muestra el "Pensamiento del Agente" paso a paso, imprimiendo las decisiones intermedias del LLM en pantalla.

### 5. Ojos del Agente (Log Observer)
- **Monitorización Continua**: Se implementó [PzLogObserver](file:///d:/GitHub/pz-server-administrator/pz-server-administrator/BackgroundServices/PzLogObserver.cs#22-31), un BackgroundService de .NET que vigila de manera constante los archivos `server-console.txt` del servidor.
- **Diagnóstico Proactivo**: Si el servidor experimenta una excepción (ej. "StackOverflow", "Exception", "Fatal"), el Observer recopila las últimas 20 líneas y alerta a la interfaz, allanando el camino para el "AutoFix" incluso sin intervención humana.
