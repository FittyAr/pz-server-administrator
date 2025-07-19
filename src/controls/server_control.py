import flet as ft
from utils.config_loader import config_loader
from utils.server_manager import ServerManager
from controls.server_selector_control import ServerSelectorControl


class ServerControl:
    """
    Control para la gestión del servidor Project Zomboid
    """
    
    def __init__(self):
        self.server_status = "Detenido"
        self.status_color = ft.Colors.RED
        self.current_server_id = None
        self.server_manager = None
        self.server_selector = ServerSelectorControl(on_server_change=self._on_server_change)
        
        # Elementos de UI que necesitan actualizarse
        self.status_text = ft.Text(f"Estado: {self.server_status}", size=18, weight=ft.FontWeight.W_500)
        self.status_icon = ft.Icon(ft.Icons.CIRCLE, color=self.status_color, size=20)
        self.server_info_container = ft.Container()
        
        # Inicializar con el servidor favorito o por defecto si existe
        selected_server = config_loader.initialize_selected_server()
        if selected_server:
            self._on_server_change(selected_server)
        
    def _on_server_change(self, server_id: str):
        """
        Maneja el cambio de servidor seleccionado
        """
        self.current_server_id = server_id
        
        if server_id:
            # Obtener configuración del servidor
            all_servers = config_loader.get_all_servers()
            server_config = all_servers.get(server_id)
            
            if server_config:
                # Crear nuevo ServerManager para este servidor
                self.server_manager = ServerManager(
                    server_path=server_config.get('server_path', ''),
                    java_path="java"  # TODO: Hacer configurable
                )
                
                # Actualizar estado del servidor
                self._update_server_status()
                self._update_server_info(server_config)
        else:
            self.server_manager = None
            self.server_status = "Sin servidor seleccionado"
            self.status_color = ft.Colors.GREY
            self._update_ui()
    
    def _update_server_status(self):
        """
        Actualiza el estado del servidor
        """
        if self.server_manager and self.server_manager.is_server_running():
            self.server_status = "Ejecutándose"
            self.status_color = ft.Colors.GREEN
        else:
            self.server_status = "Detenido"
            self.status_color = ft.Colors.RED
        
        self._update_ui()
    
    def _update_server_info(self, server_config):
        """
        Actualiza la información mostrada del servidor
        """
        server_name = server_config.get('name', 'Servidor sin nombre')
        server_path = server_config.get('server_path', 'No especificada')
        
        # Actualizar el contenedor de información
        self.server_info_container.content = ft.Column([
            ft.Text(f"Servidor: {server_name}", size=16, weight=ft.FontWeight.BOLD),
            ft.Text(f"Ruta: {server_path}", size=12, color=ft.Colors.ON_SURFACE_VARIANT),
            ft.Divider(),
            ft.Text("Estadísticas del Servidor", size=14, weight=ft.FontWeight.BOLD),
            ft.Text("CPU: 0%"),
            ft.Text("RAM: 0 MB"),
            ft.Text("Jugadores conectados: 0/32"),
            ft.Text("Tiempo de actividad: 00:00:00"),
        ], spacing=5)
    
    def _update_ui(self):
        """
        Actualiza los elementos de la UI
        """
        if hasattr(self, 'status_text'):
            self.status_text.value = f"Estado: {self.server_status}"
        if hasattr(self, 'status_icon'):
            self.status_icon.color = self.status_color
    
    def _start_server(self, e):
        """
        Inicia el servidor Project Zomboid
        """
        if not self.current_server_id:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("Selecciona un servidor primero"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            e.page.update()
            return
        
        if self.server_manager:
            try:
                success = self.server_manager.start_server()
                if success:
                    self.server_status = "Ejecutándose"
                    self.status_color = ft.Colors.GREEN
                    message = "Servidor iniciado exitosamente"
                    color = ft.Colors.GREEN
                else:
                    message = "Error al iniciar el servidor"
                    color = ft.Colors.RED
            except Exception as ex:
                message = f"Error: {str(ex)}"
                color = ft.Colors.RED
        else:
            message = "No hay servidor configurado"
            color = ft.Colors.RED
        
        self._update_ui()
        e.page.snack_bar = ft.SnackBar(content=ft.Text(message), bgcolor=color)
        e.page.snack_bar.open = True
        e.page.update()
        
    def _stop_server(self, e):
        """
        Detiene el servidor Project Zomboid
        """
        if not self.current_server_id:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("No hay servidor seleccionado"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            e.page.update()
            return
        
        if self.server_manager:
            try:
                success = self.server_manager.stop_server()
                if success:
                    self.server_status = "Detenido"
                    self.status_color = ft.Colors.RED
                    message = "Servidor detenido exitosamente"
                    color = ft.Colors.GREEN
                else:
                    message = "Error al detener el servidor"
                    color = ft.Colors.RED
            except Exception as ex:
                message = f"Error: {str(ex)}"
                color = ft.Colors.RED
        else:
            message = "No hay servidor configurado"
            color = ft.Colors.RED
        
        self._update_ui()
        e.page.snack_bar = ft.SnackBar(content=ft.Text(message), bgcolor=color)
        e.page.snack_bar.open = True
        e.page.update()
        
    def _restart_server(self, e):
        """
        Reinicia el servidor Project Zomboid
        """
        if not self.current_server_id:
            e.page.snack_bar = ft.SnackBar(
                content=ft.Text("No hay servidor seleccionado"),
                bgcolor=ft.Colors.RED
            )
            e.page.snack_bar.open = True
            e.page.update()
            return
        
        if self.server_manager:
            try:
                self.server_status = "Reiniciando..."
                self.status_color = ft.Colors.ORANGE
                self._update_ui()
                e.page.update()
                
                success = self.server_manager.restart_server()
                if success:
                    self.server_status = "Ejecutándose"
                    self.status_color = ft.Colors.GREEN
                    message = "Servidor reiniciado exitosamente"
                    color = ft.Colors.GREEN
                else:
                    self.server_status = "Detenido"
                    self.status_color = ft.Colors.RED
                    message = "Error al reiniciar el servidor"
                    color = ft.Colors.RED
            except Exception as ex:
                self.server_status = "Detenido"
                self.status_color = ft.Colors.RED
                message = f"Error: {str(ex)}"
                color = ft.Colors.RED
        else:
            message = "No hay servidor configurado"
            color = ft.Colors.RED
        
        self._update_ui()
        e.page.snack_bar = ft.SnackBar(content=ft.Text(message), bgcolor=color)
        e.page.snack_bar.open = True
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
                
                # Selector de servidores
                ft.Container(
                    content=self.server_selector.build(),
                    margin=ft.margin.only(bottom=20)
                ),
                
                ft.Divider(),
                
                # Estado del servidor
                ft.Container(
                    content=ft.Row(
                        [
                            self.status_icon,
                            self.status_text,
                        ],
                        alignment=ft.MainAxisAlignment.START
                    ),
                    padding=10,
                    bgcolor=ft.Colors.ON_SURFACE_VARIANT,
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
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE,
                            disabled=not bool(self.current_server_id)
                        ),
                        ft.ElevatedButton(
                            "Detener Servidor",
                            icon=ft.Icons.STOP,
                            on_click=self._stop_server,
                            bgcolor=ft.Colors.RED,
                            color=ft.Colors.WHITE,
                            disabled=not bool(self.current_server_id)
                        ),
                        ft.ElevatedButton(
                            "Reiniciar Servidor",
                            icon=ft.Icons.REFRESH,
                            on_click=self._restart_server,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE,
                            disabled=not bool(self.current_server_id)
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
                
                # Contenedor dinámico de información del servidor
                self.server_info_container if self.server_info_container.content else ft.Container(
                    content=ft.Column(
                        [
                            ft.Icon(ft.Icons.INFO_OUTLINE, size=48, color=ft.Colors.GREY),
                            ft.Text(
                                "Selecciona un servidor para ver su información",
                                size=16,
                                color=ft.Colors.GREY,
                                text_align=ft.TextAlign.CENTER
                            )
                        ],
                        horizontal_alignment=ft.CrossAxisAlignment.CENTER
                    ),
                    padding=20,
                    bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                    border_radius=8,
                    alignment=ft.alignment.center,
                    height=200
                ),
            ],
            spacing=10,
            scroll=ft.ScrollMode.AUTO
        )