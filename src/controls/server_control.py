import flet as ft


class ServerControl:
    """
    Control para la gestión del servidor Project Zomboid
    """
    
    def __init__(self):
        self.server_status = "Detenido"
        self.status_color = ft.colors.RED
        
    def _start_server(self, e):
        """
        Inicia el servidor Project Zomboid
        """
        # TODO: Implementar lógica de inicio del servidor
        self.server_status = "Ejecutándose"
        self.status_color = ft.colors.GREEN
        e.page.update()
        
    def _stop_server(self, e):
        """
        Detiene el servidor Project Zomboid
        """
        # TODO: Implementar lógica de parada del servidor
        self.server_status = "Detenido"
        self.status_color = ft.colors.RED
        e.page.update()
        
    def _restart_server(self, e):
        """
        Reinicia el servidor Project Zomboid
        """
        # TODO: Implementar lógica de reinicio del servidor
        self.server_status = "Reiniciando..."
        self.status_color = ft.colors.ORANGE
        e.page.update()
    
    def build(self):
        """
        Construye y retorna la interfaz del control del servidor
        """
        return ft.Column(
            [
                ft.Text(
                    "Control del Servidor Project Zomboid",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                ft.Divider(),
                
                # Estado del servidor
                ft.Container(
                    content=ft.Row(
                        [
                            ft.Icon(
                                ft.Icons.CIRCLE,
                                color=self.status_color,
                                size=20
                            ),
                            ft.Text(
                                f"Estado: {self.server_status}",
                                size=18,
                                weight=ft.FontWeight.W_500
                            ),
                        ],
                        alignment=ft.MainAxisAlignment.START
                    ),
                    padding=10,
                    bgcolor=ft.colors.SURFACE_VARIANT,
                    border_radius=8,
                    margin=ft.margin.only(bottom=20)
                ),
                
                # Botones de control
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Iniciar Servidor",
                            icon=ft.Icons.PLAY_ARROW,
                            on_click=self._start_server,
                            bgcolor=ft.colors.GREEN,
                            color=ft.colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Detener Servidor",
                            icon=ft.Icons.STOP,
                            on_click=self._stop_server,
                            bgcolor=ft.colors.RED,
                            color=ft.colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Reiniciar Servidor",
                            icon=ft.Icons.REFRESH,
                            on_click=self._restart_server,
                            bgcolor=ft.colors.ORANGE,
                            color=ft.colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
                
                ft.Divider(),
                
                # Información del servidor
                ft.Text(
                    "Información del Servidor",
                    size=20,
                    weight=ft.FontWeight.BOLD
                ),
                
                ft.Container(
                    content=ft.Column(
                        [
                            ft.Text("CPU: 0%"),
                            ft.Text("RAM: 0 MB"),
                            ft.Text("Jugadores conectados: 0/32"),
                            ft.Text("Tiempo de actividad: 00:00:00"),
                        ],
                        spacing=5
                    ),
                    padding=15,
                    bgcolor=ft.colors.SURFACE_VARIANT,
                    border_radius=8,
                    width=300
                ),
            ],
            spacing=10,
            scroll=ft.ScrollMode.AUTO
        )