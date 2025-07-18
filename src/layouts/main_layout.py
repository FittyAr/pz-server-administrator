import flet as ft
from controls.server_control import ServerControl
from controls.config_control import ConfigControl
from controls.players_control import PlayersControl
from controls.logs_control import LogsControl
from controls.backup_control import BackupControl


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
            padding=20
        )
        
        # Inicializar controles
        self.server_control = ServerControl()
        self.config_control = ConfigControl()
        self.players_control = PlayersControl()
        self.logs_control = LogsControl()
        self.backup_control = BackupControl()
        
        # Cargar contenido inicial
        self._update_content()
    
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
    
    def build(self):
        """
        Construye y retorna el layout principal
        """
        navigation_rail = ft.NavigationRail(
            selected_index=self.selected_index,
            label_type=ft.NavigationRailLabelType.ALL,
            min_width=100,
            min_extended_width=200,
            group_alignment=-0.9,
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
                    icon=ft.Icons.DESCRIPTION,
                    selected_icon=ft.Icons.DESCRIPTION,
                    label="Logs",
                ),
                ft.NavigationRailDestination(
                    icon=ft.Icons.BACKUP,
                    selected_icon=ft.Icons.BACKUP,
                    label="Respaldos",
                ),
            ],
            on_change=self._on_navigation_change,
        )
        
        return ft.Row(
            [
                navigation_rail,
                ft.VerticalDivider(width=1),
                self.content_area,
            ],
            expand=True,
        )