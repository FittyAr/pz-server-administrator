import flet as ft
import configparser
import os
import json
from typing import Optional, Dict, Any

class IniSimpleEditorControl:
    """Control para edición simple de archivos INI con interfaz amigable"""
    
    def __init__(self):
        self.config_data = {}
        self.controls_map = {}  # Mapeo de controles para obtener valores
        self.help_data = {}  # Datos de ayuda para parámetros
        self.container = ft.Container()
        self._load_help_data()
        self._create_editor()
    
    def _create_editor(self):
        """Crear el editor simple inicial"""
        self.container = ft.Container(
            content=ft.Column([
                ft.Container(
                    content=ft.Text(
                        "No hay contenido para mostrar",
                        text_align=ft.TextAlign.CENTER,
                        color=ft.Colors.ON_SURFACE_VARIANT
                    ),
                    alignment=ft.alignment.center,
                    height=200
                )
            ], scroll=ft.ScrollMode.AUTO),
            padding=10,
            border=ft.border.all(1, ft.Colors.OUTLINE),
            border_radius=8,
            expand=True
        )
    
    def _load_help_data(self):
        """Cargar datos de ayuda desde el archivo JSON"""
        try:
            help_file_path = os.path.join(os.path.dirname(__file__), '..', 'assets', 'parameter_help.json')
            if os.path.exists(help_file_path):
                with open(help_file_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    self.help_data = data.get('server_parameters', {})
        except Exception as e:
            print(f"Error loading help data: {e}")
            self.help_data = {}
    
    def load_ini_content(self, content: str):
        """Cargar contenido INI y crear controles de edición"""
        if not content or content.strip() == "":
            self._create_editor()
            return
        
        try:
            self.config_data = {}
            self.controls_map = {}
            
            # Verificar si el contenido tiene headers de sección
            has_sections = any(line.strip().startswith('[') and line.strip().endswith(']') 
                             for line in content.split('\n'))
            
            if has_sections:
                # Parsear como INI normal con secciones
                config = configparser.ConfigParser(allow_no_value=True)
                config.read_string(content)
                
                # Convertir a diccionario
                for section_name in config.sections():
                    self.config_data[section_name] = dict(config[section_name])
            else:
                # Tratar como archivo de configuración sin secciones (crear sección DEFAULT)
                lines = content.strip().split('\n')
                default_section = {}
                for line in lines:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        default_section[key.strip()] = value.strip()
                if default_section:
                    self.config_data['DEFAULT'] = default_section
            
            self._create_controls()
            
        except Exception as e:
            self._show_error(f"Error al parsear el archivo INI: {str(e)}")
    
    def _create_controls(self):
        """Crear controles de edición basados en los datos del INI"""
        if not self.config_data:
            self._create_editor()
            return
        
        controls = []
        
        for section_name, section_data in self.config_data.items():
            # Título de la sección
            if section_name != 'DEFAULT':
                section_title = ft.Container(
                    content=ft.Text(
                        f"[{section_name}]",
                        size=16,
                        weight=ft.FontWeight.BOLD,
                        color=ft.Colors.PRIMARY
                    ),
                    margin=ft.margin.only(top=20, bottom=10)
                )
                controls.append(section_title)
            
            # Controles para cada configuración
            for key, value in section_data.items():
                control_row = self._create_config_row(section_name, key, value)
                controls.append(control_row)
        
        if not controls:
            controls.append(
                ft.Container(
                    content=ft.Text(
                        "No se encontraron configuraciones válidas en el archivo",
                        text_align=ft.TextAlign.CENTER,
                        color=ft.Colors.ON_SURFACE_VARIANT
                    ),
                    alignment=ft.alignment.center,
                    height=200
                )
            )
        
        self.container.content = ft.Column(controls, scroll=ft.ScrollMode.AUTO)
    
    def _create_config_row(self, section: str, key: str, value: str):
        """Crear una fila de configuración con etiqueta y control de entrada"""
        control_key = f"{section}.{key}"
        
        # Crear el control apropiado según el tipo de valor
        input_control = self._create_input_control(key, value, control_key)
        
        # Guardar referencia del control
        self.controls_map[control_key] = input_control
        
        # Crear icono de ayuda si hay información disponible
        help_icon = self._create_help_icon(key)
        
        # Crear la fila con label, control de entrada e icono de ayuda
        row_controls = [
            ft.Container(
                content=ft.Text(
                    key,
                    size=14,
                    weight=ft.FontWeight.W_500,
                    color=ft.Colors.ON_SURFACE
                ),
                width=180,
                alignment=ft.alignment.center_left
            ),
            ft.Container(
                content=input_control,
                expand=True
            )
        ]
        
        # Agregar icono de ayuda si existe
        if help_icon:
            row_controls.append(
                ft.Container(
                    content=help_icon,
                    width=30,
                    alignment=ft.alignment.center
                )
            )
        
        return ft.Container(
            content=ft.Row(row_controls, spacing=10),
            padding=ft.padding.symmetric(vertical=5, horizontal=10),
            border_radius=4,
            bgcolor=ft.Colors.SURFACE_CONTAINER_HIGHEST if len(self.controls_map) % 2 == 0 else None
        )
    
    def _create_help_icon(self, key: str):
        """Crear icono de ayuda con tooltip para un parámetro"""
        key_lower = key.lower()
        
        # Buscar información de ayuda para este parámetro
        help_info = self.help_data.get(key_lower)
        if not help_info:
            return None
        
        # Crear el tooltip con la información de ayuda
        tooltip_text = f"{help_info.get('title', key)}\n\n{help_info.get('description', 'Sin descripción disponible.')}"
        
        return ft.IconButton(
            icon=ft.Icons.HELP_OUTLINE,
            icon_size=16,
            icon_color=ft.Colors.PRIMARY,
            tooltip=tooltip_text,
            style=ft.ButtonStyle(
                padding=ft.padding.all(4),
                shape=ft.CircleBorder()
            )
        )
    
    def _create_input_control(self, key: str, value: str, control_key: str):
        """Crear el control de entrada apropiado según el tipo de configuración"""
        key_lower = key.lower()
        value_lower = value.lower().strip()
        
        # Valores booleanos
        if value_lower in ['true', 'false']:
            return ft.Switch(
                value=value_lower == 'true',
                data=control_key,
                label=value_lower.title()
            )
        
        # Valores numéricos con validación
        if key_lower in ['maxplayers', 'port', 'pinglimit', 'pauseempty', 'zombieslore', 'zombiespopulation']:
            try:
                int(value)  # Verificar que es numérico
                return ft.TextField(
                    value=value,
                    data=control_key,
                    input_filter=ft.NumbersOnlyInputFilter(),
                    width=150,
                    border_radius=4
                )
            except ValueError:
                pass
        
        # Campos de texto largos
        if key_lower in ['publicdescription', 'description']:
            return ft.TextField(
                value=value,
                data=control_key,
                multiline=True,
                max_lines=3,
                border_radius=4
            )
        
        # Campos de texto con opciones conocidas
        if key_lower in ['difficulty', 'gamemode']:
            return ft.Dropdown(
                value=value,
                data=control_key,
                options=self._get_dropdown_options(key_lower),
                width=200
            )
        
        # Por defecto, campo de texto simple
        return ft.TextField(
            value=value,
            data=control_key,
            border_radius=4
        )
    
    def _get_dropdown_options(self, key: str):
        """Obtener opciones para dropdowns según la clave"""
        options_map = {
            'difficulty': [
                ft.dropdown.Option("VeryEasy", "Muy Fácil"),
                ft.dropdown.Option("Easy", "Fácil"),
                ft.dropdown.Option("Normal", "Normal"),
                ft.dropdown.Option("Hard", "Difícil"),
                ft.dropdown.Option("VeryHard", "Muy Difícil")
            ],
            'gamemode': [
                ft.dropdown.Option("Survival", "Supervivencia"),
                ft.dropdown.Option("Creative", "Creativo"),
                ft.dropdown.Option("Sandbox", "Sandbox")
            ]
        }
        return options_map.get(key, [])
    
    def _show_error(self, message: str):
        """Mostrar mensaje de error"""
        self.container.content = ft.Column([
            ft.Container(
                content=ft.Row([
                    ft.Icon(ft.Icons.ERROR, color=ft.Colors.RED),
                    ft.Text(
                        message,
                        color=ft.Colors.RED,
                        size=14
                    )
                ], spacing=10),
                padding=20,
                alignment=ft.alignment.center
            )
        ])
    
    def get_ini_content(self) -> str:
        """Obtener el contenido INI desde los controles de edición"""
        if not self.controls_map:
            return ""
        
        # Organizar por secciones
        sections = {}
        
        for control_key, control in self.controls_map.items():
            section, key = control_key.split('.', 1)
            
            if section not in sections:
                sections[section] = {}
            
            # Obtener valor según el tipo de control
            if isinstance(control, ft.Switch):
                value = 'true' if control.value else 'false'
            elif isinstance(control, (ft.TextField, ft.Dropdown)):
                value = str(control.value) if control.value is not None else ""
            else:
                value = ""
            
            sections[section][key] = value
        
        # Generar contenido INI
        lines = []
        
        for section_name, section_data in sections.items():
            if section_name != 'DEFAULT':
                lines.append(f"[{section_name}]")
            
            for key, value in section_data.items():
                lines.append(f"{key}={value}")
            
            if section_name != 'DEFAULT':
                lines.append("")  # Línea vacía entre secciones
        
        return '\n'.join(lines)
    
    def get_control(self):
        """Obtener el control principal"""
        return self.container
    
    def clear(self):
        """Limpiar el editor"""
        self.config_data = {}
        self.controls_map = {}
        self._create_editor()