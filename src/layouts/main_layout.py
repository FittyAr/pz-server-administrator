import flet as ft
from controls.server_control import ServerControl
from controls.config_control import ConfigControl
from controls.players_control import PlayersControl
from controls.logs_control import LogsControl
from controls.backup_control import BackupControl
from controls.app_config_control import AppConfigControl
from controls.path_config_control import PathConfigControl
from controls.config_file_buttons_control import ConfigFileButtonsControl
from controls.edit_mode_control import EditModeControl
from utils.config_loader import config_loader


class MainLayout:
    """
    Layout principal de la aplicación con NavigationRail
    """
    
    def __init__(self, page: ft.Page):
        self.page = page
        self.selected_index = 0
        self.content_area = ft.Container(
            content=ft.Text("Cargando...", size=20),
            expand=True,
            padding=20,
            bgcolor=ft.Colors.SURFACE,
            border_radius=12,
            margin=ft.margin.all(8)
        )
        
        # Cargar preferencias de UI
        self.ui_preferences = config_loader.get_ui_preferences() or {}
        
        # Estado de visibilidad de botones de configuración
        self.config_buttons_visible = False
        
        # Inicializar controles
        self.server_control = ServerControl()
        self.config_control = ConfigControl()
        self.players_control = PlayersControl()
        self.logs_control = LogsControl()
        self.backup_control = BackupControl()
        self.app_config_control = AppConfigControl()
        self.path_config_control = PathConfigControl()
        
        # Control de botones de archivos de configuración
        self.config_file_buttons_control = ConfigFileButtonsControl(
            on_config_file_click=self._on_config_file_click
        )
        
        # Control de modo de edición
        self.edit_mode_control = EditModeControl(
            on_mode_change=self._on_edit_mode_change
        )
        
        # Estado del modo de edición
        self.show_edit_mode_control = False
        
        # Configurar callback del selector de servidor
        if hasattr(self.server_control, 'server_selector'):
            original_callback = self.server_control.server_selector.on_server_change
            def combined_callback(server_id):
                original_callback(server_id)
                self._notify_server_selection(server_id)
            self.server_control.server_selector.on_server_change = combined_callback
        
        # Inicializar servidor seleccionado
        self.selected_server_id = config_loader.initialize_selected_server()
        self.selected_server_text = ft.Text(
            self._get_server_display_name(),
            size=14,
            weight=ft.FontWeight.W_500,
            color=ft.Colors.ON_SURFACE
        )
        
        # Si hay un servidor seleccionado, notificar a todos los controles
        if self.selected_server_id:
            self._notify_server_selection(self.selected_server_id)
        
        # Cargar contenido inicial
        self._update_content()
    
    def _notify_server_selection(self, server_id):
        """Notificar a todos los controles sobre el cambio de servidor"""
        print(f"DEBUG: _notify_server_selection llamado con server_id: {server_id}")
        self.selected_server_id = server_id
        
        # Notificar a todos los controles
        self.config_control.set_server(server_id)
        self.players_control.set_server(server_id)
        self.logs_control.set_server(server_id)
        self.backup_control.set_server(server_id)
        
        # Obtener la ruta del servidor y notificar al control de botones
        server_path = None
        server_name = None
        if server_id:
            from utils.config_loader import config_loader
            servers = config_loader.get_all_servers()
            if server_id in servers:
                server_path = servers[server_id].get('server_path')
                server_name = servers[server_id].get('name')
        
        print(f"DEBUG: Actualizando server_path a: {server_path}, server_name: {server_name}")
        self.config_file_buttons_control.update_server_path(server_path, server_name)
        
        # Actualizar la interfaz
        self._update_content()
    

    
    def _on_config_file_click(self, file_type: str):
        """Manejar clic en botones de archivos de configuración"""
        # Cambiar a la sección de configuración usando índice especial
        self.selected_index = 99
        
        # Si es server config (ini), mostrar el control de modo de edición
        if file_type == "ini":
            self.show_edit_mode_control = True
            # Configurar el modo de edición en el control de configuración
            if hasattr(self.config_control, 'set_edit_mode'):
                current_mode = self.edit_mode_control.get_current_mode()
                # Invertir la lógica: simple -> advanced, advanced -> simple
                config_mode = "advanced" if current_mode == "simple" else "simple"
                self.config_control.set_edit_mode(config_mode)
        else:
            self.show_edit_mode_control = False
        
        # Actualizar el tipo de archivo en el control de configuración
        if hasattr(self.config_control, 'set_file_type'):
            self.config_control.set_file_type(file_type)
        
        # Actualizar la interfaz
        self._update_content()
        if hasattr(self, 'page') and self.page:
            self.page.update()
    
    def _on_edit_mode_change(self, mode: str):
        """Manejar cambio en el modo de edición"""
        if hasattr(self.config_control, 'set_edit_mode'):
            # Invertir la lógica: simple -> advanced, advanced -> simple
            config_mode = "advanced" if mode == "simple" else "simple"
            self.config_control.set_edit_mode(config_mode)
            
            # Actualizar la interfaz si estamos en la sección de configuración
            if self.selected_index == 99:
                self._update_content()
                if hasattr(self, 'page') and self.page:
                    self.page.update()

    def _toggle_theme(self, e):
        """
        Alterna entre tema claro y oscuro
        """
        current_theme = config_loader.get_app_config('theme')
        new_theme = 'light' if current_theme == 'dark' else 'dark'
        
        # Actualizar configuración
        config_loader.update_app_config('theme', new_theme)
        
        # Aplicar nuevo tema
        self.page.theme_mode = ft.ThemeMode.LIGHT if new_theme == 'light' else ft.ThemeMode.DARK
        self.page.update()
        
        # Mostrar notificación
        theme_name = "Claro" if new_theme == 'light' else "Oscuro"
        self.page.snack_bar = ft.SnackBar(
            content=ft.Text(f"Tema cambiado a: {theme_name}"),
            bgcolor=ft.Colors.PRIMARY
        )
        self.page.snack_bar.open = True
        self.page.update()
    
    def _get_server_display_name(self) -> str:
        """
        Obtiene el nombre del servidor seleccionado para mostrar
        """
        if not self.selected_server_id:
            return "Ningún servidor seleccionado"
        
        # Obtener información del servidor
        servers = config_loader.get_server_config('servers') or {}
        server_info = servers.get(self.selected_server_id, {})
        server_name = server_info.get('name', self.selected_server_id)
        
        return f"Servidor: {server_name}"
    
    def update_selected_server_display(self):
        """
        Actualiza la visualización del servidor seleccionado
        """
        self.selected_server_text.value = self._get_server_display_name()
        self.page.update()
        
        # Notificar a todos los controles que el servidor ha cambiado
        self._notify_server_change()
    
    def _notify_server_change(self):
        """
        Notifica a todos los controles que el servidor seleccionado ha cambiado
        """
        # Notificar al control de configuración
        if hasattr(self, 'config_control'):
            self.config_control.refresh_for_selected_server()
        
        # Actualizar estado de botones de configuración
        if self.config_buttons_visible:
            self._update_config_buttons_state()
        
        # Aquí se pueden agregar más controles en el futuro
        # if hasattr(self, 'other_control'):
        #     self.other_control.refresh_for_selected_server()
    
    def _create_header(self):
        """
        Crea el header de la aplicación con botón de tema y servidor seleccionado
        """
        current_theme = config_loader.get_app_config('theme')
        theme_icon = ft.Icons.LIGHT_MODE if current_theme == 'dark' else ft.Icons.DARK_MODE
        theme_tooltip = "Cambiar a modo claro" if current_theme == 'dark' else "Cambiar a modo oscuro"
        
        return ft.Container(
            content=ft.Row([
                ft.Text(
                    "PZ Server Administrator",
                    size=24,
                    weight=ft.FontWeight.BOLD,
                    color=ft.Colors.PRIMARY
                ),
                ft.Container(expand=True),
                ft.Container(
                    content=ft.Row([
                        ft.Icon(
                            ft.Icons.COMPUTER,
                            size=16,
                            color=ft.Colors.PRIMARY
                        ),
                        self.selected_server_text
                    ], spacing=8),
                    padding=ft.padding.symmetric(horizontal=12, vertical=6),
                    bgcolor=ft.Colors.PRIMARY_CONTAINER,
                    border_radius=8
                ),
                ft.IconButton(
                    icon=theme_icon,
                    tooltip=theme_tooltip,
                    on_click=self._toggle_theme,
                    icon_size=24,
                    style=ft.ButtonStyle(
                        bgcolor=ft.Colors.PRIMARY_CONTAINER,
                        color=ft.Colors.ON_PRIMARY_CONTAINER,
                        shape=ft.RoundedRectangleBorder(radius=8)
                    )
                )
            ], spacing=16),
            padding=ft.padding.symmetric(horizontal=20, vertical=16),
            bgcolor=ft.Colors.SURFACE,
            border=ft.border.only(bottom=ft.BorderSide(1, ft.Colors.OUTLINE))
        )
    
    def _on_navigation_change(self, e):
        """
        Maneja el cambio de navegación
        """
        new_index = e.control.selected_index
        
        # Cambiar a la nueva sección
        self.selected_index = new_index
        self._update_content()
        
        e.page.update()
    
    def _on_manual_navigation_click(self, index: int):
        """
        Maneja el clic en botones de navegación personalizados
        """
        self.selected_index = index
        self.show_edit_mode_control = False  # Ocultar control de modo de edición al cambiar sección
        self._update_content()
        
        if hasattr(self, 'page') and self.page:
            self.page.update()
    
    def _update_content(self):
        """Actualiza el contenido del área principal según la selección"""
        if self.selected_index == 0:
            self.content_area.content = self.server_control.build()
        elif self.selected_index == 1:
            self.content_area.content = self.players_control.build()
        elif self.selected_index == 2:
            self.content_area.content = self.logs_control.build()
        elif self.selected_index == 3:
            self.content_area.content = self.backup_control.build()
        elif self.selected_index == 4:
            self.content_area.content = self.path_config_control.build()
        elif self.selected_index == 5:
            self.content_area.content = self.app_config_control.build()
        elif self.selected_index == 99:  # Índice especial para configuración
            # Si se debe mostrar el control de modo de edición, incluirlo
            if self.show_edit_mode_control:
                config_content = self.config_control.build()
                edit_mode_content = self.edit_mode_control.get_control()
                
                self.content_area.content = ft.Column([
                    edit_mode_content,
                    ft.Divider(height=1, color=ft.Colors.OUTLINE),
                    config_content
                ], spacing=16, expand=True)
            else:
                self.content_area.content = self.config_control.build()
    
    def build(self):
        """
        Construye y retorna el layout principal
        """
        # Crear NavigationRail con configuración desde JSON
        extended = self.ui_preferences.get('navigation_rail_extended', True)
        
        # Crear el NavigationRail
        nav_rail = ft.NavigationRail(
            selected_index=self.selected_index,
            label_type=ft.NavigationRailLabelType.ALL,
            min_width=100,
            min_extended_width=200,
            group_alignment=-0.9,
            extended=extended,
            bgcolor=ft.Colors.SURFACE,
            destinations=[
                ft.NavigationRailDestination(
                    icon=ft.Icons.COMPUTER,
                    selected_icon=ft.Icons.COMPUTER,
                    label="Servidor",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.PEOPLE,
                    selected_icon=ft.Icons.PEOPLE,
                    label="Jugadores",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.ARTICLE,
                    selected_icon=ft.Icons.ARTICLE,
                    label="Logs",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.BACKUP,
                    selected_icon=ft.Icons.BACKUP,
                    label="Respaldos",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.FOLDER_OPEN,
                    selected_icon=ft.Icons.FOLDER_OPEN,
                    label="Rutas",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.SETTINGS_APPLICATIONS,
                    selected_icon=ft.Icons.SETTINGS_APPLICATIONS,
                    label="Config App",
                ),
            ],
            on_change=self._on_navigation_change,
        )
        
        # Crear contenedor del navigation rail con botones de configuración entre Servidor y Jugadores
        navigation_content = ft.Column([
            # Botón Servidor (índice 0)
            ft.Container(
                content=ft.Row([
                    ft.Icon(ft.Icons.COMPUTER, size=20, color=ft.Colors.PRIMARY if self.selected_index == 0 else ft.Colors.ON_SURFACE),
                    ft.Text("Servidor", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 0 else ft.Colors.ON_SURFACE)
                ], spacing=8),
                padding=ft.padding.symmetric(horizontal=12, vertical=10),
                bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 0 else None,
                border_radius=8,
                margin=ft.margin.symmetric(horizontal=8, vertical=2),
                on_click=lambda e: self._on_manual_navigation_click(0)
            ),
            
            # Control de botones de archivos de configuración
            self.config_file_buttons_control.get_control(),
            
            # Resto de botones de navegación
            ft.Container(
                content=ft.Column([
                    # Jugadores (índice 1)
                    ft.Container(
                        content=ft.Row([
                            ft.Icon(ft.Icons.PEOPLE, size=20, color=ft.Colors.PRIMARY if self.selected_index == 1 else ft.Colors.ON_SURFACE),
                            ft.Text("Jugadores", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 1 else ft.Colors.ON_SURFACE)
                        ], spacing=8),
                        padding=ft.padding.symmetric(horizontal=12, vertical=10),
                        bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 1 else None,
                        border_radius=8,
                        margin=ft.margin.symmetric(horizontal=8, vertical=2),
                        on_click=lambda e: self._on_manual_navigation_click(1)
                    ),
                    # Logs (índice 2)
                    ft.Container(
                        content=ft.Row([
                            ft.Icon(ft.Icons.ARTICLE, size=20, color=ft.Colors.PRIMARY if self.selected_index == 2 else ft.Colors.ON_SURFACE),
                            ft.Text("Logs", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 2 else ft.Colors.ON_SURFACE)
                        ], spacing=8),
                        padding=ft.padding.symmetric(horizontal=12, vertical=10),
                        bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 2 else None,
                        border_radius=8,
                        margin=ft.margin.symmetric(horizontal=8, vertical=2),
                        on_click=lambda e: self._on_manual_navigation_click(2)
                    ),
                    # Respaldos (índice 3)
                    ft.Container(
                        content=ft.Row([
                            ft.Icon(ft.Icons.BACKUP, size=20, color=ft.Colors.PRIMARY if self.selected_index == 3 else ft.Colors.ON_SURFACE),
                            ft.Text("Respaldos", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 3 else ft.Colors.ON_SURFACE)
                        ], spacing=8),
                        padding=ft.padding.symmetric(horizontal=12, vertical=10),
                        bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 3 else None,
                        border_radius=8,
                        margin=ft.margin.symmetric(horizontal=8, vertical=2),
                        on_click=lambda e: self._on_manual_navigation_click(3)
                    ),
                    # Rutas (índice 4)
                    ft.Container(
                        content=ft.Row([
                            ft.Icon(ft.Icons.FOLDER_OPEN, size=20, color=ft.Colors.PRIMARY if self.selected_index == 4 else ft.Colors.ON_SURFACE),
                            ft.Text("Rutas", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 4 else ft.Colors.ON_SURFACE)
                        ], spacing=8),
                        padding=ft.padding.symmetric(horizontal=12, vertical=10),
                        bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 4 else None,
                        border_radius=8,
                        margin=ft.margin.symmetric(horizontal=8, vertical=2),
                        on_click=lambda e: self._on_manual_navigation_click(4)
                    ),
                    # Config App (índice 5)
                    ft.Container(
                        content=ft.Row([
                            ft.Icon(ft.Icons.SETTINGS_APPLICATIONS, size=20, color=ft.Colors.PRIMARY if self.selected_index == 5 else ft.Colors.ON_SURFACE),
                            ft.Text("Config App", size=14, weight=ft.FontWeight.W_500, color=ft.Colors.PRIMARY if self.selected_index == 5 else ft.Colors.ON_SURFACE)
                        ], spacing=8),
                        padding=ft.padding.symmetric(horizontal=12, vertical=10),
                        bgcolor=ft.Colors.PRIMARY_CONTAINER if self.selected_index == 5 else None,
                        border_radius=8,
                        margin=ft.margin.symmetric(horizontal=8, vertical=2),
                        on_click=lambda e: self._on_manual_navigation_click(5)
                    )
                ], spacing=4),
                expand=True
            )
        ], spacing=8, expand=True)
        
        navigation_rail = ft.Container(
            content=navigation_content,
            bgcolor=ft.Colors.SURFACE,
            border=ft.border.only(right=ft.BorderSide(1, ft.Colors.OUTLINE)),
            width=200
        )
        
        # Crear el área principal con mejor estructura
        main_content = ft.Container(
            content=ft.Column([
                self._create_header(),
                ft.Container(
                    content=self.content_area,
                    expand=True,
                    bgcolor=ft.Colors.SURFACE,
                    padding=8
                )
            ]),
            expand=True,
            bgcolor=ft.Colors.SURFACE
        )
        
        return ft.Row(
            [
                navigation_rail,
                main_content,
            ],
            expand=True,
            spacing=0
        )