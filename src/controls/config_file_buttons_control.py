import flet as ft
from typing import Callable, Optional
import os

class ConfigFileButtonsControl:
    """Control para manejar los botones de archivos de configuración (.ini, .lua, .json)"""
    
    def __init__(self, on_config_file_click: Callable[[str], None]):
        self.on_config_file_click = on_config_file_click
        self.config_buttons = []
        self.expanded = False
        self.current_server_path = None
        
        # Crear el botón principal y los botones de configuración
        self._create_main_button()
        self._create_config_buttons()
    
    def _create_main_button(self):
        """Crear el botón principal de configuración con flecha"""
        self.arrow_icon = ft.Icon(
            ft.Icons.KEYBOARD_ARROW_UP,
            size=20,
            color=ft.Colors.ON_SURFACE
        )
        
        self.main_button = ft.Container(
            content=ft.Row([
                self.arrow_icon,
                ft.Icon(
                    ft.Icons.SETTINGS,
                    size=20,
                    color=ft.Colors.PRIMARY
                ),
                ft.Text(
                    "Server Files",
                    size=14,
                    weight=ft.FontWeight.W_500,
                    color=ft.Colors.ON_SURFACE
                )
            ], spacing=8, alignment=ft.MainAxisAlignment.START),
            padding=ft.padding.symmetric(horizontal=12, vertical=10),
            bgcolor=ft.Colors.SURFACE,
            on_click=self._toggle_config_buttons,
            tooltip="Mostrar/Ocultar opciones de configuración",
            animate=ft.Animation(200, ft.AnimationCurve.EASE_IN_OUT)
        )
    
    def _create_config_buttons(self):
        """Crear los botones para cada tipo de archivo de configuración"""
        config_files = [
            {"type": "ini", "icon": ft.Icons.SETTINGS, "label": "Server Config", "color": ft.Colors.BLUE_400},
            {"type": "lua", "icon": ft.Icons.CODE, "label": "Sandbox Vars", "color": ft.Colors.GREEN_400},
            {"type": "spawn_regions", "icon": ft.Icons.MAP, "label": "Spawn Regions", "color": ft.Colors.ORANGE_400},
            {"type": "spawn_points", "icon": ft.Icons.LOCATION_ON, "label": "Spawn Points", "color": ft.Colors.PURPLE_400}
        ]
        
        self.config_buttons = []
        for config in config_files:
            # Crear una función de callback que capture correctamente el file_type
            def create_callback(file_type):
                return lambda e: self._on_config_button_click(file_type)
            
            button = ft.Container(
                content=ft.Row([
                    ft.Icon(
                        config["icon"],
                        size=16,
                        color=config["color"]
                    ),
                    ft.Text(
                        config["label"],
                        size=12,
                        weight=ft.FontWeight.W_500,
                        color=config["color"]
                    )
                ], spacing=8, alignment=ft.MainAxisAlignment.START),
                padding=ft.padding.symmetric(horizontal=16, vertical=8),
                margin=ft.margin.only(left=20),
                bgcolor=ft.Colors.SURFACE,
                on_click=create_callback(config["type"]),
                tooltip=f"Abrir configuración {config['type'].upper()}",
                animate=ft.Animation(150, ft.AnimationCurve.EASE_OUT),
                visible=False,
                height=0 if not self.expanded else None
            )
            self.config_buttons.append(button)
    
    def _toggle_config_buttons(self, e):
        """Alternar la visibilidad de los botones de configuración"""
        self.expanded = not self.expanded
        
        # Cambiar el icono de la flecha
        if self.expanded:
            self.arrow_icon.name = ft.Icons.KEYBOARD_ARROW_DOWN
        else:
            self.arrow_icon.name = ft.Icons.KEYBOARD_ARROW_UP
        
        # Mostrar/ocultar los botones de configuración con animación
        for button in self.config_buttons:
            button.visible = self.expanded
            button.height = None if self.expanded else 0
        
        # Actualizar la página para reflejar los cambios
        if hasattr(e.page, 'update'):
            e.page.update()
    
    def _on_config_button_click(self, file_type: str):
        """Manejar el clic en un botón de configuración"""
        print(f"DEBUG: _on_config_button_click llamado con file_type: {file_type}")
        print(f"DEBUG: current_server_path: {self.current_server_path}")
        print(f"DEBUG: on_config_file_click: {self.on_config_file_click}")
        
        if self.current_server_path and self.on_config_file_click:
            print(f"DEBUG: Ejecutando callback con file_type: {file_type}")
            self.on_config_file_click(file_type)
        else:
            print("DEBUG: No se ejecutó el callback - falta server_path o callback")
    
    def update_server_path(self, server_path: Optional[str]):
        """Actualizar la ruta del servidor actual"""
        self.current_server_path = server_path
        self._update_buttons_state()
    
    def _update_buttons_state(self):
        """Actualizar el estado de los botones según la ruta del servidor actual"""
        has_server = bool(self.current_server_path)
        
        # Actualizar estado del botón principal
        if has_server:
            self.main_button.disabled = False
            self.main_button.bgcolor = ft.Colors.SURFACE
        else:
            self.main_button.disabled = True
            self.main_button.bgcolor = ft.Colors.SURFACE_VARIANT
        
        # Actualizar estado de los botones de configuración
        for button in self.config_buttons:
            if has_server:
                button.bgcolor = ft.Colors.SURFACE
                button.border = ft.border.all(1, ft.Colors.OUTLINE)
                button.disabled = False
            else:
                button.bgcolor = ft.Colors.SURFACE_VARIANT
                button.border = ft.border.all(1, ft.Colors.OUTLINE)
                button.disabled = True
    

    
    def get_control(self):
        """Obtener el control completo con botón principal y botones de configuración"""
        return ft.Column([
            self.main_button,
            ft.Column(
                self.config_buttons,
                spacing=4
            )
        ], spacing=8)