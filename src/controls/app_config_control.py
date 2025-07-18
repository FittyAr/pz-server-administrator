import flet as ft
import json
import time
import sys
from utils.config_loader import config_loader
from pathlib import Path

class AppConfigControl:
    """
    Control para gestionar la configuración de la aplicación
    """
    
    def __init__(self):
        self.config_data = config_loader.config_data.copy()
        self.selected_section = "app_config"
        
        # Controles de UI
        self.section_dropdown = None
        self.config_editor = None
        self.status_text = None
        
    def build(self):
        """
        Construye la interfaz del control de configuración
        """
        # Dropdown para seleccionar sección
        self.section_dropdown = ft.Dropdown(
            label="Sección de Configuración",
            options=[
                ft.dropdown.Option("app_config", "Configuración de la Aplicación"),
                ft.dropdown.Option("server_config", "Configuración del Servidor"),
                ft.dropdown.Option("backup_config", "Configuración de Respaldos"),
                ft.dropdown.Option("logging_config", "Configuración de Logs"),
                ft.dropdown.Option("player_management", "Gestión de Jugadores"),
                ft.dropdown.Option("monitoring", "Monitoreo"),
                ft.dropdown.Option("security", "Seguridad"),
                ft.dropdown.Option("ui_preferences", "Preferencias de UI"),
                ft.dropdown.Option("advanced_settings", "Configuración Avanzada")
            ],
            value=self.selected_section,
            on_change=self._on_section_change,
            width=300
        )
        
        # Editor de configuración
        self.config_editor = ft.TextField(
            label="Configuración JSON",
            multiline=True,
            min_lines=20,
            max_lines=30,
            value=self._get_section_json(),
            expand=True
        )
        
        # Texto de estado
        self.status_text = ft.Text(
            "Listo para editar configuración",
            size=12,
            color=ft.Colors.GREY_400
        )
        
        return ft.Column([
            ft.Container(
                content=ft.Column([
                    ft.Row([
                        ft.Text(
                            "Configuración de la Aplicación",
                            size=24,
                            weight=ft.FontWeight.BOLD
                        ),
                        ft.Icon(ft.Icons.SETTINGS, size=30)
                    ], alignment=ft.MainAxisAlignment.SPACE_BETWEEN),
                    
                    ft.Divider(),
                    
                    # Información de configuración
                    ft.Container(
                        content=ft.Column([
                            ft.Text("Información de Configuración", weight=ft.FontWeight.BOLD),
                            ft.Row([
                                ft.Text(f"Archivo: {config_loader.config_path}", size=12),
                                ft.IconButton(
                                    icon=ft.Icons.FOLDER_OPEN,
                                    tooltip="Abrir ubicación del archivo",
                                    on_click=self._open_config_location
                                )
                            ]),
                            ft.Text(f"Versión: {config_loader.get_app_config('version') or 'N/A'}", size=12),
                            ft.Text(f"Servidor actual: {config_loader.get_server_config('default_server') or 'N/A'}", size=12),
                        ]),
                        bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                        padding=15,
                        border_radius=8,
                        margin=ft.margin.only(bottom=20)
                    ),
                    
                    # Selector de sección
                    ft.Row([
                        self.section_dropdown,
                        ft.ElevatedButton(
                            "Validar Configuración",
                            icon=ft.Icons.CHECK_CIRCLE,
                            on_click=self._validate_config,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        )
                    ]),
                    
                    ft.Divider(),
                    
                    # Editor de configuración
                    ft.Container(
                        content=self.config_editor,
                        border=ft.border.all(1, ft.Colors.OUTLINE),
                        border_radius=8,
                        padding=10,
                        expand=True
                    ),
                    
                    # Botones de acción
                    ft.Row([
                        ft.ElevatedButton(
                            "Guardar Cambios",
                            icon=ft.Icons.SAVE,
                            on_click=self._save_config,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Recargar desde Archivo",
                            icon=ft.Icons.REFRESH,
                            on_click=self._reload_config,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Restaurar Predeterminados",
                            icon=ft.Icons.RESTORE,
                            on_click=self._restore_defaults,
                            bgcolor=ft.Colors.RED,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Exportar Configuración",
                            icon=ft.Icons.DOWNLOAD,
                            on_click=self._export_config,
                            bgcolor=ft.Colors.PURPLE,
                            color=ft.Colors.WHITE
                        )
                    ], wrap=True),
                    
                    # Estado
                    self.status_text
                ]),
                padding=20,
                expand=True
            )
        ], expand=True)
    
    def _get_section_json(self) -> str:
        """
        Obtiene el JSON de la sección seleccionada
        """
        try:
            section_data = self.config_data.get(self.selected_section, {})
            return json.dumps(section_data, indent=2, ensure_ascii=False)
        except Exception as e:
            return f"Error al cargar sección: {e}"
    
    def _on_section_change(self, e):
        """
        Maneja el cambio de sección
        """
        self.selected_section = e.control.value
        self.config_editor.value = self._get_section_json()
        self.status_text.value = f"Editando sección: {self.selected_section}"
        e.page.update()
    
    def _validate_config(self, e):
        """
        Valida la configuración JSON
        """
        try:
            # Validar JSON
            json.loads(self.config_editor.value)
            
            # Validar configuración usando config_loader
            validation_result = config_loader.validate_config()
            
            if validation_result['errors']:
                error_msg = "\n".join(validation_result['errors'])
                e.page.snack_bar = ft.SnackBar(
                    content=ft.Text(f"Errores encontrados:\n{error_msg}"),
                    bgcolor=ft.Colors.RED
                )
                e.page.snack_bar.open = True
            elif validation_result['warnings']:
                warning_msg = "\n".join(validation_result['warnings'])
                e.page.snack_bar = ft.SnackBar(
                    content=ft.Text(f"Advertencias:\n{warning_msg}"),
                    bgcolor=ft.Colors.ORANGE
                )
                e.page.snack_bar.open = True
            else:
                e.page.snack_bar = ft.SnackBar(
                    content=ft.Text("Configuración válida"),
                    bgcolor=ft.Colors.GREEN
                )
                e.page.snack_bar.open = True
            
            self.status_text.value = "Validación completada"
            
        except json.JSONDecodeError as json_error:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"JSON inválido: {json_error}"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            self.status_text.value = "Error de validación JSON"
        
        e.page.update()
    
    def _save_config(self, e):
        """
        Guarda los cambios de configuración
        """
        try:
            # Parsear JSON de la sección
            section_data = json.loads(self.config_editor.value)
            
            # Actualizar datos de configuración
            self.config_data[self.selected_section] = section_data
            config_loader.config_data = self.config_data
            
            # Guardar al archivo
            if config_loader.save_config():
                e.page.snack_bar = ft.SnackBar(
                    content=ft.Text("Configuración guardada exitosamente"),
                    bgcolor=ft.Colors.GREEN
                )
                e.page.snack_bar.open = True
                self.status_text.value = "Configuración guardada"
            else:
                e.page.snack_bar = ft.SnackBar(
                    content=ft.Text("Error al guardar configuración"),
                    bgcolor=ft.Colors.RED
                )
                e.page.snack_bar.open = True
                self.status_text.value = "Error al guardar"
                
        except json.JSONDecodeError as json_error:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"JSON inválido: {json_error}"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            self.status_text.value = "Error: JSON inválido"
        except Exception as error:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"Error inesperado: {error}"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            self.status_text.value = f"Error: {error}"
        
        e.page.update()
    
    def _reload_config(self, e):
        """
        Recarga la configuración desde el archivo
        """
        config_loader._load_config()
        self.config_data = config_loader.config_data.copy()
        self.config_editor.value = self._get_section_json()
        self.status_text.value = "Configuración recargada desde archivo"
        
        e.page.snack_bar = ft.SnackBar(
            content=ft.Text("Configuración recargada"),
            bgcolor=ft.Colors.BLUE
        )
        e.page.snack_bar.open = True
        e.page.update()
    
    def _restore_defaults(self, e):
        """
        Restaura la configuración predeterminada
        """
        # Mostrar diálogo de confirmación
        def confirm_restore(e):
            config_loader._create_default_config()
            self.config_data = config_loader.config_data.copy()
            self.config_editor.value = self._get_section_json()
            self.status_text.value = "Configuración restaurada a valores predeterminados"
            
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("Configuración restaurada"),
                bgcolor=ft.Colors.ORANGE
            )
            e.page.snack_bar.open = True
            dialog.open = False
            e.page.update()
        
        def cancel_restore(cancel_e):
            dialog.open = False
            e.page.update()
        
        dialog = ft.AlertDialog(
            modal=True,
            title=ft.Text("Confirmar Restauración"),
            content=ft.Text("¿Estás seguro de que quieres restaurar la configuración a los valores predeterminados? Esta acción no se puede deshacer."),
            actions=[
                ft.TextButton("Cancelar", on_click=cancel_restore),
                ft.TextButton("Restaurar", on_click=confirm_restore),
            ],
            actions_alignment=ft.MainAxisAlignment.END,
        )
        
        e.page.dialog = dialog
        dialog.open = True
        e.page.update()
    
    def _export_config(self, e):
        """
        Exporta la configuración actual
        """
        try:
            export_path = Path(config_loader.config_path.parent) / f"config_export_{int(time.time())}.json"
            
            with open(export_path, 'w', encoding='utf-8') as file:
                json.dump(self.config_data, file, indent=2, ensure_ascii=False)
            
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"Configuración exportada a: {export_path.name}"),
                bgcolor=ft.Colors.GREEN
            )
            e.page.snack_bar.open = True
            self.status_text.value = f"Exportado a: {export_path.name}"
            
        except Exception as error:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"Error al exportar: {error}"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            self.status_text.value = f"Error al exportar: {error}"
        
        e.page.update()
    
    def _open_config_location(self, e):
        """
        Abre la ubicación del archivo de configuración
        """
        import os
        import subprocess
        
        try:
            config_dir = config_loader.config_path.parent
            if os.name == 'nt':  # Windows
                subprocess.run(['explorer', str(config_dir)])
            elif os.name == 'posix':  # macOS/Linux
                subprocess.run(['open' if sys.platform == 'darwin' else 'xdg-open', str(config_dir)])
                
            self.status_text.value = "Ubicación del archivo abierta"
            e.page.update()
            
        except Exception as error:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"Error al abrir ubicación: {error}"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            self.status_text.value = f"Error: {error}"
            e.page.update()