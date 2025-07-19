import flet as ft
from datetime import datetime


class LogsControl:
    """
    Control para la visualización y gestión de logs del servidor
    """
    
    def __init__(self):
        self.log_type = "server"
        self.auto_refresh = False
        self.current_server_id = None
        self.log_content = ft.ListView(
            expand=True,
            spacing=2,
            padding=10
        )
        self._load_sample_logs()
        
    def set_server(self, server_id):
        """Establece el servidor actual para el control de logs"""
        self.current_server_id = server_id
        # TODO: Cargar logs del servidor específico
        self._load_logs()
        
    def _on_log_type_change(self, e):
        """
        Maneja el cambio de tipo de log
        """
        self.log_type = e.control.value
        self._load_logs()
        e.page.update()
        
    def _toggle_auto_refresh(self, e):
        """
        Activa/desactiva la actualización automática de logs
        """
        self.auto_refresh = e.control.value
        # TODO: Implementar timer para auto-refresh
        
    def _clear_logs(self, e):
        """
        Limpia la visualización de logs
        """
        self.log_content.controls.clear()
        e.page.update()
        
    def _export_logs(self, e):
        """
        Exporta los logs a un archivo
        """
        # TODO: Implementar exportación de logs
        e.page.show_snack_bar(
            ft.SnackBar(
                content=ft.Text("Logs exportados exitosamente"),
                bgcolor=ft.Colors.GREEN
            )
        )
        
    def _load_logs(self):
        """
        Carga los logs según el tipo seleccionado
        """
        self.log_content.controls.clear()
        
        if self.log_type == "server":
            self._load_sample_logs()
        elif self.log_type == "chat":
            self._load_chat_logs()
        elif self.log_type == "admin":
            self._load_admin_logs()
        elif self.log_type == "error":
            self._load_error_logs()
            
    def _load_sample_logs(self):
        """
        Carga logs de ejemplo del servidor
        """
        sample_logs = [
            {"time": "2024-01-15 10:30:15", "level": "INFO", "message": "Server started successfully", "color": ft.Colors.GREEN},
            {"time": "2024-01-15 10:30:20", "level": "INFO", "message": "Player 'Player1' connected from 192.168.1.100", "color": ft.Colors.BLUE},
            {"time": "2024-01-15 10:35:42", "level": "INFO", "message": "Player 'Player2' connected from 192.168.1.101", "color": ft.Colors.BLUE},
            {"time": "2024-01-15 10:45:12", "level": "WARN", "message": "High memory usage detected: 85%", "color": ft.Colors.ORANGE},
            {"time": "2024-01-15 11:00:33", "level": "INFO", "message": "Autosave completed", "color": ft.Colors.GREEN},
        ]
        
        for log in sample_logs:
            self._add_log_entry(log)
            
    def _load_chat_logs(self):
        """
        Carga logs de chat
        """
        chat_logs = [
            {"time": "2024-01-15 10:32:15", "level": "CHAT", "message": "[Player1]: Hello everyone!", "color": ft.Colors.CYAN},
            {"time": "2024-01-15 10:32:45", "level": "CHAT", "message": "[Player2]: Hi there!", "color": ft.Colors.CYAN},
            {"time": "2024-01-15 10:35:12", "level": "CHAT", "message": "[Player1]: Anyone want to team up?", "color": ft.Colors.CYAN},
        ]
        
        for log in chat_logs:
            self._add_log_entry(log)
            
    def _load_admin_logs(self):
        """
        Carga logs de administración
        """
        admin_logs = [
            {"time": "2024-01-15 09:15:30", "level": "ADMIN", "message": "Admin 'ServerAdmin' logged in", "color": ft.Colors.PURPLE},
            {"time": "2024-01-15 09:20:15", "level": "ADMIN", "message": "Server configuration updated", "color": ft.Colors.PURPLE},
            {"time": "2024-01-15 10:45:22", "level": "ADMIN", "message": "Player 'BadPlayer' banned by ServerAdmin", "color": ft.Colors.RED},
        ]
        
        for log in admin_logs:
            self._add_log_entry(log)
            
    def _load_error_logs(self):
        """
        Carga logs de errores
        """
        error_logs = [
            {"time": "2024-01-15 08:45:12", "level": "ERROR", "message": "Failed to load mod: ModName.lua", "color": ft.Colors.RED},
            {"time": "2024-01-15 09:12:33", "level": "ERROR", "message": "Database connection timeout", "color": ft.Colors.RED},
            {"time": "2024-01-15 10:22:45", "level": "WARN", "message": "Deprecated function used in script", "color": ft.Colors.ORANGE},
        ]
        
        for log in error_logs:
            self._add_log_entry(log)
            
    def _add_log_entry(self, log):
        """
        Añade una entrada de log a la visualización
        """
        log_row = ft.Container(
            content=ft.Row(
                [
                    ft.Text(
                        log["time"],
                        size=12,
                        color=ft.Colors.GREY_400,
                        width=150
                    ),
                    ft.Container(
                        content=ft.Text(
                            log["level"],
                            size=12,
                            weight=ft.FontWeight.BOLD,
                            color=ft.Colors.WHITE
                        ),
                        bgcolor=log["color"],
                        padding=ft.padding.symmetric(horizontal=8, vertical=2),
                        border_radius=4,
                        width=60
                    ),
                    ft.Text(
                        log["message"],
                        size=12,
                        expand=True
                    ),
                ],
                spacing=10
            ),
            padding=ft.padding.symmetric(vertical=2, horizontal=5),
            border_radius=4
        )
        
        self.log_content.controls.append(log_row)
    
    def build(self):
        """
        Construye y retorna la interfaz del control de logs
        """
        return ft.Column(
            [
                ft.Text(
                    "Visualización de Logs",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                ft.Divider(),
                
                # Controles superiores
                ft.Row(
                    [
                        ft.Text("Tipo de log:", size=16),
                        ft.Dropdown(
                            width=200,
                            value=self.log_type,
                            options=[
                                ft.dropdown.Option("server", "Servidor"),
                                ft.dropdown.Option("chat", "Chat"),
                                ft.dropdown.Option("admin", "Administración"),
                                ft.dropdown.Option("error", "Errores"),
                            ],
                            on_change=self._on_log_type_change
                        ),
                        ft.Switch(
                            label="Auto-actualizar",
                            value=self.auto_refresh,
                            on_change=self._toggle_auto_refresh
                        ),
                    ],
                    alignment=ft.MainAxisAlignment.START,
                    spacing=20
                ),
                
                ft.Container(height=10),
                
                # Botones de acción
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Actualizar",
                            icon=ft.Icons.REFRESH,
                            on_click=lambda e: self._load_logs(),
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Limpiar",
                            icon=ft.Icons.CLEAR,
                            on_click=self._clear_logs,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Exportar",
                            icon=ft.Icons.DOWNLOAD,
                            on_click=self._export_logs,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
                
                ft.Container(height=10),
                
                # Área de logs
                ft.Container(
                    content=self.log_content,
                    border=ft.border.all(1, ft.Colors.OUTLINE),
                    border_radius=8,
                    expand=True,
                    bgcolor=ft.Colors.ON_SURFACE_VARIANT
                ),
            ],
            spacing=10,
            expand=True
        )