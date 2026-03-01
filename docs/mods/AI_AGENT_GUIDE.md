# Guía del Agente de IA: Diagnóstico y Armonización Autónoma

El Agente de IA es una extensión del `AiService` diseñada para actuar como un Ingeniero de Sistemas virtual. Su objetivo es mantener la estabilidad del servidor mediante el análisis de conflictos y la ejecución de correcciones técnicas.

## 🧠 Funcionamiento Interno

El agente utiliza un modelo de lenguaje avanzado (Google Gemini Pro) con un **System Prompt** especializado que le otorga el rol de experto en la arquitectura del motor de Project Zomboid (Java/Lua).

### Ciclo de Trabajo Agéntico:
1. **Recolección de Contexto**: Obtiene el orden de carga actual, categorías de mods, IDs de workshop y fragmentos de logs de error (`server-console.txt`).
2. **Análisis Técnico**: Evalúa dependencias y jerarquías (Frameworks > Maps > Scripts > Localization).
3. **Generación de Plan**: Produce un conjunto de `AiAction` (JSON) con instrucciones precisas:
   - `Deactivate`: Desactivar mods que causan crashes.
   - `Reorder`: Ajustar el `Order` para cumplir con las reglas técnicas.
   - `FixConfig`: Sugerir cambios en parámetros de configuración.
4. **Ejecución**: Aplica los cambios directamente en la base de datos `Mods.db` y sincroniza los archivos `.ini` y `.lua`.

## ⚙️ Configuración y Modos

Se han implementado dos niveles de autonomía configurables en la pestaña de **Diagnóstico IA**, sujetos a la validación de la **Gemini API Key**:

### 🔑 Requisito: Validación de API Key
Para habilitar cualquier funcionalidad agéntica o de nube (Cloud), es obligatorio:
1.  Ingresar una Gemini API Key válida en los ajustes.
2.  Presionar el botón **"Probar Key"** para validar la conexión.
3.  Si la clave no es válida o no ha sido probada, todas las opciones de IA y sincronización permanecerán deshabilitadas para evitar errores de comunicación.

### 1. Modo Manual (Por defecto)
- La IA presenta el **Plan de Acción Recomendado** en pantalla.
- El administrador debe revisar las acciones y presionar **"Aplicar Armonización Automática"** para ejecutarlas por lotes.
- Ideal para administradores que desean control total sobre los cambios.

### 2. Modo Automático (Auto-Fix)
- Activando el toggle **"Modo Automático"**, el agente aplicará las correcciones inmediatamente después de generar el plan.
- Útil para mantenimiento rápido y corrección instantánea de órdenes de carga tras añadir nuevos mods.

## 🛠️ Especificaciones Técnicas
- **Modelo**: `gemini-1.5-flash` (optimizada para velocidad y parsing de JSON).
- **Esquema de Salida**: Formato JSON estricto para evitar errores de interpretación.
- **Seguridad**: El agente solo opera sobre el módulo de mods; no tiene acceso a borrar archivos del sistema fuera del ámbito del servidor.
