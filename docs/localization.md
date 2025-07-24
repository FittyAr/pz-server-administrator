# Sistema de Internacionalización (i18n)

Este documento describe cómo funciona el sistema de internacionalización en la aplicación PZ Server Administrator.

## Resumen

El sistema de i18n permite que la interfaz de usuario de la aplicación se muestre en múltiples idiomas. Utiliza archivos de traducción en formato JSON para almacenar los textos y un servicio de localización para cargarlos y gestionarlos dinámicamente.

## Características

- **Modular**: Se pueden agregar nuevos idiomas simplemente creando un nuevo archivo JSON en el directorio de idiomas.
- **Dinámico**: La aplicación detecta automáticamente los idiomas disponibles al inicio.
- **Centralizado**: Toda la lógica de localización se gestiona a través del `LocalizationService`.
- **Fácil de usar**: Los componentes Razor pueden acceder a las traducciones inyectando el `ILocalizationService` y usando el método `Get(string key)`.

## Estructura de Archivos

- **Archivos de idioma**: Se encuentran en `config/lang/`. Cada archivo se nombra con el código de idioma de dos letras (por ejemplo, `en.json`, `es.json`).
- **Servicio de Localización**: La interfaz `ILocalizationService` y su implementación `LocalizationService` se encuentran en `pz-server-administrator/Services/`.
- **Componente Selector de Idioma**: El componente `LanguageSelector.razor` se encuentra en `pz-server-administrator/Components/Layout/`.

## Cómo Agregar un Nuevo Idioma

1.  **Crear el archivo JSON**: Crea un nuevo archivo en la carpeta `config/lang/` con el código del nuevo idioma (por ejemplo, `fr.json` para francés).
2.  **Copiar y traducir**: Copia el contenido de un archivo de idioma existente (por ejemplo, `en.json`) en el nuevo archivo y traduce todos los valores de las cadenas.

¡Eso es todo! La aplicación detectará automáticamente el nuevo idioma y lo mostrará en el selector de idiomas.

## Uso en Componentes

Para usar el servicio de localización en un componente Razor, sigue estos pasos:

1.  **Inyectar el servicio**:

    ```csharp
    @inject ILocalizationService LocService
    ```

2.  **Obtener una traducción**:

    ```csharp
    <FluentLabel>@LocService.Get("Key.To.Translate")</FluentLabel>
    ```

Si una clave de traducción no se encuentra en el archivo de idioma actual, se devolverá la propia clave como valor predeterminado.