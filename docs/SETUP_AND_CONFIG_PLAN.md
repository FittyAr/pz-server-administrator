# Plan de Implementación: Configuración Centralizada y Asistente de Onboarding

Este documento detalla el plan para unificar toda la configuración de la aplicación y crear un flujo de configuración guiado (Wizard) para nuevos usuarios.

## 📋 Objetivos Principales
1.  **Refactorización de IA**: Soporte para múltiples proveedores (Gemini, OpenAI, Anthropic, Ollama).
2.  **Configuración Global**: Migrar todos los parámetros (API Keys, rutas, perfiles) a un único punto de verdad (`config.json`).
3.  **Asistente de Configuración (Wizard)**: Implementar un flujo paso a paso para la primera ejecución.
4.  **Gestión de Usuarios**: Incluir la creación de administración en el wizard.

---

## 🛠️ Refactorización Arquitectónica

### 1. Sistema de IA Multi-Proveedor
Se implementará un patrón de estrategia para los proveedores de IA.
- **`IAiProvider`**: Interfaz base.
- **`GeminiProvider`**, **`OpenAiProvider`**, **`AnthropicProvider`**, **`OllamaProvider`**: Implementaciones específicas.
- **`AiProviderFactory`**: Clase para instanciar el proveedor correcto según la configuración.

### 2. Centralización de Datos (`ZsmConfiguration`)
Se moverá la información de `CloudProfile` (actualmente en SQLite) al archivo `config.json`.
- **Nuevo modelo `AiConfiguration`**: Almacenará el proveedor activo, su API Key, modelo específico y configuraciones (Auto-Fix).
- **Nuevo modelo `CloudConfiguration`**: Almacenará la configuración de sincronización comunitaria.

---

## 🚀 Flujo del Asistente (Setup Wizard)

El asistente se activará si se detecta que la aplicación no tiene un usuario administrador configurado o si las rutas base del servidor están vacías.

### Pasos del Wizard:
1.  **Bienvenida**: Introducción a la herramienta.
2.  **Seguridad**: Creación de la cuenta de Administrador (Usuario/Password).
3.  **Localización del Servidor**:
    *   Auto-detección de carpetas de Project Zomboid.
    *   Validación de rutas (INI, DBs, Workshop).
4.  **Configuración de IA & Cloud**:
    *   Selección de proveedor de IA.
    *   Ingreso y testeo de API Key.
    *   Habilitación de sincronización cloud opcional.
5.  **Finalización**: Resumen y botón de "Entrar al Panel".

---

## 🔄 Simulaciones de Uso y Casos de Borde

### Caso A: Usuario Nuevo (Camino Feliz)
1. Instala en Docker -> Entra a la URL.
2. Ve el Wizard.
3. Crea admin "zomboid_admin".
4. El sistema detecta automáticamente `/home/steam/Zomboid`.
5. Ingresa Gemini Key -> "Test OK".
6. Finaliza -> Redirección a Home.

### Caso B: Usuario que prefiere omitir
1. Ve el Wizard.
2. Clic en "Configuración Avanzada / Omitir".
3. Redirección directa a `/settings` (Página Global).
4. Configura todo manualmente.

### Caso C: Error en validación de rutas
1. El usuario ingresa una ruta manual incorrecta.
2. El Wizard muestra error en tiempo real: "No se encontró servertest.ini en esta ubicación".
3. Ofrece sugerencia de búsqueda automática.

---

## 📝 Changelog de Implementación

### [Fase 1] - Modelos y Servicios (Backend)
- [ ] Mover `CloudProfile` de `Mods.db` a `Models/ZsmConfiguration.cs`.
- [ ] Implementar `IAiProvider` y adaptadores.
- [ ] Actualizar `ConfigurationService` para manejar los nuevos nodos de JSON.

### [Fase 2] - UI de Configuración Global
- [ ] Crear `Pages/GlobalSettings.razor` consolidando `Settings.razor` y la pestaña de Cloud de `ModManager`.
- [ ] Eliminar configuraciones redundantes de otras páginas.

### [Fase 3] - Setup Wizard
- [ ] Crear `Components/Shared/SetupWizard.razor`.
- [ ] Implementar lógica de redirección condicional en `MainLayout`.
- [ ] Implementar pasos del asistente con validaciones Fluent UI.

---

## 📈 Flujos de Trabajo (Workflows)

### Proceso de Adición de Nuevo Proveedor de IA
1. Crear clase en `Services/Ai/Providers/[Name]Provider.cs`.
2. Implementar `AnalyzeModConflictsAsync` según la API.
3. Añadir el nombre al Enum `AiProviderType`.
4. Añadir la lógica de instanciación en `AiProviderFactory`.

### Prooceso de Validación de Primera Ejecución
1. El middleware evalúa: `_config.Users.Count == 0 || string.IsNullOrEmpty(_config.AppSettings.ServerDirectoryPath)`.
2. Si es cierto, pone el flag `IsFirstRun = true` en el estado de la app.
3. El frontend muestra el Overlay del Wizard.
