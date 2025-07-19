import flet as ft
from utils.config_loader import config_loader
from utils.platform_utils import platform_utils


class ServerSelectorControl:
    """
    Control para la selección y gestión de servidores detectados
    """
    
    def __init__(self, on_server_change=None):
        self.on_server_change = on_server_change
        # Inicializar con el servidor favorito o seleccionado
        self.selected_server_id = config_loader.initialize_selected_server()
        self.servers_list = ft.ListView(
            expand=True,
            spacing=5,
            padding=10
        )
        self.refresh_servers()
    
    def refresh_servers(self):
        """
        Actualiza la lista de servidores detectados
        """
        self.servers_list.controls.clear()
        
        # Obtener todos los servidores
        all_servers = config_loader.get_all_servers()
        default_server_id = config_loader.get_default_server_id()
        
        if not all_servers:
            self.servers_list.controls.append(
                ft.Container(
                    content=ft.Column([
                        ft.Icon(ft.Icons.SEARCH_OFF, size=48, color=ft.Colors.GREY),
                        ft.Text(
                            "No se encontraron servidores",
                            size=16,
                            color=ft.Colors.GREY,
                            text_align=ft.TextAlign.CENTER
                        ),
                        ft.Text(
                            "Verifica que tengas servidores configurados en tu carpeta Zomboid/Server",
                            size=12,
                            color=ft.Colors.GREY,
                            text_align=ft.TextAlign.CENTER
                        )
                    ], horizontal_alignment=ft.CrossAxisAlignment.CENTER),
                    padding=20,
                    alignment=ft.alignment.center
                )
            )
            return
        
        # Crear tarjetas para cada servidor
        for server_id, server_config in all_servers.items():
            server_card = self._build_server_card(server_id, server_config)
            self.servers_list.controls.append(server_card)
    
    def _build_server_card(self, server_id: str, server_data: dict) -> ft.Card:
        """
        Construye una tarjeta para mostrar información del servidor
        """
        is_favorite = server_id == config_loader.get_favorite_server()
        is_selected = server_id == config_loader.get_selected_server()
        is_auto_detected = server_data.get('auto_detected', False)
        is_valid = server_data.get('valid', True)
        
        # Indicadores visuales
        indicators = []
        if is_favorite:
            indicators.append(
                ft.Container(
                    content=ft.Icon(ft.Icons.STAR, color=ft.Colors.AMBER, size=16),
                    tooltip="Servidor favorito"
                )
            )
        
        if is_selected:
            indicators.append(
                ft.Container(
                    content=ft.Icon(ft.Icons.RADIO_BUTTON_CHECKED, color=ft.Colors.GREEN, size=16),
                    tooltip="Servidor actualmente seleccionado"
                )
            )
        
        if is_auto_detected:
            indicators.append(
                ft.Container(
                    content=ft.Icon(ft.Icons.SEARCH, color=ft.Colors.BLUE, size=16),
                    tooltip="Detectado automáticamente"
                )
            )
        
        # Mostrar advertencia solo si faltan archivos .lua (información, no error)
        if 'missing_lua_files' in server_data and server_data['missing_lua_files']:
            indicators.append(
                ft.Container(
                    content=ft.Icon(ft.Icons.INFO, color=ft.Colors.ORANGE, size=16),
                    tooltip="Servidor parcial - faltan algunos archivos .lua"
                )
            )
        
        # Información de validación y estado del servidor
        validation_info = []
        
        # Mostrar estado del servidor
        if 'server_status' in server_data:
            status_color = ft.Colors.GREEN if server_data['server_status'] == "Configurado" else ft.Colors.ORANGE
            validation_info.append(
                ft.Text(
                    f"Estado: {server_data['server_status']}",
                    size=10,
                    color=status_color
                )
            )
        
        # Mostrar archivos .lua faltantes si los hay
        if 'missing_lua_files' in server_data and server_data['missing_lua_files']:
            validation_info.append(
                ft.Text(
                    f"Archivos .lua faltantes: {', '.join(server_data['missing_lua_files'])}",
                    size=10,
                    color=ft.Colors.ORANGE
                )
            )
        
        # Mostrar archivos .lua existentes
        if 'existing_lua_files' in server_data and server_data['existing_lua_files']:
            validation_info.append(
                ft.Text(
                    f"Archivos .lua presentes: {', '.join(server_data['existing_lua_files'])}",
                    size=10,
                    color=ft.Colors.GREEN
                )
            )
        
        # Información del sistema operativo
        system_info = platform_utils.get_system_info()
        platform_indicator = ft.Container(
            content=ft.Text(
                f"SO: {system_info['system'].title()}",
                size=10,
                color=ft.Colors.ON_SURFACE_VARIANT
            ),
            tooltip=f"Plataforma: {system_info['platform']}"
        )
        
        # Botones de acción
        action_buttons = []
        
        # Todos los servidores con .ini son válidos y pueden ser seleccionados
        action_buttons.extend([
            ft.IconButton(
                icon=ft.Icons.STAR if is_favorite else ft.Icons.STAR_BORDER,
                tooltip="Marcar como favorito" if not is_favorite else "Quitar de favoritos",
                on_click=lambda e, sid=server_id: self._toggle_favorite(e, sid),
                style=ft.ButtonStyle(
                    color=ft.Colors.AMBER if is_favorite else ft.Colors.GREY
                )
            ),
            ft.IconButton(
                icon=ft.Icons.RADIO_BUTTON_CHECKED if is_selected else ft.Icons.RADIO_BUTTON_UNCHECKED,
                tooltip="Servidor activo" if is_selected else "Seleccionar como servidor activo",
                on_click=lambda e, sid=server_id: self._select_server(e, sid),
                style=ft.ButtonStyle(
                    color=ft.Colors.GREEN if is_selected else ft.Colors.GREY
                )
            )
        ])
        
        # Color de fondo según estado del servidor
        if 'server_status' in server_data:
            if server_data['server_status'] == "Configurado":
                card_color = None  # Color por defecto
            else:
                card_color = ft.Colors.AMBER_50  # Color suave para servidores parciales
        else:
            card_color = None
        
        return ft.Card(
             content=ft.Container(
                 content=ft.Column([
                     # Encabezado con nombre e indicadores
                     ft.Row([
                         ft.Container(
                             content=ft.Text(
                                 server_data.get('name', server_id),
                                 size=16,
                                 weight=ft.FontWeight.BOLD,
                                 color=ft.Colors.ON_ERROR_CONTAINER if not is_valid else None
                             ),
                             expand=True
                         ),
                         *indicators
                     ]),
                     
                     # Descripción
                     ft.Text(
                         server_data.get('description', 'Sin descripción'),
                         size=12,
                         color=ft.Colors.ON_SURFACE_VARIANT
                     ),
                     
                     # Ruta del servidor
                     ft.Text(
                         f"Ruta: {server_data.get('server_path', 'No especificada')}",
                         size=10,
                         color=ft.Colors.ON_SURFACE_VARIANT
                     ),
                     
                     # Información de validación
                     *validation_info,
                     
                     # Información de plataforma y botones
                     ft.Row([
                         platform_indicator,
                         ft.Container(expand=True),
                         *action_buttons
                     ])
                 ]),
                 padding=16,
                 bgcolor=card_color
             )
         )
    
    def _toggle_favorite(self, e, server_id: str):
        """
        Alterna el estado de favorito de un servidor
        """
        current_favorite = config_loader.get_favorite_server()
        
        if current_favorite == server_id:
            # Quitar de favoritos (establecer como vacío)
            success = config_loader.set_favorite_server("")
            message = "Servidor removido de favoritos"
        else:
            # Establecer como favorito
            success = config_loader.set_favorite_server(server_id)
            message = "Servidor marcado como favorito"
        
        if success:
            # Mostrar notificación
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(message),
                bgcolor=ft.Colors.GREEN
            )
            e.page.snack_bar.open = True
            
            # Actualizar la lista
            self.refresh_servers()
            e.page.update()
        else:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("Error al actualizar servidor favorito"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            e.page.update()
    
    def _show_server_details(self, e, server_id: str):
        """
        Muestra detalles de un servidor incompleto
        """
        all_servers = config_loader.get_all_servers()
        server_data = all_servers.get(server_id, {})
        
        missing_files = server_data.get('missing_files', [])
        details_text = f"Servidor: {server_data.get('name', server_id)}\n\n"
        details_text += f"Ruta: {server_data.get('server_path', 'No especificada')}\n\n"
        
        if missing_files:
            details_text += "Archivos faltantes:\n"
            for file in missing_files:
                details_text += f"• {file}\n"
        
        # Mostrar diálogo con detalles
        dialog = ft.AlertDialog(
            title=ft.Text("Detalles del servidor"),
            content=ft.Text(details_text),
            actions=[
                ft.TextButton(
                    "Cerrar",
                    on_click=lambda _: self._close_dialog(e.page)
                )
            ]
        )
        
        e.page.dialog = dialog
        dialog.open = True
        e.page.update()
    
    def _close_dialog(self, page):
        """
        Cierra el diálogo actual
        """
        if page.dialog:
            page.dialog.open = False
            page.update()
    
    def _select_server(self, e, server_id: str):
        """
        Selecciona un servidor como activo para trabajar con él
        """
        # Actualizar servidor seleccionado en la configuración
        success = config_loader.set_selected_server(server_id)
        
        if success:
            self.selected_server_id = server_id
            
            # Actualizar la visualización en el layout principal si existe
            if hasattr(e.page, 'main_layout') and hasattr(e.page.main_layout, 'update_selected_server_display'):
                e.page.main_layout.selected_server_id = server_id
                e.page.main_layout.update_selected_server_display()
            
            # Actualizar toda la lista de servidores para reflejar el cambio
            self.refresh_servers()
            
            # Notificar cambio si hay callback
            if self.on_server_change:
                self.on_server_change(server_id)
            
            # Mostrar notificación
            server_name = config_loader.get_all_servers().get(server_id, {}).get('name', server_id)
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text(f"Servidor activo: {server_name}"),
                bgcolor=ft.Colors.BLUE
            )
            e.page.snack_bar.open = True
            e.page.update()
        else:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("Error al seleccionar servidor"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            e.page.update()
    
    def _scan_servers(self, e):
        """
        Escanea nuevamente el directorio en busca de servidores
        """
        # Forzar un nuevo escaneo
        config_loader.scan_server_directory()
        self.refresh_servers()
        
        e.page.snack_bar = ft.SnackBar(
            content=ft.Text("Escaneo de servidores completado"),
            bgcolor=ft.Colors.GREEN
        )
        e.page.snack_bar.open = True
        e.page.update()
    
    def get_selected_server_config(self):
        """
        Obtiene la configuración del servidor seleccionado
        """
        if not self.selected_server_id:
            # Si no hay servidor seleccionado, usar el que está en la configuración
            self.selected_server_id = config_loader.get_selected_server()
        
        if self.selected_server_id:
            all_servers = config_loader.get_all_servers()
            return all_servers.get(self.selected_server_id)
        
        return None
    
    def build(self):
        """
        Construye y retorna la interfaz del selector de servidores
        """
        return ft.Column([
            ft.Row([
                ft.Text(
                    "Servidores Detectados",
                    size=20,
                    weight=ft.FontWeight.BOLD
                ),
                ft.IconButton(
                    icon=ft.Icons.REFRESH,
                    tooltip="Escanear servidores",
                    on_click=self._scan_servers
                )
            ], alignment=ft.MainAxisAlignment.SPACE_BETWEEN),
            
            ft.Divider(),
            
            ft.Container(
                content=self.servers_list,
                height=400,
                border=ft.border.all(1, ft.Colors.OUTLINE),
                border_radius=8
            ),
            
            ft.Container(
                content=ft.Row([
                    ft.Icon(ft.Icons.INFO_OUTLINE, size=16, color=ft.Colors.BLUE),
                    ft.Container(
                        content=ft.Text(
                             "⭐ Favorito: Se selecciona automáticamente al iniciar la aplicación.\n"
                             "🔘 Activo: Servidor actualmente en uso en toda la aplicación.",
                             size=12,
                             color=ft.Colors.ON_SURFACE_VARIANT
                        ),
                        expand=True
                    )
                ]),
                padding=10,
                bgcolor=ft.Colors.SURFACE,
                border_radius=8,
                margin=ft.margin.only(top=10)
            )
        ], spacing=10)