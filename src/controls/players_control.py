import flet as ft


class PlayersControl:
    """
    Control para la gestión de jugadores del servidor
    """
    
    def __init__(self):
        self.players_data = [
            {"username": "Player1", "status": "Online", "level": 15, "playtime": "2h 30m", "last_seen": "Ahora"},
            {"username": "Player2", "status": "Online", "level": 8, "playtime": "1h 15m", "last_seen": "Ahora"},
            {"username": "Player3", "status": "Offline", "level": 22, "playtime": "15h 45m", "last_seen": "Hace 2h"},
            {"username": "Player4", "status": "Banned", "level": 5, "playtime": "30m", "last_seen": "Hace 1d"},
        ]
        self.selected_player = None
        self.current_server_id = None
        
    def set_server(self, server_id):
        """Establece el servidor actual para el control de jugadores"""
        self.current_server_id = server_id
        # TODO: Cargar datos de jugadores del servidor específico
        
    def _on_player_select(self, e):
        """
        Maneja la selección de un jugador
        """
        # TODO: Implementar selección de jugador
        pass
        
    def _kick_player(self, e):
        """
        Expulsa a un jugador del servidor
        """
        if self.selected_player:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Jugador {self.selected_player} expulsado"),
                    bgcolor=ft.Colors.ORANGE
                )
            )
            
    def _ban_player(self, e):
        """
        Banea a un jugador del servidor
        """
        if self.selected_player:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Jugador {self.selected_player} baneado"),
                    bgcolor=ft.Colors.RED
                )
            )
            
    def _unban_player(self, e):
        """
        Desbanea a un jugador
        """
        if self.selected_player:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Jugador {self.selected_player} desbaneado"),
                    bgcolor=ft.Colors.GREEN
                )
            )
            
    def _send_message(self, e):
        """
        Envía un mensaje a un jugador específico
        """
        # TODO: Implementar envío de mensajes
        pass
        
    def _create_player_row(self, player):
        """
        Crea una fila para mostrar información del jugador
        """
        status_color = {
            "Online": ft.Colors.GREEN,
            "Offline": ft.Colors.GREY,
            "Banned": ft.Colors.RED
        }.get(player["status"], ft.Colors.GREY)
        
        return ft.DataRow(
            cells=[
                ft.DataCell(ft.Text(player["username"])),
                ft.DataCell(
                    ft.Row([
                        ft.Icon(ft.Icons.CIRCLE, color=status_color, size=12),
                        ft.Text(player["status"])
                    ])
                ),
                ft.DataCell(ft.Text(str(player["level"]))),
                ft.DataCell(ft.Text(player["playtime"])),
                ft.DataCell(ft.Text(player["last_seen"])),
            ],
            on_select_changed=self._on_player_select
        )
    
    def build(self):
        """
        Construye y retorna la interfaz del control de jugadores
        """
        return ft.Column(
            [
                ft.Text(
                    "Gestión de Jugadores",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                ft.Divider(),
                
                # Estadísticas rápidas
                ft.Row(
                    [
                        ft.Container(
                            content=ft.Column([
                                ft.Text("2", size=24, weight=ft.FontWeight.BOLD, color=ft.Colors.GREEN),
                                ft.Text("En línea", size=12)
                            ], horizontal_alignment=ft.CrossAxisAlignment.CENTER),
                            bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                            padding=15,
                            border_radius=8,
                            width=100
                        ),
                        ft.Container(
                            content=ft.Column([
                                ft.Text("4", size=24, weight=ft.FontWeight.BOLD, color=ft.Colors.BLUE),
                                ft.Text("Total", size=12)
                            ], horizontal_alignment=ft.CrossAxisAlignment.CENTER),
                            bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                            padding=15,
                            border_radius=8,
                            width=100
                        ),
                        ft.Container(
                            content=ft.Column([
                                ft.Text("1", size=24, weight=ft.FontWeight.BOLD, color=ft.Colors.RED),
                                ft.Text("Baneados", size=12)
                            ], horizontal_alignment=ft.CrossAxisAlignment.CENTER),
                            bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                            padding=15,
                            border_radius=8,
                            width=100
                        ),
                    ],
                    spacing=10
                ),
                
                ft.Container(height=20),
                
                # Tabla de jugadores
                ft.Container(
                    content=ft.DataTable(
                        columns=[
                            ft.DataColumn(ft.Text("Usuario")),
                            ft.DataColumn(ft.Text("Estado")),
                            ft.DataColumn(ft.Text("Nivel")),
                            ft.DataColumn(ft.Text("Tiempo de Juego")),
                            ft.DataColumn(ft.Text("Última Conexión")),
                        ],
                        rows=[self._create_player_row(player) for player in self.players_data],
                    ),
                    border=ft.border.all(1, ft.Colors.OUTLINE),
                    border_radius=8,
                    padding=10
                ),
                
                ft.Container(height=10),
                
                # Botones de acción
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Expulsar",
                            icon=ft.Icons.LOGOUT,
                            on_click=self._kick_player,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Banear",
                            icon=ft.Icons.BLOCK,
                            on_click=self._ban_player,
                            bgcolor=ft.Colors.RED,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Desbanear",
                            icon=ft.Icons.CHECK_CIRCLE,
                            on_click=self._unban_player,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Enviar Mensaje",
                            icon=ft.Icons.MESSAGE,
                            on_click=self._send_message,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
                
                # Campo para mensajes globales
                ft.Container(height=20),
                ft.Text("Mensaje Global", size=16, weight=ft.FontWeight.BOLD),
                ft.Row(
                    [
                        ft.TextField(
                            hint_text="Escribe un mensaje para todos los jugadores...",
                            expand=True
                        ),
                        ft.ElevatedButton(
                            "Enviar",
                            icon=ft.Icons.SEND,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
            ],
            spacing=10,
            scroll=ft.ScrollMode.AUTO
        )