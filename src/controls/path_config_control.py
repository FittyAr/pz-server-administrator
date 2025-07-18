import flet as ft
import os
from utils.config_loader import config_loader
from utils.platform_utils import platform_utils

class PathConfigControl:
    """
    Control para configurar rutas personalizadas de servidores y del juego
    """
    
    def __init__(self):
        self.server_path_field = None
        self.game_path_field = None
        self.status_text = None
        self.platform_info = None
        
    def build(self):
        # Obtener configuración actual
        server_config = config_loader.get_server_config()
        current_server_path = server_config.get('custom_server_path', '') if server_config else ''
        current_game_path = server_config.get('custom_game_path', '') if server_config else ''
        
        # Obtener rutas por defecto del sistema
        default_paths = platform_utils.get_default_zomboid_paths()
        system_info = platform_utils.get_system_info()
        
        # Campo para ruta de servidores
        self.server_path_field = ft.TextField(
            label="Ruta de servidores personalizada",
            hint_text=f"Ejemplo: {default_paths['server_path']}",
            value=current_server_path,
            expand=True,
            helper_text="Deja vacío para usar la ruta por defecto del sistema"
        )
        
        # Campo para ruta del juego
        self.game_path_field = ft.TextField(
            label="Ruta del juego personalizada",
            hint_text=f"Ejemplo: {default_paths['game_path']}",
            value=current_game_path,
            expand=True,
            helper_text="Deja vacío para búsqueda automática"
        )
        
        # Texto de estado
        self.status_text = ft.Text(
            "",
            color=ft.Colors.GREEN,
            size=12
        )
        
        # Información de la plataforma
        self.platform_info = ft.Container(
            content=ft.Column([
                ft.Text(
                    f"Sistema operativo: {system_info['system'].title()}",
                    size=12,
                    weight=ft.FontWeight.BOLD
                ),
                ft.Text(
                    f"Plataforma: {system_info['platform']}",
                    size=10,
                    color=ft.Colors.ON_SURFACE_VARIANT
                ),
                ft.Text(
                    f"Arquitectura: {system_info['architecture']}",
                    size=10,
                    color=ft.Colors.ON_SURFACE_VARIANT
                )
            ]),
            bgcolor=ft.Colors.SURFACE,
            padding=10,
            border_radius=8
        )
        
        return ft.Container(
            content=ft.Column([
                ft.Text(
                    "Configuración de rutas",
                    size=20,
                    weight=ft.FontWeight.BOLD
                ),
                
                ft.Divider(),
                
                # Información de la plataforma
                self.platform_info,
                
                ft.Text(
                    "Rutas por defecto del sistema:",
                    size=14,
                    weight=ft.FontWeight.BOLD
                ),
                
                ft.Container(
                    content=ft.Column([
                        ft.Text(
                            f"Servidores: {default_paths['server_path']}",
                            size=12,
                            color=ft.Colors.ON_SURFACE_VARIANT
                        ),
                        ft.Text(
                            f"Juego: {default_paths['game_path']}",
                            size=12,
                            color=ft.Colors.ON_SURFACE_VARIANT
                        ),
                        ft.Text(
                            f"Ejecutable: {default_paths['executable']}",
                            size=12,
                            color=ft.Colors.ON_SURFACE_VARIANT
                        )
                    ]),
                    bgcolor=ft.Colors.SURFACE,
                    padding=10,
                    border_radius=8,
                    margin=ft.margin.only(bottom=20)
                ),
                
                ft.Text(
                    "Rutas personalizadas (opcional):",
                    size=14,
                    weight=ft.FontWeight.BOLD
                ),
                
                # Campo de ruta de servidores con botón de explorar
                ft.Row([
                    self.server_path_field,
                    ft.IconButton(
                        icon=ft.Icons.FOLDER_OPEN,
                        tooltip="Explorar carpeta",
                        on_click=lambda e: self._browse_folder(e, "server")
                    )
                ]),
                
                # Campo de ruta del juego con botón de explorar
                ft.Row([
                    self.game_path_field,
                    ft.IconButton(
                        icon=ft.Icons.FOLDER_OPEN,
                        tooltip="Explorar carpeta",
                        on_click=lambda e: self._browse_folder(e, "game")
                    )
                ]),
                
                # Botones de acción
                ft.Row([
                    ft.ElevatedButton(
                        "Guardar configuración",
                        icon=ft.Icons.SAVE,
                        on_click=self._save_config
                    ),
                    ft.OutlinedButton(
                        "Restablecer por defecto",
                        icon=ft.Icons.RESTORE,
                        on_click=self._reset_to_default
                    ),
                    ft.TextButton(
                        "Probar rutas",
                        icon=ft.Icons.CHECK_CIRCLE,
                        on_click=self._test_paths
                    )
                ], spacing=10),
                
                # Estado
                self.status_text,
                
                ft.Divider(),
                
                # Información adicional
                ft.ExpansionTile(
                    title=ft.Text("Información adicional"),
                    subtitle=ft.Text("Detalles sobre la configuración de rutas"),
                    controls=[
                        ft.Container(
                            content=ft.Column([
                                ft.Text(
                                    "• La ruta de servidores debe contener los archivos .ini y .lua de configuración",
                                    size=12
                                ),
                                ft.Text(
                                    "• La ruta del juego debe contener el ejecutable de Project Zomboid",
                                    size=12
                                ),
                                ft.Text(
                                    "• Si dejas los campos vacíos, se usarán las rutas por defecto del sistema",
                                    size=12
                                ),
                                ft.Text(
                                    "• Los servidores deben tener 4 archivos obligatorios: .ini, _SandBoxVars.lua, _spawnpoints.lua, _spawnregions.lua",
                                    size=12,
                                    weight=ft.FontWeight.BOLD,
                                    color=ft.Colors.PRIMARY
                                )
                            ]),
                            padding=10
                        )
                    ]
                )
            ], spacing=10),
            padding=20
        )
    
    def _browse_folder(self, e, folder_type: str):
        """
        Abre un diálogo para seleccionar carpeta
        """
        # Nota: En una implementación real, aquí se abriría un diálogo de selección de carpeta
        # Por ahora, mostramos un mensaje informativo
        self._show_message(
            "Selección de carpeta",
            f"En una implementación completa, aquí se abriría un diálogo para seleccionar la carpeta de {folder_type}.",
            ft.Colors.BLUE
        )
    
    def _save_config(self, e):
        """
        Guarda la configuración de rutas
        """
        try:
            server_path = self.server_path_field.value.strip()
            game_path = self.game_path_field.value.strip()
            
            # Normalizar rutas si no están vacías
            if server_path:
                server_path = platform_utils.normalize_path(server_path)
            if game_path:
                game_path = platform_utils.normalize_path(game_path)
            
            # Actualizar configuración
            success = True
            if server_path != config_loader.get_server_config('custom_server_path'):
                success &= config_loader.update_config('server_config', 'custom_server_path', server_path)
            
            if game_path != config_loader.get_server_config('custom_game_path'):
                success &= config_loader.update_config('server_config', 'custom_game_path', game_path)
            
            if success:
                self._show_message(
                    "Configuración guardada exitosamente",
                    "Las rutas personalizadas han sido guardadas.",
                    ft.Colors.GREEN
                )
            else:
                self._show_message(
                    "Error al guardar",
                    "No se pudo guardar la configuración.",
                    ft.Colors.ERROR
                )
        
        except Exception as ex:
            self._show_message(
                "Error",
                f"Error al guardar la configuración: {str(ex)}",
                ft.Colors.ERROR
            )
    
    def _reset_to_default(self, e):
        """
        Restablece las rutas a los valores por defecto
        """
        self.server_path_field.value = ""
        self.game_path_field.value = ""
        
        # Guardar configuración vacía
        config_loader.update_config('server_config', 'custom_server_path', "")
        config_loader.update_config('server_config', 'custom_game_path', "")
        
        self._show_message(
            "Configuración restablecida",
            "Se han restablecido las rutas por defecto del sistema.",
            ft.Colors.BLUE
        )
        
        # self.update()  # No disponible sin UserControl
    
    def _test_paths(self, e):
        """
        Prueba las rutas configuradas
        """
        try:
            server_path = self.server_path_field.value.strip()
            game_path = self.game_path_field.value.strip()
            
            # Usar rutas por defecto si están vacías
            default_paths = platform_utils.get_default_zomboid_paths()
            
            test_server_path = server_path if server_path else default_paths['server_path']
            test_game_path = game_path if game_path else default_paths['game_path']
            
            # Normalizar rutas
            test_server_path = platform_utils.normalize_path(test_server_path)
            test_game_path = platform_utils.normalize_path(test_game_path)
            
            results = []
            
            # Probar ruta de servidores
            if os.path.exists(test_server_path):
                results.append(f"✓ Ruta de servidores válida: {test_server_path}")
                
                # Buscar servidores
                servers = config_loader.scan_server_directory(test_server_path)
                if servers:
                    valid_servers = [k for k, v in servers.items() if v.get('valid', False)]
                    invalid_servers = [k for k, v in servers.items() if not v.get('valid', False)]
                    
                    results.append(f"  - Servidores válidos encontrados: {len(valid_servers)}")
                    if invalid_servers:
                        results.append(f"  - Servidores incompletos: {len(invalid_servers)}")
                else:
                    results.append("  - No se encontraron servidores")
            else:
                results.append(f"✗ Ruta de servidores no existe: {test_server_path}")
            
            # Probar ruta del juego
            if os.path.exists(test_game_path):
                results.append(f"✓ Ruta del juego válida: {test_game_path}")
                
                # Buscar ejecutable
                executable = platform_utils.find_pz_executable([test_game_path])
                if executable and os.path.exists(executable):
                    results.append(f"  - Ejecutable encontrado: {executable}")
                else:
                    results.append("  - Ejecutable no encontrado")
            else:
                results.append(f"✗ Ruta del juego no existe: {test_game_path}")
            
            # Mostrar resultados
            results_text = "\n".join(results)
            self._show_message(
                "Resultados de la prueba",
                results_text,
                ft.Colors.BLUE
            )
        
        except Exception as ex:
            self._show_message(
                "Error en la prueba",
                f"Error al probar las rutas: {str(ex)}",
                ft.Colors.ERROR
            )
    
    def _show_message(self, title: str, message: str, color: str):
        """
        Muestra un mensaje de estado
        """
        self.status_text.value = f"{title}: {message}"
        self.status_text.color = color
        # self.update()  # No disponible sin UserControl
        
        # Limpiar mensaje después de 5 segundos
        def clear_message():
            self.status_text.value = ""
            # self.update()  # No disponible sin UserControl
        
        # En una implementación real, usarías un timer
        # Por ahora, el mensaje permanece hasta la próxima acción