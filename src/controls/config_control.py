import flet as ft


class ConfigControl:
    """
    Control para la gestión de archivos de configuración del servidor
    """
    
    def __init__(self):
        self.selected_config_type = "server_settings"
        self.config_content = ft.TextField(
            multiline=True,
            min_lines=20,
            max_lines=30,
            expand=True,
            value="# Selecciona un archivo de configuración para editarlo"
        )
        
    def _on_config_type_change(self, e):
        """
        Maneja el cambio de tipo de configuración
        """
        self.selected_config_type = e.control.value
        self._load_config_content()
        e.page.update()
        
    def _load_config_content(self):
        """
        Carga el contenido del archivo de configuración seleccionado
        """
        # TODO: Implementar carga real de archivos
        config_templates = {
            "server_settings": "# Server Settings (servertest.ini)\nPublicName=My PZ Server\nPublicDescription=A Project Zomboid Server\nMaxPlayers=32\nPVP=true\nPauseEmpty=true\nPingLimit=400\n",
            "sandbox_vars": "-- Sandbox Variables (sandbox_vars.lua)\nSandBoxVars = {\n    Version = 5,\n    Zombies = 4,\n    Distribution = 1,\n    DayLength = 3,\n    StartMonth = 7,\n    StartTime = 2,\n    WaterShut = 6,\n    ElecShut = 6,\n}",
            "spawn_regions": "-- Spawn Regions (spawnregions.lua)\nfunction SpawnRegions()\n    return {\n        { name = \"Muldraugh, KY\", file = \"media/maps/Muldraugh, KY/spawnpoints.lua\" },\n        { name = \"West Point, KY\", file = \"media/maps/West Point, KY/spawnpoints.lua\" },\n        { name = \"Riverside, KY\", file = \"media/maps/Riverside, KY/spawnpoints.lua\" },\n    }\nend",
            "server_rules": "{\n  \"rules\": [\n    \"No griefing\",\n    \"No cheating\",\n    \"Respect other players\",\n    \"No offensive language\"\n  ],\n  \"punishments\": {\n    \"warning\": 1,\n    \"kick\": 2,\n    \"ban\": 3\n  }\n}"
        }
        
        self.config_content.value = config_templates.get(
            self.selected_config_type, 
            "# Archivo de configuración no encontrado"
        )
        
    def _save_config(self, e):
        """
        Guarda los cambios en el archivo de configuración
        """
        # TODO: Implementar guardado real de archivos
        e.page.show_snack_bar(
            ft.SnackBar(
                content=ft.Text("Configuración guardada exitosamente"),
                bgcolor=ft.colors.GREEN
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
                bgcolor=ft.colors.ORANGE
            )
        )
        e.page.update()
    
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
                    border=ft.border.all(1, ft.colors.OUTLINE),
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
                            bgcolor=ft.colors.BLUE,
                            color=ft.colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Restaurar",
                            icon=ft.Icons.RESTORE,
                            on_click=self._reset_config,
                            bgcolor=ft.colors.ORANGE,
                            color=ft.colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Validar Sintaxis",
                            icon=ft.Icons.CHECK_CIRCLE,
                            bgcolor=ft.colors.GREEN,
                            color=ft.colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
            ],
            spacing=10,
            expand=True
        )