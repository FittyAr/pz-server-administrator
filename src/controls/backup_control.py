import flet as ft
from datetime import datetime, timedelta


class BackupControl:
    """
    Control para la gestión de respaldos del servidor
    """
    
    def __init__(self):
        self.backup_list = ft.ListView(
            expand=True,
            spacing=5,
            padding=10
        )
        self.auto_backup_enabled = True
        self.backup_interval = "6h"
        self.current_server_id = None
        self._load_sample_backups()
        
    def set_server(self, server_id):
        """Establece el servidor actual para el control de respaldos"""
        self.current_server_id = server_id
        # TODO: Cargar respaldos del servidor específico
        self._load_sample_backups()
        
    def _create_backup(self, e):
        """
        Crea un nuevo respaldo manual
        """
        # TODO: Implementar creación real de respaldo
        current_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        e.page.show_snack_bar(
            ft.SnackBar(
                content=ft.Text("Creando respaldo..."),
                bgcolor=ft.Colors.BLUE
            )
        )
        
        # Simular creación de respaldo
        new_backup = {
            "name": f"manual_backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}",
            "date": current_time,
            "size": "245 MB",
            "type": "Manual",
            "status": "Completado"
        }
        
        self._add_backup_entry(new_backup)
        e.page.update()
        
    def _restore_backup(self, backup_name):
        """
        Restaura un respaldo específico
        """
        # TODO: Implementar restauración real
        return backup_name
        
    def _delete_backup(self, backup_name):
        """
        Elimina un respaldo específico
        """
        # TODO: Implementar eliminación real
        return backup_name
        
    def _toggle_auto_backup(self, e):
        """
        Activa/desactiva los respaldos automáticos
        """
        self.auto_backup_enabled = e.control.value
        # TODO: Implementar configuración de respaldos automáticos
        
    def _on_interval_change(self, e):
        """
        Cambia el intervalo de respaldos automáticos
        """
        self.backup_interval = e.control.value
        # TODO: Implementar cambio de intervalo
        
    def _load_sample_backups(self):
        """
        Carga respaldos de ejemplo
        """
        sample_backups = [
            {
                "name": "auto_backup_20240115_060000",
                "date": "2024-01-15 06:00:00",
                "size": "234 MB",
                "type": "Automático",
                "status": "Completado"
            },
            {
                "name": "manual_backup_20240114_180000",
                "date": "2024-01-14 18:00:00",
                "size": "228 MB",
                "type": "Manual",
                "status": "Completado"
            },
            {
                "name": "auto_backup_20240114_120000",
                "date": "2024-01-14 12:00:00",
                "size": "225 MB",
                "type": "Automático",
                "status": "Completado"
            },
            {
                "name": "auto_backup_20240114_060000",
                "date": "2024-01-14 06:00:00",
                "size": "220 MB",
                "type": "Automático",
                "status": "Completado"
            },
        ]
        
        for backup in sample_backups:
            self._add_backup_entry(backup)
            
    def _add_backup_entry(self, backup):
        """
        Añade una entrada de respaldo a la lista
        """
        type_color = ft.Colors.BLUE if backup["type"] == "Automático" else ft.Colors.GREEN
        status_color = ft.Colors.GREEN if backup["status"] == "Completado" else ft.Colors.ORANGE
        
        backup_card = ft.Container(
            content=ft.Column(
                [
                    ft.Row(
                        [
                            ft.Text(
                                backup["name"],
                                size=16,
                                weight=ft.FontWeight.BOLD,
                                expand=True
                            ),
                            ft.Container(
                                content=ft.Text(
                                    backup["type"],
                                    size=12,
                                    color=ft.Colors.WHITE
                                ),
                                bgcolor=type_color,
                                padding=ft.padding.symmetric(horizontal=8, vertical=2),
                                border_radius=4
                            ),
                        ]
                    ),
                    ft.Row(
                        [
                            ft.Text(f"Fecha: {backup['date']}", size=12, color=ft.Colors.GREY_400),
                            ft.Text(f"Tamaño: {backup['size']}", size=12, color=ft.Colors.GREY_400),
                            ft.Text(
                                backup["status"],
                                size=12,
                                color=status_color,
                                weight=ft.FontWeight.BOLD
                            ),
                        ],
                        spacing=20
                    ),
                    ft.Row(
                        [
                            ft.ElevatedButton(
                                "Restaurar",
                                icon=ft.Icons.RESTORE,
                                on_click=lambda e, name=backup["name"]: self._restore_backup(name),
                                bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE,
                                scale=0.8
                            ),
                            ft.ElevatedButton(
                                "Descargar",
                                icon=ft.Icons.DOWNLOAD,
                                bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE,
                                scale=0.8
                            ),
                            ft.ElevatedButton(
                                "Eliminar",
                                icon=ft.Icons.DELETE,
                                on_click=lambda e, name=backup["name"]: self._delete_backup(name),
                                bgcolor=ft.Colors.RED,
                            color=ft.Colors.WHITE,
                                scale=0.8
                            ),
                        ],
                        spacing=5
                    ),
                ],
                spacing=5
            ),
            bgcolor=ft.Colors.ON_SURFACE_VARIANT,
            padding=15,
            border_radius=8,
            margin=ft.margin.only(bottom=5)
        )
        
        self.backup_list.controls.insert(0, backup_card)
    
    def build(self):
        """
        Construye y retorna la interfaz del control de respaldos
        """
        return ft.Column(
            [
                ft.Text(
                    "Gestión de Respaldos",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                ft.Divider(),
                
                # Configuración de respaldos automáticos
                ft.Container(
                    content=ft.Column(
                        [
                            ft.Text(
                                "Configuración de Respaldos Automáticos",
                                size=18,
                                weight=ft.FontWeight.BOLD
                            ),
                            ft.Row(
                                [
                                    ft.Switch(
                                        label="Habilitar respaldos automáticos",
                                        value=self.auto_backup_enabled,
                                        on_change=self._toggle_auto_backup
                                    ),
                                    ft.Text("Intervalo:", size=14),
                                    ft.Dropdown(
                                        width=120,
                                        value=self.backup_interval,
                                        options=[
                                            ft.dropdown.Option("1h", "1 hora"),
                                            ft.dropdown.Option("3h", "3 horas"),
                                            ft.dropdown.Option("6h", "6 horas"),
                                            ft.dropdown.Option("12h", "12 horas"),
                                            ft.dropdown.Option("24h", "24 horas"),
                                        ],
                                        on_change=self._on_interval_change
                                    ),
                                ],
                                spacing=20
                            ),
                        ],
                        spacing=10
                    ),
                    bgcolor=ft.Colors.ON_SURFACE_VARIANT,
                    padding=15,
                    border_radius=8
                ),
                
                ft.Container(height=20),
                
                # Botones de acción
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Crear Respaldo Manual",
                            icon=ft.Icons.BACKUP,
                            on_click=self._create_backup,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Limpiar Respaldos Antiguos",
                            icon=ft.Icons.CLEANING_SERVICES,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Configurar Ubicación",
                            icon=ft.Icons.FOLDER,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
                
                ft.Container(height=10),
                
                # Lista de respaldos
                ft.Text(
                    "Respaldos Disponibles",
                    size=18,
                    weight=ft.FontWeight.BOLD
                ),
                
                ft.Container(
                    content=self.backup_list,
                    border=ft.border.all(1, ft.Colors.OUTLINE),
                    border_radius=8,
                    expand=True
                ),
            ],
            spacing=10,
            expand=True
        )