import flet as ft
import os
from utils.config_loader import config_loader


class ConfigControl:
    """
    Control para la gestión de archivos de configuración del servidor
    """
    
    def __init__(self):
        self.selected_config_type = "server_settings"
        self.current_server_id = None
        self.config_content = ft.TextField(
            multiline=True,
            min_lines=20,
            max_lines=30,
            expand=True,
            value="# Selecciona un archivo de configuración para editarlo"
        )
        self.server_info_text = ft.Text(
            "Servidor: No seleccionado",
            size=14,
            color=ft.Colors.GREY
        )
        
        # Cargar contenido inicial
        self._load_config_content()
        
    def _on_config_type_change(self, e):
        """
        Maneja el cambio de tipo de configuración
        """
        self.selected_config_type = e.control.value
        self._load_config_content()
        e.page.update()
        
    def _update_server_info(self):
        """
        Actualiza la información del servidor seleccionado
        """
        self.current_server_id = config_loader.get_selected_server()
        if self.current_server_id:
            servers = config_loader.get_all_servers()
            server_data = servers.get(self.current_server_id, {})
            server_name = server_data.get('name', self.current_server_id)
            self.server_info_text.value = f"Servidor: {server_name}"
        else:
            self.server_info_text.value = "Servidor: No seleccionado"
    
    def _get_config_file_path(self, config_type: str) -> str:
        """
        Obtiene la ruta del archivo de configuración según el tipo
        """
        if not self.current_server_id:
            return None
            
        servers = config_loader.get_all_servers()
        server_data = servers.get(self.current_server_id, {})
        server_path = server_data.get('server_path')
        
        if not server_path:
            return None
            
        file_mappings = {
            "server_settings": f"{self.current_server_id}.ini",
            "sandbox_vars": f"{self.current_server_id}_sandbox_vars.lua",
            "spawn_regions": f"{self.current_server_id}_spawnregions.lua",
            "server_rules": f"{self.current_server_id}_rules.json"
        }
        
        filename = file_mappings.get(config_type)
        if filename:
            return os.path.join(server_path, filename)
        return None
    
    def _load_config_content(self):
        """
        Carga el contenido del archivo de configuración seleccionado
        """
        self._update_server_info()
        
        if not self.current_server_id:
            self.config_content.value = "# No hay servidor seleccionado\n# Selecciona un servidor en la sección 'Selector de Servidores'"
            return
            
        config_file_path = self._get_config_file_path(self.selected_config_type)
        
        if config_file_path and os.path.exists(config_file_path):
            try:
                with open(config_file_path, 'r', encoding='utf-8') as file:
                    self.config_content.value = file.read()
            except Exception as e:
                self.config_content.value = f"# Error al cargar el archivo: {str(e)}\n# Ruta: {config_file_path}"
        else:
            # Mostrar plantilla si el archivo no existe
            config_templates = {
                "server_settings": f"# Server Settings ({self.current_server_id}.ini)\n# Archivo no encontrado, creando plantilla\nPublicName=My PZ Server\nPublicDescription=A Project Zomboid Server\nMaxPlayers=32\nPVP=true\nPauseEmpty=true\nPingLimit=400\n",
                "sandbox_vars": f"-- Sandbox Variables ({self.current_server_id}_sandbox_vars.lua)\n-- Archivo no encontrado, creando plantilla\nSandBoxVars = {{\n    Version = 5,\n    Zombies = 4,\n    Distribution = 1,\n    DayLength = 3,\n    StartMonth = 7,\n    StartTime = 2,\n    WaterShut = 6,\n    ElecShut = 6,\n}}",
                "spawn_regions": f"-- Spawn Regions ({self.current_server_id}_spawnregions.lua)\n-- Archivo no encontrado, creando plantilla\nfunction SpawnRegions()\n    return {{\n        {{ name = \"Muldraugh, KY\", file = \"media/maps/Muldraugh, KY/spawnpoints.lua\" }},\n        {{ name = \"West Point, KY\", file = \"media/maps/West Point, KY/spawnpoints.lua\" }},\n        {{ name = \"Riverside, KY\", file = \"media/maps/Riverside, KY/spawnpoints.lua\" }},\n    }}\nend",
                "server_rules": f"{{\n  \"_comment\": \"Reglas del servidor {self.current_server_id} - Archivo no encontrado, creando plantilla\",\n  \"rules\": [\n    \"No griefing\",\n    \"No cheating\",\n    \"Respect other players\",\n    \"No offensive language\"\n  ],\n  \"punishments\": {{\n    \"warning\": 1,\n    \"kick\": 2,\n    \"ban\": 3\n  }}\n}}"
            }
            
            self.config_content.value = config_templates.get(
                self.selected_config_type, 
                f"# Archivo de configuración no encontrado para {self.current_server_id}"
            )
        
    def _save_config(self, e):
        """
        Guarda los cambios en el archivo de configuración
        """
        if not self.current_server_id:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text("No hay servidor seleccionado"),
                    bgcolor=ft.Colors.RED
                )
            )
            return
            
        config_file_path = self._get_config_file_path(self.selected_config_type)
        
        if not config_file_path:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text("No se pudo determinar la ruta del archivo"),
                    bgcolor=ft.Colors.RED
                )
            )
            return
            
        try:
            # Crear directorio si no existe
            os.makedirs(os.path.dirname(config_file_path), exist_ok=True)
            
            # Guardar archivo
            with open(config_file_path, 'w', encoding='utf-8') as file:
                file.write(self.config_content.value)
                
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Configuración guardada: {os.path.basename(config_file_path)}"),
                    bgcolor=ft.Colors.GREEN
                )
            )
        except Exception as ex:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Error al guardar: {str(ex)}"),
                    bgcolor=ft.Colors.RED
                )
            )
        
    def _reset_config(self, e):
        """
        Restaura la configuración a los valores por defecto
        """
        self._load_config_content()
        e.page.show_snack_bar(
            ft.SnackBar(
                content=ft.Text("Configuración restaurada"),
                bgcolor=ft.Colors.ORANGE
            )
        )
        e.page.update()
    
    def refresh_for_selected_server(self):
        """
        Actualiza el contenido para el servidor seleccionado
        Método llamado cuando cambia la selección de servidor
        """
        self._load_config_content()
    
    def build(self):
        """
        Construye y retorna la interfaz del control de configuración
        """
        return ft.Column(
            [
                ft.Text(
                    "Gestión de Configuraciones",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                self.server_info_text,
                ft.Divider(),
                
                # Selector de tipo de configuración
                ft.Row(
                    [
                        ft.Text("Archivo de configuración:", size=16),
                        ft.Dropdown(
                            width=300,
                            value=self.selected_config_type,
                            options=[
                                ft.dropdown.Option("server_settings", "Configuración del Servidor (.ini)"),
                                ft.dropdown.Option("sandbox_vars", "Variables Sandbox (.lua)"),
                                ft.dropdown.Option("spawn_regions", "Regiones de Spawn (.lua)"),
                                ft.dropdown.Option("server_rules", "Reglas del Servidor (.json)"),
                            ],
                            on_change=self._on_config_type_change
                        ),
                    ],
                    alignment=ft.MainAxisAlignment.START
                ),
                
                ft.Container(height=10),
                
                # Editor de configuración
                ft.Container(
                    content=self.config_content,
                    border=ft.border.all(1, ft.Colors.OUTLINE),
                    border_radius=8,
                    padding=10,
                    expand=True
                ),
                
                # Botones de acción
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Guardar Cambios",
                            icon=ft.Icons.SAVE,
                            on_click=self._save_config,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Restaurar",
                            icon=ft.Icons.RESTORE,
                            on_click=self._reset_config,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Validar Sintaxis",
                            icon=ft.Icons.CHECK_CIRCLE,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
            ],
            spacing=10,
            expand=True
        )