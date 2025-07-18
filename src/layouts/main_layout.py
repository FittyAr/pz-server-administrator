import flet as ft
from controls.server_control import ServerControl
from controls.config_control import ConfigControl
from controls.players_control import PlayersControl
from controls.logs_control import LogsControl
from controls.backup_control import BackupControl
from controls.app_config_control import AppConfigControl
from controls.path_config_control import PathConfigControl
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
        
        # Inicializar controles
        self.server_control = ServerControl()
        self.config_control = ConfigControl()
        self.players_control = PlayersControl()
        self.logs_control = LogsControl()
        self.backup_control = BackupControl()
        self.app_config_control = AppConfigControl()
        self.path_config_control = PathConfigControl()
        
        # Cargar contenido inicial
        self._update_content()
    
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
    
    def _create_header(self):
        """
        Crea el header de la aplicación con botón de tema
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
            ]),
            padding=ft.padding.symmetric(horizontal=20, vertical=16),
            bgcolor=ft.Colors.SURFACE,
            border=ft.border.only(bottom=ft.BorderSide(1, ft.Colors.OUTLINE))
        )
    
    def _on_navigation_change(self, e):
        """
        Maneja el cambio de navegación en el NavigationRail
        """
        self.selected_index = e.control.selected_index
        self._update_content()
        self.page.update()
    
    def _update_content(self):
        """
        Actualiza el contenido del área principal según la selección
        """
        if self.selected_index == 0:
            self.content_area.content = self.server_control.build()
        elif self.selected_index == 1:
            self.content_area.content = self.config_control.build()
        elif self.selected_index == 2:
            self.content_area.content = self.players_control.build()
        elif self.selected_index == 3:
            self.content_area.content = self.logs_control.build()
        elif self.selected_index == 4:
            self.content_area.content = self.backup_control.build()
        elif self.selected_index == 5:
            self.content_area.content = self.path_config_control.build()
        elif self.selected_index == 6:
            self.content_area.content = self.app_config_control.build()
    
    def build(self):
        """
        Construye y retorna el layout principal
        """
        # Crear NavigationRail con configuración desde JSON
        extended = self.ui_preferences.get('navigation_rail_extended', True)
        navigation_rail = ft.Container(
            content=ft.NavigationRail(
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
                        icon=ft.Icons.SETTINGS,
                        selected_icon=ft.Icons.SETTINGS,
                        label="Configuración",
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
            ),
            bgcolor=ft.Colors.SURFACE,
            border=ft.border.only(right=ft.BorderSide(1, ft.Colors.OUTLINE))
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