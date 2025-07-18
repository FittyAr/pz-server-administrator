import flet as ft
from layouts.main_layout import MainLayout
from utils.config_loader import config_loader

def main(page: ft.Page):
    """
    Función principal de la aplicación
    """
    # Cargar configuración
    app_config = config_loader.get_app_config()
    ui_preferences = config_loader.get_ui_preferences()
    
    # Configuración de la página desde config.json
    page.title = app_config.get('app_name', 'PZ Server Administrator')
    
    # Configurar tema
    theme = app_config.get('theme', 'dark')
    page.theme_mode = ft.ThemeMode.DARK if theme == 'dark' else ft.ThemeMode.LIGHT
    
    # Configurar dimensiones de ventana
    window_size = ui_preferences.get('window_size', {'width': 1200, 'height': 800})
    page.window_width = window_size.get('width', 1200)
    page.window_height = window_size.get('height', 800)
    page.window_min_width = 800
    page.window_min_height = 600
    
    # Crear y mostrar el layout principal
    main_layout = MainLayout(page)
    page.add(main_layout.build())
    
    # TODO: Implementar validaciones de configuración más adelante
    # validation_result = config_loader.validate_config()
    
    page.update()

if __name__ == "__main__":
    ft.app(target=main)