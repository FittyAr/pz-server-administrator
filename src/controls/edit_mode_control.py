import flet as ft
from typing import Callable, Optional

class EditModeControl:
    """Control para seleccionar el modo de edición: Simple o Avanzada"""
    
    def __init__(self, on_mode_change: Optional[Callable[[str], None]] = None):
        self.on_mode_change = on_mode_change
        self.current_mode = "simple"  # Modo por defecto
        
        # Crear el control de selección de modo
        self._create_mode_selector()
    
    def _create_mode_selector(self):
        """Crear el selector de modo de edición"""
        self.mode_radio_group = ft.RadioGroup(
            content=ft.Row([
                ft.Radio(
                    value="simple",
                    label="Simple",
                    label_style=ft.TextStyle(
                        size=14,
                        weight=ft.FontWeight.W_500,
                        color=ft.Colors.ON_SURFACE
                    )
                ),
                ft.Radio(
                    value="advanced",
                    label="Avanzada",
                    label_style=ft.TextStyle(
                        size=14,
                        weight=ft.FontWeight.W_500,
                        color=ft.Colors.ON_SURFACE
                    )
                )
            ], spacing=20),
            value="simple",
            on_change=self._on_mode_change
        )
        
        # Contenedor principal del control
        self.control_container = ft.Container(
            content=ft.Column([
                ft.Row([
                    ft.Icon(
                        ft.Icons.EDIT_NOTE,
                        size=20,
                        color=ft.Colors.PRIMARY
                    ),
                    ft.Text(
                        "Modo de Edición:",
                        size=14,
                        weight=ft.FontWeight.W_600,
                        color=ft.Colors.ON_SURFACE
                    )
                ], spacing=8),
                self.mode_radio_group
            ], spacing=8),
            padding=ft.padding.all(12),
            border_radius=8,
            bgcolor=ft.Colors.SURFACE,
            border=ft.border.all(1, ft.Colors.OUTLINE)
        )
    
    def _on_mode_change(self, e):
        """Manejar el cambio de modo de edición"""
        self.current_mode = e.control.value
        
        # Notificar el cambio si hay un callback
        if self.on_mode_change:
            self.on_mode_change(self.current_mode)
    
    def get_current_mode(self) -> str:
        """Obtener el modo de edición actual"""
        return self.current_mode
    
    def set_mode(self, mode: str):
        """Establecer el modo de edición programáticamente"""
        if mode in ["simple", "advanced"]:
            self.current_mode = mode
            self.mode_radio_group.value = mode
            
            # Notificar el cambio si hay un callback
            if self.on_mode_change:
                self.on_mode_change(self.current_mode)
    
    def is_simple_mode(self) -> bool:
        """Verificar si está en modo simple"""
        return self.current_mode == "simple"
    
    def is_advanced_mode(self) -> bool:
        """Verificar si está en modo avanzado"""
        return self.current_mode == "advanced"
    
    def get_control(self):
        """Obtener el control completo"""
        return self.control_container