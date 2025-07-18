import flet as ft
import os
import configparser
from utils.config_loader import config_loader
from utils.server_manager import ServerManager
from controls.ini_simple_editor_control import IniSimpleEditorControl


class ConfigControl:
    """
    Control para la gestión de archivos de configuración del servidor
    """
    
    def __init__(self):
        self.selected_config_type = "server_settings"
        self.current_server_id = None
        self.edit_mode = "advanced"  # "advanced" o "simple"
        
        # Configuración de archivos disponibles
        self.config_files = {
            "server_settings": {"name": "Configuración del Servidor", "extension": ".ini", "enabled": False},
            "sandbox_vars": {"name": "Variables Sandbox", "extension": ".lua", "enabled": False},
            "spawn_regions": {"name": "Regiones de Spawn", "extension": ".lua", "enabled": False},
            "server_rules": {"name": "Reglas del Servidor", "extension": ".json", "enabled": False}
        }
        
        # Editor avanzado
        self.config_content = ft.TextField(
            multiline=True,
            min_lines=20,
            max_lines=30,
            expand=True,
            border_color=ft.Colors.OUTLINE,
            focused_border_color=ft.Colors.PRIMARY,
            text_style=ft.TextStyle(font_family="Consolas"),
            value="# Selecciona un archivo de configuración para editarlo"
        )
        
        # Editor simple para archivos INI
        self.ini_simple_editor = IniSimpleEditorControl()
        
        # Editor simple (se creará dinámicamente)
        self.simple_editor_container = ft.Container()
        
        self.server_info_text = ft.Text(
            "Servidor: No seleccionado",
            size=14,
            color=ft.Colors.GREY
        )
        
        # Cargar contenido inicial
        self._load_config_content()
        self._update_file_buttons_status()
        
    def _on_config_type_change(self, e):
        """
        Maneja el cambio de tipo de configuración
        """
        self.selected_config_type = e.control.value
        self._load_config_content()
        e.page.update()
        
    def _update_server_info(self):
        """
        Actualiza la información del servidor seleccionado
        """
        self.current_server_id = config_loader.get_selected_server()
        if self.current_server_id:
            servers = config_loader.get_all_servers()
            server_data = servers.get(self.current_server_id, {})
            server_name = server_data.get('name', self.current_server_id)
            self.server_info_text.value = f"Servidor: {server_name}"
        else:
            self.server_info_text.value = "Servidor: No seleccionado"
    
    def _get_config_file_path(self, config_type: str) -> str:
        """
        Obtiene la ruta del archivo de configuración según el tipo
        """
        if not self.current_server_id:
            return None
            
        servers = config_loader.get_all_servers()
        server_data = servers.get(self.current_server_id, {})
        server_path = server_data.get('server_path')
        server_name = server_data.get('name')  # Nombre real del servidor
        
        if not server_path or not server_name:
            return None
        
        # Usar el nombre real del servidor, no el ID normalizado
        file_mappings = {
            "server_settings": f"{server_name}.ini",
            "sandbox_vars": f"{server_name}_SandBoxVars.lua",
            "spawn_regions": f"{server_name}_spawnregions.lua",
            "server_rules": f"{server_name}_rules.json"
        }
        
        filename = file_mappings.get(config_type)
        if filename:
            return os.path.join(server_path, filename)
        return None
    
    def _update_file_buttons_status(self):
        """Actualiza el estado de los botones de archivos según su existencia"""
        if not self.current_server_id:
            for file_type in self.config_files:
                self.config_files[file_type]["enabled"] = False
            return
            
        servers = config_loader.get_all_servers()
        server_data = servers.get(self.current_server_id, {})
        if not server_data:
            return
            
        # Verificar existencia de archivos
        server_path = server_data.get('server_path', '')
        server_name = server_data.get('name', '')
        
        for file_type, file_info in self.config_files.items():
            file_path = self._get_config_file_path_for_type(file_type, server_path, server_name)
            file_info["enabled"] = os.path.exists(file_path) if file_path else False
    
    def _get_config_file_path_for_type(self, file_type, server_path, server_name):
        """Obtiene la ruta del archivo para un tipo específico"""
        file_mapping = {
            "server_settings": f"{server_name}.ini",
            "sandbox_vars": f"{server_name}_SandBoxVars.lua",
            "spawn_regions": f"{server_name}_spawnregions.lua", 
            "server_rules": f"{server_name}_rules.json"
        }
        
        if file_type in file_mapping:
            return os.path.join(server_path, file_mapping[file_type])
        return None
    
    def _update_simple_editor(self, content=None):
        """Actualiza el editor simple para archivos .ini"""
        if self.selected_config_type != "server_settings" or not content:
            self.simple_editor_container.content = ft.Container(
                content=ft.Text(
                    "Editor simple disponible solo para archivos .ini con contenido válido",
                    text_align=ft.TextAlign.CENTER
                ),
                alignment=ft.alignment.center,
                height=200
            )
            return
             
        # Parsear contenido .ini
        fields = []
        lines = content.split('\n')
        current_section = None
        
        for line in lines:
            line = line.strip()
            
            # Detectar secciones [Section]
            if line.startswith('[') and line.endswith(']'):
                current_section = line[1:-1]
                fields.append(
                    ft.Container(
                        content=ft.Text(
                            f"[{current_section}]",
                            size=16,
                            weight=ft.FontWeight.BOLD,
                            color=ft.Colors.PRIMARY
                        ),
                        margin=ft.margin.only(top=20, bottom=10)
                    )
                )
                continue
            
            # Procesar líneas con configuración
            if line and not line.startswith('#') and '=' in line:
                key, value = line.split('=', 1)
                key = key.strip()
                value = value.strip()
                
                # Crear control apropiado según el tipo de valor
                control = self._create_config_control(key, value)
                
                fields.append(
                    ft.Container(
                        content=ft.Row([
                            ft.Container(
                                content=ft.Text(
                                    key,
                                    size=14,
                                    weight=ft.FontWeight.W_500
                                ),
                                width=200
                            ),
                            ft.Container(
                                content=control,
                                width=300
                            )
                        ]),
                        margin=ft.margin.only(bottom=10)
                    )
                )
        
        if not fields:
            self.simple_editor_container.content = ft.Container(
                content=ft.Text(
                    "No se encontraron configuraciones válidas en el archivo",
                    text_align=ft.TextAlign.CENTER
                ),
                alignment=ft.alignment.center,
                height=200
            )
        else:
            self.simple_editor_container.content = ft.Container(
                content=ft.Column(fields, scroll=ft.ScrollMode.AUTO),
                padding=10
            )
    
    def _create_config_control(self, key, value):
        """Crea el control apropiado según el tipo de configuración"""
        key_lower = key.lower()
        
        # Valores booleanos
        if value.lower() in ['true', 'false']:
            return ft.Switch(
                value=value.lower() == 'true',
                data=key
            )
        
        # Valores numéricos con rangos conocidos
        if key_lower in ['maxplayers', 'port', 'pinglimit']:
            try:
                return ft.TextField(
                    value=value,
                    data=key,
                    input_filter=ft.NumbersOnlyInputFilter(),
                    width=150
                )
            except:
                pass
        
        # Valores de texto con opciones conocidas
        if key_lower in ['publicname', 'publicdescription']:
            return ft.TextField(
                value=value,
                data=key,
                multiline=key_lower == 'publicdescription',
                max_lines=3 if key_lower == 'publicdescription' else 1
            )
        
        # Por defecto, campo de texto
        return ft.TextField(
            value=value,
            data=key
        )
    
    def _load_config_content(self):
        """
        Carga el contenido del archivo de configuración seleccionado
        """
        self._update_server_info()
        
        if not self.current_server_id:
            self.config_content.value = "# No hay servidor seleccionado\n# Selecciona un servidor en la sección 'Selector de Servidores'"
            self._update_simple_editor()
            return
            
        config_file_path = self._get_config_file_path(self.selected_config_type)
        
        if config_file_path and os.path.exists(config_file_path):
            try:
                # Intentar diferentes codificaciones
                encodings = ['utf-8', 'latin-1', 'cp1252', 'iso-8859-1']
                content = None
                
                for encoding in encodings:
                    try:
                        with open(config_file_path, 'r', encoding=encoding) as file:
                            content = file.read()
                            break
                    except UnicodeDecodeError:
                        continue
                
                if content is not None:
                    self.config_content.value = content
                    self._update_simple_editor(content)
                    # Cargar contenido en el editor simple de INI si es server_settings
                    if self.selected_config_type == "server_settings":
                        self.ini_simple_editor.load_ini_content(content)
                else:
                    self.config_content.value = f"# Error: No se pudo decodificar el archivo con ninguna codificación\n# Ruta: {config_file_path}"
                    self._update_simple_editor()
                    if self.selected_config_type == "server_settings":
                        self.ini_simple_editor.clear()
                    
            except Exception as e:
                self.config_content.value = f"# Error al cargar el archivo: {str(e)}\n# Ruta: {config_file_path}"
                self._update_simple_editor()
                if self.selected_config_type == "server_settings":
                    self.ini_simple_editor.clear()
        else:
            # Archivo no existe, mostrar plantilla
            config_templates = {
                "server_settings": f"# Archivo no encontrado: {config_file_path}\n# Creando plantilla para {self.current_server_id}\n\nPublicName=My PZ Server\nPublicDescription=A Project Zomboid Server\nMaxPlayers=32\nPVP=true\nPauseEmpty=true\nPingLimit=400\n",
                "sandbox_vars": f"-- Archivo no encontrado: {config_file_path}\n-- Creando plantilla para {self.current_server_id}\n\nSandBoxVars = {{\n    Version = 5,\n    Zombies = 4,\n    Distribution = 1,\n    DayLength = 3,\n    StartMonth = 7,\n    StartTime = 2,\n    WaterShut = 6,\n    ElecShut = 6,\n}}",
                "spawn_regions": f"-- Archivo no encontrado: {config_file_path}\n-- Creando plantilla para {self.current_server_id}\n\nfunction SpawnRegions()\n    return {{\n        {{ name = \"Muldraugh, KY\", file = \"media/maps/Muldraugh, KY/spawnpoints.lua\" }},\n        {{ name = \"West Point, KY\", file = \"media/maps/West Point, KY/spawnpoints.lua\" }},\n        {{ name = \"Riverside, KY\", file = \"media/maps/Riverside, KY/spawnpoints.lua\" }},\n    }}\nend",
                "server_rules": f"{{\n  \"_comment\": \"Archivo no encontrado: {config_file_path} - Creando plantilla para {self.current_server_id}\",\n  \"rules\": [\n    \"No griefing\",\n    \"No cheating\",\n    \"Respect other players\",\n    \"No offensive language\"\n  ],\n  \"punishments\": {{\n    \"warning\": 1,\n    \"kick\": 2,\n    \"ban\": 3\n  }}\n}}"
            }
            
            template = config_templates.get(self.selected_config_type, f"# Archivo de configuración no encontrado: {config_file_path}")
            self.config_content.value = template
            self._update_simple_editor(template)
            # Cargar plantilla en el editor simple de INI si es server_settings
            if self.selected_config_type == "server_settings":
                self.ini_simple_editor.load_ini_content(template)
        
    def _save_config(self, e):
        """
        Guarda los cambios en el archivo de configuración
        """
        if not self.current_server_id:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text("No hay servidor seleccionado"),
                    bgcolor=ft.Colors.RED
                )
            )
            return
            
        config_file_path = self._get_config_file_path(self.selected_config_type)
        
        if not config_file_path:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text("No se pudo determinar la ruta del archivo"),
                    bgcolor=ft.Colors.RED
                )
            )
            return
            
        try:
            # Crear directorio si no existe
            os.makedirs(os.path.dirname(config_file_path), exist_ok=True)
            
            # Obtener contenido según el modo de edición
            if self.edit_mode == "advanced":
                content = self.config_content.value
            else:
                # Modo simple: obtener contenido del editor específico para INI si es server_settings
                if self.selected_config_type == "server_settings":
                    content = self.ini_simple_editor.get_ini_content()
                else:
                    content = self._get_simple_editor_content()
            
            # Guardar archivo
            with open(config_file_path, 'w', encoding='utf-8') as file:
                file.write(content)
                
            # Actualizar estado de archivos y recargar
            self._update_file_buttons_status()
            if self.edit_mode == "advanced":
                self._load_config_content()
                
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Configuración guardada: {os.path.basename(config_file_path)}"),
                    bgcolor=ft.Colors.GREEN
                )
            )
        except Exception as ex:
            e.page.show_snack_bar(
                ft.SnackBar(
                    content=ft.Text(f"Error al guardar: {str(ex)}"),
                    bgcolor=ft.Colors.RED
                )
            )
    
    def _get_simple_editor_content(self):
        """Obtiene el contenido del editor simple y lo convierte a formato .ini"""
        if not hasattr(self.simple_editor_container, 'content') or not self.simple_editor_container.content:
            return self.config_content.value
        
        lines = []
        current_section = None
        
        # Recorrer los controles del editor simple
        container = self.simple_editor_container.content
        if hasattr(container, 'content') and hasattr(container.content, 'controls'):
            for control in container.content.controls:
                if hasattr(control, 'content'):
                    # Verificar si es una sección
                    if hasattr(control.content, 'value') and control.content.value.startswith('['):
                        lines.append(control.content.value)
                        continue
                    
                    # Verificar si es una fila de configuración
                    if hasattr(control.content, 'controls') and len(control.content.controls) >= 2:
                        row = control.content
                        if len(row.controls) >= 2:
                            key_container = row.controls[0]
                            value_container = row.controls[1]
                            
                            if hasattr(key_container, 'content') and hasattr(value_container, 'content'):
                                key = key_container.content.value
                                value_control = value_container.content
                                
                                # Obtener valor según el tipo de control
                                if hasattr(value_control, 'value'):
                                    if isinstance(value_control, ft.Switch):
                                        value = 'true' if value_control.value else 'false'
                                    else:
                                        value = str(value_control.value)
                                    
                                    lines.append(f"{key}={value}")
        
        return '\n'.join(lines) if lines else self.config_content.value
        
    def _reset_config(self, e):
        """
        Restaura la configuración a los valores por defecto
        """
        self._load_config_content()
        e.page.show_snack_bar(
            ft.SnackBar(
                content=ft.Text("Configuración restaurada"),
                bgcolor=ft.Colors.ORANGE
            )
        )
        e.page.update()
    
    def refresh_for_selected_server(self):
        """
        Actualiza el contenido para el servidor seleccionado
        Método llamado cuando cambia la selección de servidor
        """
        self._load_config_content()
    
    def update_selected_server(self, server_id):
        """Actualiza el servidor seleccionado y recarga la configuración"""
        self.current_server_id = server_id
        
        if server_id:
            servers = config_loader.get_all_servers()
            server_data = servers.get(server_id, {})
            server_name = server_data.get('name', 'Desconocido')
            self.server_info_text.value = f"Servidor: {server_name}"
        else:
            self.server_info_text.value = "Servidor: No seleccionado"
        
        self._update_file_buttons_status()
        self._load_config_content()
    
    def _on_file_type_change(self, e, file_type):
        """Maneja el cambio de tipo de archivo"""
        self.selected_config_type = file_type
        self._load_config_content()
        # Actualizar la interfaz completa para reflejar el cambio
        if hasattr(e, 'page') and e.page:
            e.page.update()
     
    def _on_mode_change(self, e):
        """Maneja el cambio de modo de edición"""
        if e.control.selected:
            self.edit_mode = list(e.control.selected)[0]
            # Actualizar la interfaz completa para reflejar el cambio
            if hasattr(e, 'page') and e.page:
                e.page.update()
    
    def set_edit_mode(self, mode: str):
        """Establece el modo de edición programáticamente"""
        if mode in ["simple", "advanced"]:
            self.edit_mode = mode
            self._load_config_content()  # Recargar contenido para el nuevo modo
            
            # Si cambiamos a modo simple y es server_settings, cargar contenido en el editor INI
            if mode == "simple" and self.selected_config_type == "server_settings":
                self.ini_simple_editor.load_ini_content(self.config_content.value)
    
    def set_file_type(self, file_type: str):
        """Establece el tipo de archivo de configuración"""
        # Mapear tipos de archivo del control de botones a tipos internos
        type_mapping = {
            "ini": "server_settings",
            "lua": "sandbox_vars", 
            "spawn_regions": "spawn_regions",
            "spawn_points": "server_rules"  # Mapear spawn_points a server_rules por ahora
        }
        
        internal_type = type_mapping.get(file_type, file_type)
        if internal_type in self.config_files:
            self.selected_config_type = internal_type
            self._load_config_content()
    
    def set_server(self, server_id: str):
        """Establece el servidor actual"""
        self.current_server_id = server_id
        self._update_server_info()
        self._update_file_buttons_status()
        self._load_config_content()
    
    def refresh_for_selected_server(self):
        """Actualiza el control para el servidor seleccionado"""
        self.current_server_id = config_loader.get_selected_server()
        self._update_server_info()
        self._update_file_buttons_status()
        self._load_config_content()
    
    def build(self):
        """
        Construye y retorna la interfaz del control de configuración
        """
        # Crear botones de navegación por archivos
        file_buttons = []
        for file_type, file_info in self.config_files.items():
            button = ft.ElevatedButton(
                file_info["name"],
                icon=ft.Icons.DESCRIPTION,
                on_click=lambda e, ft=file_type: self._on_file_type_change(e, ft),
                disabled=not file_info["enabled"],
                style=ft.ButtonStyle(
                    bgcolor=ft.Colors.PRIMARY if file_type == self.selected_config_type else None
                )
            )
            file_buttons.append(button)
        
        # Selector de modo de edición
        mode_selector = ft.Row([
            ft.Text("Modo de edición:", size=14, weight=ft.FontWeight.BOLD),
            ft.SegmentedButton(
                segments=[
                    ft.Segment(
                        value="advanced",
                        label=ft.Text("Avanzado"),
                        icon=ft.Icon(ft.Icons.CODE)
                    ),
                    ft.Segment(
                        value="simple",
                        label=ft.Text("Simple"),
                        icon=ft.Icon(ft.Icons.EDIT)
                    )
                ],
                selected={self.edit_mode},
                on_change=self._on_mode_change
            )
        ])
        
        # Contenedor del editor según el modo
        if self.edit_mode == "advanced":
            editor_content = ft.Container(
                content=self.config_content,
                border=ft.border.all(1, ft.Colors.OUTLINE),
                border_radius=8,
                padding=10,
                expand=True
            )
        else:
            # Modo simple: usar el control específico para INI si es server_settings
            if self.selected_config_type == "server_settings":
                editor_content = self.ini_simple_editor.get_control()
            else:
                editor_content = ft.Container(
                    content=self.simple_editor_container,
                    border=ft.border.all(1, ft.Colors.OUTLINE),
                    border_radius=8,
                    padding=10,
                    expand=True
                )
        
        
        return ft.Column(
            [
                ft.Text(
                    "Gestión de Configuraciones",
                    size=24,
                    weight=ft.FontWeight.BOLD
                ),
                self.server_info_text,
                ft.Divider(),
                
                # Navegación por archivos
                ft.Text("Archivos de configuración:", size=16, weight=ft.FontWeight.BOLD),
                ft.Row(file_buttons, wrap=True),
                
                ft.Divider(),
                
                # Selector de modo de edición
                mode_selector,
                
                ft.Container(height=10),
                
                # Editor de configuración
                editor_content,
                
                # Botones de acción
                ft.Row(
                    [
                        ft.ElevatedButton(
                            "Guardar Cambios",
                            icon=ft.Icons.SAVE,
                            on_click=self._save_config,
                            bgcolor=ft.Colors.BLUE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Restaurar",
                            icon=ft.Icons.RESTORE,
                            on_click=self._reset_config,
                            bgcolor=ft.Colors.ORANGE,
                            color=ft.Colors.WHITE
                        ),
                        ft.ElevatedButton(
                            "Validar Sintaxis",
                            icon=ft.Icons.CHECK_CIRCLE,
                            bgcolor=ft.Colors.GREEN,
                            color=ft.Colors.WHITE
                        ),
                    ],
                    spacing=10
                ),
            ],
            spacing=10,
            expand=True
        )